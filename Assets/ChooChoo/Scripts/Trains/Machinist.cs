using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BehaviorSystem;
using Timberborn.Common;
using Timberborn.Persistence;
using Timberborn.TickSystem;
using Timberborn.WalkingSystem;
using UnityEngine;

namespace ChooChoo
{
  public class Machinist : TickableComponent, IPersistentEntity
  {
    private static readonly ComponentKey MachinistKey = new(nameof (Machinist));
    private static readonly PropertyKey<ITrainDestination> CurrentDestinationKey = new("CurrentDestination");
    private TrackFollowerFactory _trackFollowerFactory;
    private TrainDestinationObjectSerializer _trainDestinationObjectSerializer;
    private WalkerSpeedManager _walkerSpeedManager;
    private TrainWagonManager _trainWagonManager;
    private TrackFollower _trackFollower;
    private readonly List<TrackConnection> _pathConnections = new(100);
    private readonly List<TrackConnection> _tempPathCorners = new(100);
    private ITrainDestination _currentTrainDestination;
    private ITrainDestination _previousTrainDestination;
    private TrackConnection _lastTrackConnection;

    public event EventHandler<StartedNewPathEventArgs> StartedNewPath;

    public bool CurrentDestinationReachable { get; private set; }

    public IReadOnlyList<TrackConnection> PathCorners { get; private set; }

    [Inject]
    public void InjectDependencies(
      TrackFollowerFactory trackFollowerFactory,
      TrainDestinationObjectSerializer trainDestinationObjectSerializer
      )
    {
      _trackFollowerFactory = trackFollowerFactory;
      _trainDestinationObjectSerializer = trainDestinationObjectSerializer;
    }

    public void Awake()
    {
      _walkerSpeedManager = GetComponent<WalkerSpeedManager>();
      _trainWagonManager = GetComponent<TrainWagonManager>();
      _trackFollower = _trackFollowerFactory.Create(gameObject);
      PathCorners = _pathConnections.AsReadOnly();
    }

    public override void Tick()
    {
      if (Stopped())
        return;
      if (_trackFollower.ReachedLastPathCorner())
        Stop();
      else
        Move();
    }

    public ExecutorStatus GoTo(ITrainDestination trainDestination)
    {
      _previousTrainDestination = _currentTrainDestination;
      int path = (int) FindPath(trainDestination);
      if (path != 2)
        return (ExecutorStatus) path;
      return (ExecutorStatus) path;
    }

    public void Stop()
    {
      _previousTrainDestination = _currentTrainDestination;
      _currentTrainDestination = null;
      _trackFollower.StopMoving();
      _trainWagonManager.StopWagons();
      _lastTrackConnection = null;
    }
    
    public bool Stopped() => _currentTrainDestination == null;

    public void RefreshPath()
    {
      _previousTrainDestination = null;
      if (_currentTrainDestination == null)
        return;
      FindPath(_currentTrainDestination);
    }
    
    public void Save(IEntitySaver entitySaver)
    {
      IObjectSaver component = entitySaver.GetComponent(MachinistKey);
      if (_currentTrainDestination != null)
        component.Set(CurrentDestinationKey, _currentTrainDestination, _trainDestinationObjectSerializer);
    }

    public void Load(IEntityLoader entityLoader)
    {
      IObjectLoader component = entityLoader.GetComponent(MachinistKey);
      if (component.Has(CurrentDestinationKey))
        _currentTrainDestination = component.Get(CurrentDestinationKey, _trainDestinationObjectSerializer);
    }

    private ExecutorStatus FindPath(ITrainDestination trainDestination)
    {
      if (!HasSavedPathToDestination(trainDestination))
      {
        Vector3 start = transform.position;
        _pathConnections.Clear();
        CurrentDestinationReachable = trainDestination.GeneratePath(start, ref _lastTrackConnection, _tempPathCorners);
        if (CurrentDestinationReachable)
        {
          if (!_pathConnections.IsEmpty())
            _pathConnections.RemoveLast();
          _pathConnections.AddRange(_tempPathCorners);
          _tempPathCorners.Clear();
          _lastTrackConnection = _pathConnections.Last();
        }
        else
          _pathConnections.Clear();
        _trackFollower.StartMovingAlongPath(_pathConnections);
        EventHandler<StartedNewPathEventArgs> startedNewPath = StartedNewPath;
        if (startedNewPath != null)
          startedNewPath(this, new StartedNewPathEventArgs(100));
      }
      if (CurrentDestinationReachable)
      {
        _currentTrainDestination = trainDestination;
        return !_trackFollower.ReachedLastPathCorner() ? ExecutorStatus.Running : ExecutorStatus.Success;
      }
      Stop();
      return ExecutorStatus.Failure;
    }

    private bool HasSavedPathToDestination(ITrainDestination trainDestination) => Equals(_previousTrainDestination, trainDestination);
    
    private void Move()
    {
      var speed = _walkerSpeedManager.Speed * CalculateSpeedReductionAtStartAndEnd();
      var time = Time.fixedDeltaTime;
      if (_trackFollower.MoveAlongPath(time, "Walking", speed))
        _trainWagonManager.MoveWagons(_trackFollower.PreviouslyAnimatedPathCorners, time, speed + 0.06f);
    }
    
    private float CalculateSpeedReductionAtStartAndEnd()
    {
      var start = CalculateSlowdown(_pathConnections[0].PathCorners[0]);
      var end = CalculateSlowdown(_pathConnections.Last().PathCorners[0]);
    
      return start * end;
    }
    
    private float CalculateSlowdown(Vector3 position)
    {
      var distanceFromStart = Vector3.Distance(transform.position, position);
      if (distanceFromStart > 1.5f)
        return 1;

      return distanceFromStart / 1.5f + 0.1f;
    }
  }
}
