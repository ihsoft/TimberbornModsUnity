using System.Collections.Generic;
using HarmonyLib;
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
    private IReadOnlyList<Vector3> _pathCorners;
    private int _nextCornerIndex;
    private Transform _objectToFollow;

    public ObjectFollower(
      INavigationService navigationService,
      MovementAnimator movementAnimator,
      Transform transform)
    {
      _navigationService = navigationService;
      _movementAnimator = movementAnimator;
      _transform = transform;
    }

    public void SetObjectToFollow(Transform objectToFollow)
    {
      _objectToFollow = objectToFollow;
    }

    public void MoveTowardsObject(List<Vector3> animatedPathCorners, float deltaTime, string animationName, float movementSpeed)
    {
      _pathCorners = animatedPathCorners;
      _pathCorners.AddItem(_objectToFollow.position);
      _nextCornerIndex = 0;
      _animatedPathCorners.Clear();
      float time = Time.time;
      _animatedPathCorners.Add(new PathCorner(_transform.position, time));
      float num = deltaTime;
      // Plugin.Log.LogWarning(Vector3.Distance(_transform.position, _objectToFollow.position) + "");
      while ((Vector3.Distance(_transform.position, _objectToFollow.position) > 0 && _pathCorners[_nextCornerIndex] != _objectToFollow.position)
             && num > 0) 
      {
        _nextCornerIndex = PeekNextCornerIndex();
        Vector3 position;
        (position, num) = MoveInDirection(_transform.position, _pathCorners[_nextCornerIndex], movementSpeed, num);
        _transform.position = position;
        float timeInSeconds = time + deltaTime - num;
        _animatedPathCorners.Add(new PathCorner(position, timeInSeconds));
        // Plugin.Log.LogInfo(Vector3.Distance(_transform.position, _objectToFollow.position) + "" + _transform.position);
      }
      
      _movementAnimator.AnimateMovementAlongPath(_animatedPathCorners, animationName, movementSpeed);
    }

    public void StopMoving()
    {
      _pathCorners = null;
      _movementAnimator.StopAnimatingMovement();
    }

    private int PeekNextCornerIndex() => _nextCornerIndex + 1 >= _pathCorners.Count || !_navigationService.InStoppingProximity(_transform.position, _pathCorners[_nextCornerIndex]) ? _nextCornerIndex : _nextCornerIndex + 1;
   

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
