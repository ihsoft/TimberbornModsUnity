using System.Collections.Generic;
using Bindito.Core;
using Timberborn.Carrying;
using Timberborn.CharacterMovementSystem;
using Timberborn.EntitySystem;
using Timberborn.WalkingSystem;
using UnityEngine;

namespace ChooChoo
{
  public class TrainWagon : MonoBehaviour, IDeletableEntity
  {
    private MovementAnimator _movementAnimator;
    private ObjectFollowerFactory _objectFollowerFactory;
    private WalkerSpeedManager _walkerSpeedManager;
    public GameObject Train { get; set; }
    public GoodCarrier GoodCarrier { get; set; }
    public ObjectFollower ObjectFollower;

    private string _animationName = "Walking";

    public float wagonLength;

    [Inject]
    public void InjectDependencies(ObjectFollowerFactory objectFollowerFactory) => _objectFollowerFactory = objectFollowerFactory;

    public void Awake()
    {
      _walkerSpeedManager = GetComponent<WalkerSpeedManager>();
      GoodCarrier = GetComponent<GoodCarrier>();
      ObjectFollower = _objectFollowerFactory.Create(gameObject);
    }

    public void InitializeObjectFollower(Transform objectToFollow, float distanceFromObject, bool checkForLeavingSection)
    {
      ObjectFollower.SetObjectToFollow(objectToFollow, distanceFromObject + wagonLength / 2, checkForLeavingSection);
    }

    public void StartMoving(ITrackFollower trackFollower, List<TrackRoute> pathRoutes) => ObjectFollower.SetNewPathRoutes(trackFollower, pathRoutes);

    public void Stop() => ObjectFollower.StopMoving();

    public void Move()
    {
      var speed = _walkerSpeedManager.CalculateNormalWalkingSpeed();
      speed *= 1.008f;
      var time = Time.fixedDeltaTime;
      ObjectFollower.MoveTowardsObject(time, _animationName, speed);
    }

    public void DeleteEntity()
    {
      ObjectFollower.StopMoving();
    }
  }
}
