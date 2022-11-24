using Bindito.Core;
using System;
using Timberborn.BehaviorSystem;
using Timberborn.Common;
using Timberborn.Persistence;
using Timberborn.WorkSystem;
using UnityEngine;

namespace ChooChoo
{
  public class TrainMovingRootBehavior : RootBehavior, IPersistentEntity
  {
    private static readonly ComponentKey FlyingRootBehaviorKey = new(nameof (TrainMovingRootBehavior));
    private static readonly PropertyKey<bool> FlownKey = new("Flown");
    private static readonly PropertyKey<bool> IsFlyingKey = new("IsFlying");
    private static readonly PropertyKey<bool> IsReturningKey = new("IsReturning");
    private static readonly PropertyKey<bool> IsReturnedKey = new("IsReturned");
    private RandomTrainDestinationPicker _randomTrainDestinationPicker;
    private IRandomNumberGenerator _randomNumberGenerator;
    private WorkingHoursManager _workingHoursManager;
    private MoveToPositionExecutor _flyToPositionExecutor;
    private WaitExecutor _waitExecutor;
    private Vector3 _globalMarketPosition;
    private bool _flown;
    private bool _isFlying;
    private bool _isReturning;
    public bool IsReturned { get; private set; }
    
    [Inject]
    public void InjectDependencies(
      RandomTrainDestinationPicker randomTrainDestinationPicker, 
      IRandomNumberGenerator randomNumberGenerator, 
      WorkingHoursManager workingHoursManager)
    {
      _randomTrainDestinationPicker = randomTrainDestinationPicker;
      _randomNumberGenerator = randomNumberGenerator;
      _workingHoursManager = workingHoursManager;
    }

    public void Awake()
    {
      _flyToPositionExecutor = GetComponent<MoveToPositionExecutor>();
      _waitExecutor = GetComponent<WaitExecutor>();
    }

    private void Start()
    {
      var globalMarketServant = GetComponent<TrainYardSubject>();
      _globalMarketPosition = globalMarketServant.LinkedGlobalMarketPosition;
    }

    public override Decision Decide(GameObject agent)
    {
      // if (_workingHoursManager.AreWorkingHours)
      // {
        return MakeDecisionDuringWorkHours();
      // }
      //
      // return MakeDecisionAfterWorkHours(agent);
    }

    public void Save(IEntitySaver entitySaver)
    {
      entitySaver.GetComponent(FlyingRootBehaviorKey).Set(FlownKey, _flown);
      entitySaver.GetComponent(FlyingRootBehaviorKey).Set(IsFlyingKey, _isFlying);
      entitySaver.GetComponent(FlyingRootBehaviorKey).Set(IsReturningKey, _isReturning);
      entitySaver.GetComponent(FlyingRootBehaviorKey).Set(IsReturnedKey, IsReturned);
    }

    public void Load(IEntityLoader entityLoader)
    {
      _flown = entityLoader.GetComponent(FlyingRootBehaviorKey).Get(FlownKey);
      _isFlying = entityLoader.GetComponent(FlyingRootBehaviorKey).Get(IsFlyingKey);
      _isReturning = entityLoader.GetComponent(FlyingRootBehaviorKey).Get(IsReturningKey);
      IsReturned = entityLoader.GetComponent(FlyingRootBehaviorKey).Get(IsReturnedKey);
    }

    private Decision MakeDecisionDuringWorkHours()
    {
      if (!_flown)
      {
        _flown = true;
        return MoveToRandomDestination();
      }
        
      _waitExecutor.LaunchForSpecifiedTime(_randomNumberGenerator.Range(0.1f, 0.3f));
      _flown = false;
      return Decision.ReleaseWhenFinished(_waitExecutor);
    }
    
    private Decision MakeDecisionAfterWorkHours(GameObject agent)
    {

      IsReturned = true;
      _waitExecutor.LaunchForSpecifiedTime(1.5f);
      return Decision.ReleaseWhenFinished(_waitExecutor);
    }

    private Decision ReturnToGlobalMarketAtSameHeight(float height)
    {
      var globalMarketPositionAtSomeHeight = _globalMarketPosition + new Vector3(0, height, 0);
    
      return StartFlyToPositionExecutor(globalMarketPositionAtSomeHeight);
    }
    
    private Decision MoveToRandomDestination()
    {
      var randomLocation = _randomTrainDestinationPicker.RandomTrainDestination();
      
      return StartFlyToPositionExecutor(randomLocation);
    }

    private Decision StartFlyToPositionExecutor(Vector3 coordinates)
    {
      Plugin.Log.LogWarning( "goto "+coordinates);
      switch (_flyToPositionExecutor.Launch(coordinates))
      {
        case ExecutorStatus.Success:
          return Decision.ReleaseNow();
        case ExecutorStatus.Failure:
          return Decision.ReleaseNow();
        case ExecutorStatus.Running:
          return Decision.ReturnWhenFinished(_flyToPositionExecutor);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
