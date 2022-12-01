using Timberborn.CharacterMovementSystem;
using Timberborn.Navigation;
using UnityEngine;

namespace  ChooChoo
{
  public class TrackFollowerFactory
  {
    private readonly INavigationService _navigationService;

    public TrackFollowerFactory(INavigationService navigationService) => _navigationService = navigationService;

    public TrackFollower Create(GameObject owner) => new(_navigationService, owner.GetComponent<MovementAnimator>(), owner.transform);
  }
}
