using Timberborn.CharacterMovementSystem;
using Timberborn.Navigation;
using UnityEngine;

namespace  ChooChoo
{
  public class ObjectFollowerFactory
  {
    private readonly INavigationService _navigationService;
    private readonly TrainNavigationService _trainNavigationService;

    public ObjectFollowerFactory(INavigationService navigationService, TrainNavigationService trainNavigationService) 
    {
      _navigationService = navigationService;
      _trainNavigationService = trainNavigationService;
    }

    public ObjectFollower Create(GameObject owner) => new(_navigationService, _trainNavigationService, owner.GetComponent<MovementAnimator>(), owner.transform);
  }
}
