using System.Collections.Generic;
using System.Linq;
using Timberborn.CharacterMovementSystem;
using Timberborn.Navigation;
using UnityEngine;

namespace ChooChoo
{
  public class TrackFollower : ITrackFollower
  {
    private readonly INavigationService _navigationService;
    private readonly TrackSectionService _trackSectionService;
    private readonly TrainNavigationService _trainNavigationService;
    private readonly MovementAnimator _movementAnimator;
    private readonly Transform _transform;
    private readonly Machinist _machinist;
    private readonly List<PathCorner> _animatedPathCorners = new(100);
    private List<TrackConnection> _pathCorners;
    private int _pathCornersCount;
    private int _currentCornerIndex;
    private int _nextSubCornerIndex;
    private TrackSection _trackSection;
    private static readonly float ExtraPathCornersMultiplier = 2.5f;

    public int CurrentCornerIndex => _currentCornerIndex;

    public TrackFollower(
      INavigationService navigationService,
      TrainNavigationService trainNavigationService,
      TrackSectionService trackSectionService,
      MovementAnimator movementAnimator,
      Transform transform,
      Machinist machinist)
    {
      _navigationService = navigationService;
      _trainNavigationService = trainNavigationService;
      _trackSectionService = trackSectionService;
      _movementAnimator = movementAnimator;
      _transform = transform;
      _machinist = machinist;
    }

    public void StartMovingAlongPath(List<TrackConnection> pathCorners)
    {
      _pathCorners = pathCorners;
      _currentCornerIndex = 1;
      _nextSubCornerIndex = 0;
    }

    public bool MoveAlongPath(float deltaTime, string animationName, float movementSpeed)
    {
      _animatedPathCorners.Clear();
      float time = Time.time;
      _animatedPathCorners.Add(new PathCorner(_transform.position, time));
      float num = deltaTime;
      while (num > 0.0 && !ReachedLastPathCorner())
      {
        if (!CanEnterNextSection())
        {
          _animatedPathCorners.Clear();
          return false;
        }
        
        _nextSubCornerIndex = PeekNextSubCornerIndex();
        Vector3 position;
        (position, num) = MoveInDirection(_transform.position, _pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex], movementSpeed, num);
        _transform.position = position;
        float timeInSeconds = time + deltaTime - num;
        _animatedPathCorners.Add(new PathCorner(position, timeInSeconds));
      }
      if (!ReachedLastPathCorner())
        _animatedPathCorners.Add(new PathCorner(_pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex], (float) ((double) time + deltaTime + 1.0)));
      _movementAnimator.AnimateMovementAlongPath(_animatedPathCorners, animationName, movementSpeed);
      return true;
    }

    public void StopMoving()
    {
      // _pathCorners = null;
      _movementAnimator.StopAnimatingMovement();
    }

    public bool ReachedLastPathCorner() => _navigationService.InStoppingProximity(_pathCorners.Last().PathCorners.Last(), _transform.position);

    public void ResetTrackSection()
    {
      if (_trackSection == null) 
        return;
      _trackSection.Leave();
      _trackSection = null;
    }

    public TrackConnection LastTraversedTrackConnection()
    {
      return _pathCorners[_currentCornerIndex > 1 ? _currentCornerIndex - 2 : _currentCornerIndex - 1];
    }

    private bool CanEnterNextSection()
    {
      var nextCornerToCheckIndex = -1;
      
      while (true)
      {
        TrackPiece trackPiece = _pathCorners[_currentCornerIndex + nextCornerToCheckIndex].ConnectedTrackPiece;
        nextCornerToCheckIndex += 1;
      
        if (trackPiece == null)
          return false;

        if (trackPiece.TryGetComponent(out TrackIntersection _))
          continue;

        TrackSection trackSection = trackPiece.TrackSection;
        
        var flag = trackSection.Equals(_trackSection);
        if (flag)
          return true;

        if (trackSection.Occupied)
        {
          if (trackSection.HasWaitingTrain)
          {
            _machinist.RefreshPath();
            return false;
          }
          trackSection.HasWaitingTrain = true;
          return false;
        }

        OccupyNextTrackSection(trackSection);
        return true;
      }
    }

    private void OccupyNextTrackSection(TrackSection trackSection)
    {
      _trackSection?.Leave();
      _trackSection = trackSection;
      _trackSection.Enter();
    }


    private bool LastOfSubCorners() => _nextSubCornerIndex >= _pathCorners[_currentCornerIndex].PathCorners.Length - 1;

    private int PeekNextSubCornerIndex()
    {
      if ((_currentCornerIndex + 1 >= _pathCorners.Count && _nextSubCornerIndex + 1 >= _pathCorners[_currentCornerIndex].PathCorners.Length) || !_navigationService.InStoppingProximity(_transform.position, _pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex]))
      {
        return _nextSubCornerIndex;
      }
      else
      {
        if (LastOfSubCorners())
        {
          _currentCornerIndex += 1;
          _nextSubCornerIndex = -1;
        }
        return _nextSubCornerIndex + 1;
      }
    }

    private static (Vector3 position, float leftTime) MoveInDirection(
      Vector3 position,
      Vector3 destination,
      float speed,
      float deltaTime)
    {
      Vector3 normalized = (destination - position).normalized;
      if (normalized == Vector3.zero)
        return (destination, deltaTime);
      Vector3 movement = speed * deltaTime * normalized;
      float magnitude1 = movement.magnitude;
      Vector3 vector3 = ClampMovement(movement, magnitude1);
      float actualDistance = Vector3.Distance(position, destination);
      float magnitude2 = vector3.magnitude;
      if ((double) actualDistance >= (double) magnitude2)
      {
        float num = LeftTime(deltaTime, magnitude2, magnitude1);
        return (position + vector3, num);
      }
      float num1 = LeftTime(deltaTime, actualDistance, magnitude2);
      return (destination, num1);
    }

    private static Vector3 ClampMovement(Vector3 movement, float movementMagnitude) => movementMagnitude <= 0.100000001490116 / ExtraPathCornersMultiplier ? movement : movement.normalized * 0.1f / ExtraPathCornersMultiplier;

    private static float LeftTime(float deltaTime, float actualDistance, float maxDistance) => deltaTime * (float) (1.0 - (double) actualDistance / (double) maxDistance);
  }
}
