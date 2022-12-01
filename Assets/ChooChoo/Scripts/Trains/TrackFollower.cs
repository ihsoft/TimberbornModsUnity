using System.Collections.Generic;
using System.Linq;
using Timberborn.CharacterMovementSystem;
using Timberborn.Navigation;
using UnityEngine;

namespace ChooChoo
{
  public class TrackFollower
  {
    private readonly INavigationService _navigationService;
    private readonly MovementAnimator _movementAnimator;
    private readonly Transform _transform;
    private readonly List<PathCorner> _animatedPathCorners = new(100);
    private IReadOnlyList<TrackConnection> _pathCorners;
    private int _pathCornersCount;
    private int _currentCornerIndex;
    private int _nextSubCornerIndex = 0;
    private TrackSection _trackSection;

    public TrackFollower(
      INavigationService navigationService,
      MovementAnimator movementAnimator,
      Transform transform)
    {
      _navigationService = navigationService;
      _movementAnimator = movementAnimator;
      _transform = transform;
    }

    public void StartMovingAlongPath(IReadOnlyList<TrackConnection> pathCorners)
    {
      ResetTrackSection();
      _pathCorners = pathCorners;
      _currentCornerIndex = 1;
    }

    public void MoveAlongPath(float deltaTime, string animationName, float movementSpeed)
    {
      // Plugin.Log.LogError("MoveAlongPath");
      
      _animatedPathCorners.Clear();
      float time = Time.time;
      _animatedPathCorners.Add(new PathCorner(_transform.position, time));
      float num = deltaTime;
      while (num > 0.0 && !ReachedLastPathCorner())
      {
        if (!CanEnterNextSection())
        {
          _animatedPathCorners.Clear();
          return;
        }
        
        _nextSubCornerIndex = PeekNextSubCornerIndex();
        Vector3 position;
        (position, num) = MoveInDirection(_transform.position, _pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex], movementSpeed, num);
        // Plugin.Log.LogWarning(_currentCornerIndex + "  " + _pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex]);
        _transform.position = position;
        float timeInSeconds = time + deltaTime - num;
        _animatedPathCorners.Add(new PathCorner(position, timeInSeconds));
      }
      if (!ReachedLastPathCorner())
        _animatedPathCorners.Add(new PathCorner(_pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex], (float) ((double) time + deltaTime + 1.0)));
      _movementAnimator.AnimateMovementAlongPath(_animatedPathCorners, animationName, movementSpeed);
    }

    public void StopMoving()
    {
      ResetTrackSection();
      _pathCorners = null;
      _movementAnimator.StopAnimatingMovement();
    }

    public bool ReachedLastPathCorner() => _navigationService.InStoppingProximity(_pathCorners.Last().PathCorners.Last(), _transform.position);

    private void ResetTrackSection()
    {
      if (_trackSection == null) 
        return;
      _trackSection.Occupied = false;
      _trackSection = null;
    }

    private bool CanEnterNextSection()
    {
      TrackPiece trackPiece = _pathCorners[_currentCornerIndex - 1].ConnectedTrackPiece;
      // Plugin.Log.LogInfo(_pathCorners[_nextCornerIndex].Coordinates.ToString());
      if (trackPiece == null)
        return false;
      TrackSection trackSection = trackPiece.TrackSection;

      var flag = trackSection.Equals(_trackSection);
      // Plugin.Log.LogInfo("Flag " + flag + _pathCorners[PeekNextCornerIndex()].PathCorners[0]);
      // Plugin.Log.LogInfo("Occupied " + trackSection.Occupied);
      if (flag)
        return true;

      if (trackSection.Occupied)
        return false;

      UpdateTrackSection(trackSection);
      return true;
    }

    private void UpdateTrackSection(TrackSection trackSection)
    {
      SetTrackSectionOccupation(false);
      _trackSection = trackSection;
      SetTrackSectionOccupation(true);
    }

    private void SetTrackSectionOccupation(bool newValue)
    {
      if(_trackSection != null) 
        _trackSection.Occupied = newValue;
    }

    private bool LastOfSubCorners() => _nextSubCornerIndex >= _pathCorners[_currentCornerIndex].PathCorners.Length - 1;

    private int PeekNextSubCornerIndex()
    {
      if (_currentCornerIndex + 1 >= _pathCorners.Count || !_navigationService.InStoppingProximity(_transform.position, _pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex]))
      {
        // Plugin.Log.LogError((_nextSubCornerIndex + 1 >= _pathCorners[_currentCornerIndex].PathCorners.Length) + "   " + !_navigationService.InStoppingProximity(_transform.position, _pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex]));
        return _nextSubCornerIndex;
      }
      else
      {
        // Plugin.Log.LogError(LastOfSubCorners().ToString());
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

    private static Vector3 ClampMovement(Vector3 movement, float movementMagnitude) => movementMagnitude <= 0.100000001490116 ? movement : movement.normalized * 0.1f;

    private static float LeftTime(float deltaTime, float actualDistance, float maxDistance) => deltaTime * (float) (1.0 - (double) actualDistance / (double) maxDistance);
  }
}
