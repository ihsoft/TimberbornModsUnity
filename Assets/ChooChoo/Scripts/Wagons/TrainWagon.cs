using System.Collections.Generic;
using Bindito.Core;
using Timberborn.CharacterMovementSystem;
using Timberborn.WalkingSystem;
using UnityEngine;

namespace ChooChoo
{
  public class TrainWagon : MonoBehaviour
  {
    private MovementAnimator _movementAnimator;
    private ObjectFollowerFactory _objectFollowerFactory;
    private WalkerSpeedManager _walkerSpeedManager;
    public ObjectFollower ObjectFollower;

    private string _animationName = "Walking";

    public float wagonLength;

    [Inject]
    public void InjectDependencies(ObjectFollowerFactory objectFollowerFactory) => _objectFollowerFactory = objectFollowerFactory;

    public void Awake()
    {
      _walkerSpeedManager = GetComponent<WalkerSpeedManager>();
      ObjectFollower = _objectFollowerFactory.Create(gameObject);
    }

    public void InitializeObjectFollower(Transform objectToFollow, float distanceFromObject)
    {
      ObjectFollower.SetObjectToFollow(objectToFollow, distanceFromObject + wagonLength / 2);
    }

    public void StartMoving(ITrackFollower trackFollower, List<TrackConnection> pathConnections) => ObjectFollower.SetNewPathConnections(trackFollower, pathConnections);

    public void Stop() => ObjectFollower.StopMoving();

    public void Move()
    {
      var speed = _walkerSpeedManager.Speed;
      speed *= 1.012f;
      var time = Time.fixedDeltaTime;
      ObjectFollower.MoveTowardsObject(time, _animationName, speed);
    }
    
  }
}
