using Bindito.Core;
using System;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.EntitySystem;
using UnityEngine;

namespace ChooChoo
{
  public class WaitingBehavior : RootBehavior, IDeletableEntity
  {
    private BlockService _blockService;
    private ClosestTrainWaitingLocationPicker _closestTrainWaitingLocationPicker;
    private MoveToStationExecutor _moveToStationExecutor;
    private WaitExecutor _waitExecutor;
    private TrainWaitingLocation _currentWaitingLocation;

    [Inject]
    public void InjectDependencies(BlockService blockService, ClosestTrainWaitingLocationPicker closestTrainWaitingLocationPicker)
    {
      _blockService = blockService;
      _closestTrainWaitingLocationPicker = closestTrainWaitingLocationPicker;
    }
    
    public void Awake()
    {
      _moveToStationExecutor = GetComponent<MoveToStationExecutor>();
      _waitExecutor = GetComponent<WaitExecutor>();
    }

    public override Decision Decide(GameObject agent)
    {
      var currentTrainDestination = _blockService.GetFloorObjectComponentAt<TrainDestination>(transform.position.ToBlockServicePosition());
      
      if (_currentWaitingLocation != null && currentTrainDestination == _currentWaitingLocation.TrainDestinationComponent)
      {
        _waitExecutor.LaunchForSpecifiedTime(0.375f);
        return Decision.ReleaseWhenFinished(_waitExecutor);
      }

      return GoToClosestWaitingLocation();
    }
    
    public void DeleteEntity()
    {
      if (_currentWaitingLocation != null)
        _currentWaitingLocation.Occupied = false;
    }

    private Decision GoToClosestWaitingLocation()
    {
      if (_currentWaitingLocation != null)
        _currentWaitingLocation.Occupied = false;
      
      _currentWaitingLocation = _closestTrainWaitingLocationPicker.ClosestWaitingLocation(transform.position);
      if (_currentWaitingLocation == null)
        return Decision.ReleaseNow();

      _currentWaitingLocation.Occupied = true;
      // Plugin.Log.LogWarning( "Waiting at random location");
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
