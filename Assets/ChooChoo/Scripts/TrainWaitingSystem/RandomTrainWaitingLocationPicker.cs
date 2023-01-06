using System;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.Common;
using UnityEngine;
using Random = System.Random;

namespace ChooChoo
{
    public class ClosestTrainWaitingLocationPicker
    {
        private readonly TrainWaitingLocationsRepository _trainWaitingLocationsRepository;

        private readonly TrainDestinationService _trainDestinationService;

        ClosestTrainWaitingLocationPicker(TrainWaitingLocationsRepository trainWaitingLocationsRepository, TrainDestinationService trainDestinationService)
        {
            _trainWaitingLocationsRepository = trainWaitingLocationsRepository;
            _trainDestinationService = trainDestinationService;
        }

        public TrainWaitingLocation RandomWaitingLocation(Vector3 position)
        {
            var list = _trainWaitingLocationsRepository.WaitingLocations.Where(location => !location.Occupied && _trainDestinationService.DestinationReachable(position, location.TrainDestinationComponent)).OrderBy(location => Vector3.Distance(position, location.transform.position)).ToList();
            if (!list.Any())
                return null;
            var closestReachableTrainWaitingLocation = list.First();
            
            return closestReachableTrainWaitingLocation;
        }
    }
}
