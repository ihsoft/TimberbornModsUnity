using Bindito.Core;
using System;
using Timberborn.BehaviorSystem;
using Timberborn.Common;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class TrainScheduleBehavior : RootBehavior, IPersistentEntity
  {
    private static readonly ComponentKey TrainScheduleBehaviorKey = new(nameof (TrainScheduleBehavior));
    private static readonly PropertyKey<bool> ExecuteActionsKey = new("ExecuteActions");
    private RandomTrainDestinationPicker _randomTrainDestinationPicker;
    private IRandomNumberGenerator _randomNumberGenerator;
    private TrainScheduleController _trainScheduleController;
    private MoveToStationExecutor _moveToStationExecutor;
    private Vector3 _globalMarketPosition;
    private bool _executeActions;
    private int _currentStationIndex = -1;
    private int _currentTrainActionIndex;
    
    [Inject]
    public void InjectDependencies(
      RandomTrainDestinationPicker randomTrainDestinationPicker, 
      IRandomNumberGenerator randomNumberGenerator)
    {
      _randomTrainDestinationPicker = randomTrainDestinationPicker;
      _randomNumberGenerator = randomNumberGenerator;
    }

    public void Awake()
    {
      _trainScheduleController = GetComponent<TrainScheduleController>();
      _moveToStationExecutor = GetComponent<MoveToStationExecutor>();
    }

    public override Decision Decide(GameObject agent)
    {
      if (_trainScheduleController.TrainSchedule.Count < 2)
        return Decision.ReleaseNow();
      
      if (!_executeActions)
      {
        return GoToNextStation();
      }

      return ExecuteAction();
    }

    public void Save(IEntitySaver entitySaver)
    {
      entitySaver.GetComponent(TrainScheduleBehaviorKey).Set(ExecuteActionsKey, _executeActions);
    }

    public void Load(IEntityLoader entityLoader)
    {
      _executeActions = entityLoader.GetComponent(TrainScheduleBehaviorKey).Get(ExecuteActionsKey);
    }

    private Decision GoToNextStation()
    {
      _executeActions = true;
      
      // var randomLocation = _randomTrainDestinationPicker.RandomTrainDestination();
      
      _currentStationIndex += 1;
      
      if (_currentStationIndex >= _trainScheduleController.TrainSchedule.Count)
        _currentStationIndex = 0;
      
      var nextStation = _trainScheduleController.TrainSchedule[_currentStationIndex].Station;

      // Plugin.Log.LogWarning( "Go to next station");
      switch (_moveToStationExecutor.Launch(nextStation))
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

    private Decision ExecuteAction()
    {
      if (LastActionExecuted())
      {
        _currentTrainActionIndex = 0;
        _executeActions = false;
        return Decision.ReleaseNow();
      }

      var trainAction = _trainScheduleController.TrainSchedule[_currentStationIndex].Actions[_currentTrainActionIndex];
      
      _currentTrainActionIndex += 1;

      return trainAction.ExecuteAction();
    }
    
    private bool LastActionExecuted() => _currentTrainActionIndex >= _trainScheduleController.TrainSchedule[_currentStationIndex].Actions.Count;
  }
}
