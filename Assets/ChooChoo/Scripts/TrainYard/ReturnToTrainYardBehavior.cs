using Bindito.Core;
using System;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class ReturnToTrainYardBehavior : RootBehavior
  {
    private BlockService _blockService;
    private MoveToStationExecutor _moveToStationExecutor;
    private WaitExecutor _waitExecutor;
    private TrainYardSubject _trinYardSubject;

    [Inject]
    public void InjectDependencies(BlockService blockService)
    {
      _blockService = blockService;
    }
    
    public void Awake()
    {
      _moveToStationExecutor = GetComponent<MoveToStationExecutor>();
      _waitExecutor = GetComponent<WaitExecutor>();
      _trinYardSubject = GetComponent<TrainYardSubject>();
    }

    public override Decision Decide(GameObject agent)
    {
      var currentTrainDestination = _blockService.GetFloorObjectComponentAt<TrainDestination>(transform.position.ToBlockServicePosition());
      if (currentTrainDestination == _trinYardSubject.HomeTrainYard)
      {
        _waitExecutor.LaunchForSpecifiedTime(1);
        return Decision.ReleaseWhenFinished(_waitExecutor);
      }
    
      return ReturnToOriginalTrainYard();
    }

    private Decision ReturnToOriginalTrainYard()
    {
     // Plugin.Log.LogWarning( "Returning Home");
      switch (_moveToStationExecutor.Launch(_trinYardSubject.HomeTrainYard))
      {
        case ExecutorStatus.Success:
          return Decision.ReleaseNow();
        case ExecutorStatus.Failure:
          return Decision.ReleaseNow();
        case ExecutorStatus.Running:
          return Decision.ReturnWhenFinished(_moveToStationExecutor);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
