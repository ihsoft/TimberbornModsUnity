﻿using System.Collections.Generic;
using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.CharacterMovementSystem;
using Timberborn.EntitySystem;
using Timberborn.WalkingSystem;
using UnityEngine;

namespace ChooChoo
{
  public class TrainWagon : BaseComponent, IDeletableEntity
  {
    private MovementAnimator _movementAnimator;
    private ObjectFollowerFactory _objectFollowerFactory;
    private WalkerSpeedManager _walkerSpeedManager;
    private WagonModelManager _wagonModelManager;
    public BaseComponent Train { get; set; }
    
    public ObjectFollower ObjectFollower;

    private readonly string _animationName = "Walking";

    [Inject]
    public void InjectDependencies(ObjectFollowerFactory objectFollowerFactory) => _objectFollowerFactory = objectFollowerFactory;

    public void Awake()
    {
      _walkerSpeedManager = GetComponentFast<WalkerSpeedManager>();
      _wagonModelManager = GetComponentFast<WagonModelManager>();
      ObjectFollower = _objectFollowerFactory.Create(GameObjectFast);
    }
    
    public void InitializeObjectFollower(Transform objectToFollow, float distanceFromObject)
    {
      ObjectFollower.SetObjectToFollow(objectToFollow, distanceFromObject / 2 + _wagonModelManager.ActiveWagonModel.WagonModelSpecification.Length / 2);
    }

    public void StartMoving(ITrackFollower trackFollower, List<TrackRoute> pathRoutes) => ObjectFollower.SetNewPathRoutes(trackFollower, pathRoutes);
    
    public void Move()
    {
      var speed = 7f;
      speed *= 1.008f;
      var time = Time.fixedDeltaTime;
      ObjectFollower.MoveTowardsObject(time, _animationName, speed);
    }

    public void Stop() => ObjectFollower.StopMoving();
    
    public void DeleteEntity()
    {
      ObjectFollower.StopMoving();
    }
  }
}
