using System.Collections.Generic;
using System.Linq;
using Timberborn.CharacterMovementSystem;
using Timberborn.Navigation;
using UnityEngine;

namespace ChooChoo
{
  public class ObjectFollower
  {
    private readonly INavigationService _navigationService;
    private readonly MovementAnimator _movementAnimator;
    private readonly Transform _transform;
    private readonly List<PathCorner> _animatedPathCorners = new(100);
    private TrackFollower _trackFollower;
    private int _currentCornerIndex;
    private int _nextSubCornerIndex;
    private Transform _objectToFollow;
    private float _minDistanceFromObject;

    public ObjectFollower(
      INavigationService navigationService,
      MovementAnimator movementAnimator,
      Transform transform)
    {
      _navigationService = navigationService;
      _movementAnimator = movementAnimator;
      _transform = transform;
    }

    public void SetObjectToFollow(Transform objectToFollow, float minDistanceFromObject)
    {
      _objectToFollow = objectToFollow;
      _minDistanceFromObject = minDistanceFromObject;
    }
    public void SetTrackFollower(TrackFollower trackFollower)
    {
      _trackFollower = trackFollower;
    }

    public void MoveTowardsObject(float deltaTime, string animationName, float movementSpeed)
    {
      _animatedPathCorners.Clear();
      float time = Time.time;
      _animatedPathCorners.Add(new PathCorner(_transform.position, time));
      float num = deltaTime;
      while (num > 0.0 
             && Vector3.Distance(_transform.position, _objectToFollow.position) > _minDistanceFromObject
             && !ReachedLastPathCorner()
             && _trackFollower._currentCornerIndex >= _currentCornerIndex)
      {
        _nextSubCornerIndex = PeekNextSubCornerIndex();
        Vector3 position;
        (position, num) = MoveInDirection(_transform.position, _trackFollower._pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex], movementSpeed, num);
        _transform.position = position;
        float timeInSeconds = time + deltaTime - num;
        _animatedPathCorners.Add(new PathCorner(position, timeInSeconds));
      }
      _movementAnimator.AnimateMovementAlongPath(_animatedPathCorners, animationName, movementSpeed);
    }

    public void StopMoving()
    {
      _movementAnimator.StopAnimatingMovement();
      _currentCornerIndex = 0;
      _nextSubCornerIndex = 0;
    }

    private bool ReachedLastPathCorner() => _navigationService.InStoppingProximity(_trackFollower._pathCorners.Last().PathCorners.Last(), _transform.position);

    private bool LastOfSubCorners() => _nextSubCornerIndex >= _trackFollower._pathCorners[_currentCornerIndex].PathCorners.Length - 1;

    private int PeekNextSubCornerIndex()
    {
      if (_currentCornerIndex + 1 >= _trackFollower._pathCorners.Count || !_navigationService.InStoppingProximity(_transform.position, _trackFollower._pathCorners[_currentCornerIndex].PathCorners[_nextSubCornerIndex]))
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

    private static Vector3 ClampMovement(Vector3 movement, float movementMagnitude) => movementMagnitude <= 0.100000001490116 ? movement : movement.normalized * 0.1f;

    private static float LeftTime(float deltaTime, float actualDistance, float maxDistance) => deltaTime * (float) (1.0 - (double) actualDistance / (double) maxDistance);
  }
}
