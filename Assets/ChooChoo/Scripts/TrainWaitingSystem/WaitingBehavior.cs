using Bindito.Core;
using System;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using UnityEngine;

namespace ChooChoo
{
  public class WaitingBehavior : RootBehavior
  {
    private BlockService _blockService;
    private RandomTrainWaitingLocationPicker _randomTrainWaitingLocationPicker;
    private MoveToStationExecutor _moveToStationExecutor;
    private WaitExecutor _waitExecutor;
    private TrainWaitingLocation _currentWaitingLocation;

    [Inject]
    public void InjectDependencies(BlockService blockService, RandomTrainWaitingLocationPicker randomTrainWaitingLocationPicker)
    {
      _blockService = blockService;
      _randomTrainWaitingLocationPicker = randomTrainWaitingLocationPicker;
    }
    
    public void Awake()
    {
      _moveToStationExecutor = GetComponent<MoveToStationExecutor>();
      _waitExecutor = GetComponent<WaitExecutor>();
    }

    public override Decision Decide(GameObject agent)
    {
      var start = transform.position;
      var currentLocation = _blockService.GetFloorObjectComponentAt<TrainDestination>(Vector3Int.FloorToInt(new Vector3(start.x, start.z, start.y)));
      
      if (_currentWaitingLocation != null && currentLocation == _currentWaitingLocation.TrainDestinationComponent)
      {
        _waitExecutor.LaunchForSpecifiedTime(0.375f);
        return Decision.ReleaseWhenFinished(_waitExecutor);
      }

      return GoToClosestWaitingLocation();
    }

    private Decision GoToClosestWaitingLocation()
    {
      if (_currentWaitingLocation != null)
        _currentWaitingLocation.Occupied = false;
      
      _currentWaitingLocation = _randomTrainWaitingLocationPicker.RandomWaitingLocation();

      if (_currentWaitingLocation == null)
        return Decision.ReleaseNow();

      _currentWaitingLocation.Occupied = true;
      Plugin.Log.LogWarning( "Waiting at random location");
      switch (_moveToStationExecutor.Launch(_currentWaitingLocation.TrainDestinationComponent))
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
