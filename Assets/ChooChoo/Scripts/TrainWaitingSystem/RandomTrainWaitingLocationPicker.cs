using System;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.Common;
using UnityEngine;
using Random = System.Random;

namespace ChooChoo
{
    public class RandomTrainWaitingLocationPicker
    {
        private readonly TrainWaitingLocationsRepository _trainWaitingLocationsRepository;
        
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        RandomTrainWaitingLocationPicker(TrainWaitingLocationsRepository trainWaitingLocationsRepository, IRandomNumberGenerator randomNumberGenerator)
        {
            _trainWaitingLocationsRepository = trainWaitingLocationsRepository;
            _randomNumberGenerator = randomNumberGenerator;
        }

        public TrainWaitingLocation RandomWaitingLocation()
        {
            var list = _trainWaitingLocationsRepository.WaitingLocations.Where(location => !location.Occupied).ToList();
            if (list.Count < 1)
                return null;
            var randomWaitingLocation = list[_randomNumberGenerator.Range(0, list.Count)];
            
            return randomWaitingLocation;
        }
    }
}
