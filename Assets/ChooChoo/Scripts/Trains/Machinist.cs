using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BehaviorSystem;
using Timberborn.Common;
using Timberborn.Navigation;
using Timberborn.Persistence;
using Timberborn.TickSystem;
using Timberborn.TimeSystem;
using Timberborn.WalkingSystem;
using UnityEngine;

namespace ChooChoo
{
  public class Machinist : TickableComponent, IPersistentEntity
  {
    private static readonly ComponentKey PilotKey = new(nameof (Machinist));
    private static readonly PropertyKey<ITrainDestination> CurrentDestinationKey = new("CurrentDestination");
    private static readonly float SecondsPerDistanceUnit = 1f;
    private IDayNightCycle _dayNightCycle;
    private TrackFollowerFactory _trackFollowerFactory;
    private TrainDestinationObjectSerializer _trainDestinationObjectSerializer;
    private WalkerSpeedManager _walkerSpeedManager;
    private TrackFollower _trackFollower;
    private readonly List<TrackConnection> _pathTracks = new(100);
    private readonly List<TrackConnection> _tempPathCorners = new(100);
    private ITrainDestination _currentTrainDestination;
    private ITrainDestination _previousTrainDestination;

    public event EventHandler<StartedNewPathEventArgs> StartedNewPath;

    public bool CurrentDestinationReachable { get; private set; }

    public IReadOnlyList<TrackConnection> PathCorners { get; private set; }

    public BoundingBox CurrentPathBounds { get; private set; }

    [Inject]
    public void InjectDependencies(
      IDayNightCycle dayNightCycle,
      TrackFollowerFactory trackFollowerFactory,
      TrainDestinationObjectSerializer trainDestinationObjectSerializer
      )
    {
      _dayNightCycle = dayNightCycle;
      _trackFollowerFactory = trackFollowerFactory;
      _trainDestinationObjectSerializer = trainDestinationObjectSerializer;
    }

    public void Awake()
    {
      _walkerSpeedManager = GetComponent<WalkerSpeedManager>();
      _trackFollower = _trackFollowerFactory.Create(gameObject);
      PathCorners = _pathTracks.AsReadOnly();
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
      IObjectSaver component = entitySaver.GetComponent(PilotKey);
      if (_currentTrainDestination != null)
        component.Set(CurrentDestinationKey, _currentTrainDestination, _trainDestinationObjectSerializer);
    }

    public void Load(IEntityLoader entityLoader)
    {
      IObjectLoader component = entityLoader.GetComponent(PilotKey);
      if (component.Has(CurrentDestinationKey))
      {
        _currentTrainDestination = component.Get(CurrentDestinationKey, _trainDestinationObjectSerializer);
        FindPath(_currentTrainDestination);
      }
    }

    private ExecutorStatus FindPath(ITrainDestination trainDestination)
    {
      if (!HasSavedPathToDestination(trainDestination))
      {
        Vector3 start = transform.position;
        _pathTracks.Clear();
        CurrentDestinationReachable = trainDestination.GeneratePath(start, _tempPathCorners);
        if (CurrentDestinationReachable)
        {
          if (!_pathTracks.IsEmpty())
            _pathTracks.RemoveLast();
          _pathTracks.AddRange(_tempPathCorners);
          _tempPathCorners.Clear();
        }
        else
          _pathTracks.Clear();
        _trackFollower.StartMovingAlongPath(_pathTracks);
        EventHandler<StartedNewPathEventArgs> startedNewPath = StartedNewPath;
        if (startedNewPath != null)
          startedNewPath(this, new StartedNewPathEventArgs(100));
      }
      if (CurrentDestinationReachable)
      {
        _currentTrainDestination = trainDestination;
        RecalculatePathBounds();
        return !_trackFollower.ReachedLastPathCorner() ? ExecutorStatus.Running : ExecutorStatus.Success;
      }
      Stop();
      return ExecutorStatus.Failure;
    }
    
    private void RecalculatePathBounds()
    {
      BoundingBox.Builder builder = new BoundingBox.Builder();
      for (int index = 0; index < _pathTracks.Count; ++index)
        builder.Expand(NavigationCoordinateSystem.WorldToGridInt(_pathTracks[index].PathCorners[0]));
      CurrentPathBounds = builder.Build();
    }
    
    private bool HasSavedPathToDestination(ITrainDestination trainDestination) => Equals(_previousTrainDestination, trainDestination);
    
    private void Move()
    {
      var speed = _walkerSpeedManager.Speed * CalculateSpeedReductionAtStartAndEnd();
      _trackFollower.MoveAlongPath(Time.fixedDeltaTime, "Walking", speed);
    }
    
    private float CalculateSpeedReductionAtStartAndEnd()
    {
      var start = SlowdownStart();
      var end = SlowdownEnd();
    
      return start * end;
    }
    
    private float SlowdownStart()
    {
      var distanceFromStart = Vector3.Distance(_pathTracks[0].PathCorners[0], transform.position);
      if (distanceFromStart > 3)
        return 1;

      return distanceFromStart / 3 + 0.2f;
    }
    
    private float SlowdownEnd()
    {
      var distanceToEnd = Vector3.Distance(transform.position, _pathTracks.Last().PathCorners[0]);
      if (distanceToEnd > 3)
        return 1;

      return distanceToEnd / 3 + 0.2f;
    }
  }
}
