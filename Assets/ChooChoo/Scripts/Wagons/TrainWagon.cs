using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.CharacterMovementSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChooChoo
{
  public class TrainWagon : MonoBehaviour
  {
    private MovementAnimator _movementAnimator;
    private ObjectFollowerFactory _objectFollowerFactory;
    private ObjectFollower _objectFollower;

    private string _animationName = "Walking";

    public Transform objectToFollow;
    
    public List<Vector3> PreviousPathCorners { get; set; }

    [Inject]
    public void InjectDependencies(ObjectFollowerFactory objectFollowerFactory)
    {
      _objectFollowerFactory = objectFollowerFactory;
    }

    public void Awake()
    {
      _objectFollower = _objectFollowerFactory.Create(gameObject);
    }

    public void SetObjectToFollow(Transform objectToFollow)
    {
      _objectFollower.SetObjectToFollow(objectToFollow);
    }

    public void Stop()
    {
      _objectFollower.StopMoving();
    }

    public void Move(List<Vector3> pathCorners, float time, float speed)
    {
      if (pathCorners != null && pathCorners.Count > 0)
        _objectFollower.MoveTowardsObject(pathCorners, time, _animationName, speed);
      PreviousPathCorners = pathCorners;
    }
  }
}
