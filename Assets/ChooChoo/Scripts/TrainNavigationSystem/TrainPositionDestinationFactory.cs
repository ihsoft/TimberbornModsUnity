using UnityEngine;

namespace ChooChoo
{
  public class TrainPositionDestinationFactory
  {
    private readonly TrainNavigationService _trainNavigationService;

    public TrainPositionDestinationFactory(TrainNavigationService trainNavigationService)
    {
      _trainNavigationService = trainNavigationService;
    }

    public TrainPositionDestination Create(Vector3 position) => new(_trainNavigationService, position);
  }
}
