using System;
using System.Collections.Generic;
using Bindito.Core;
using Timberborn.Carrying;
using Timberborn.CharacterMovementSystem;
using Timberborn.EntitySystem;
using Timberborn.Persistence;
using Timberborn.WalkingSystem;
using UnityEngine;

namespace ChooChoo
{
  public class TrainWagon : MonoBehaviour, IDeletableEntity, IPersistentEntity
  {
    private static readonly ComponentKey TrainWagonKey = new(nameof(TrainWagon));
    private static readonly PropertyKey<string> ActiveWagonTypeKey = new(nameof(ActiveWagonType));
    private MovementAnimator _movementAnimator;
    private ObjectFollowerFactory _objectFollowerFactory;
    private WalkerSpeedManager _walkerSpeedManager;
    public GameObject Train { get; set; }
    public GoodCarrier GoodCarrier { get; set; }
    public ObjectFollower ObjectFollower;

    private string _animationName = "Walking";

    public float wagonLength;
    public string ActiveWagonType { get; set; } = "Tobbert.WagonType.Flat";

    [Inject]
    public void InjectDependencies(ObjectFollowerFactory objectFollowerFactory) => _objectFollowerFactory = objectFollowerFactory;

    public void Awake()
    {
      _walkerSpeedManager = GetComponent<WalkerSpeedManager>();
      GoodCarrier = GetComponent<GoodCarrier>();
      ObjectFollower = _objectFollowerFactory.Create(gameObject);
    }
    
    public void InitializeObjectFollower(Transform objectToFollow, float distanceFromObject)
    {
      ObjectFollower.SetObjectToFollow(objectToFollow, distanceFromObject + wagonLength / 2);
    }

    public void StartMoving(ITrackFollower trackFollower, List<TrackRoute> pathRoutes) => ObjectFollower.SetNewPathRoutes(trackFollower, pathRoutes);
    
    public void Move()
    {
      var speed = _walkerSpeedManager.CalculateNormalWalkingSpeed();
      speed *= 1.008f;
      var time = Time.fixedDeltaTime;
      ObjectFollower.MoveTowardsObject(time, _animationName, speed);
    }

    public void Stop() => ObjectFollower.StopMoving();
    
    public void DeleteEntity()
    {
      ObjectFollower.StopMoving();
    }

    public void Save(IEntitySaver entitySaver)
    {
      entitySaver.GetComponent(TrainWagonKey).Set(ActiveWagonTypeKey, ActiveWagonType);
    }

    public void Load(IEntityLoader entityLoader)
    {
      if (entityLoader.HasComponent(TrainWagonKey))
        ActiveWagonType = entityLoader.GetComponent(TrainWagonKey).Get(ActiveWagonTypeKey);
    }
  }
}
