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

    public float wagonLength;

    [Inject]
    public void InjectDependencies(ObjectFollowerFactory objectFollowerFactory)
    {
      _objectFollowerFactory = objectFollowerFactory;
    }

    public void Awake()
    {
      _objectFollower = _objectFollowerFactory.Create(gameObject);
    }

    public void SetObjectToFollow(Transform objectToFollow, float distanceFromObject)
    {
      _objectFollower.SetObjectToFollow(objectToFollow, distanceFromObject + wagonLength / 2);
    }
    
    public void SetPathCorners(TrackFollower trackFollower)
    {
      _objectFollower.SetTrackFollower(trackFollower);
    }

    public void Stop()
    {
      _objectFollower.StopMoving();
    }

    public void Move(float time, float speed)
    {
      _objectFollower.MoveTowardsObject(time, _animationName, speed);
    }
  }
}
