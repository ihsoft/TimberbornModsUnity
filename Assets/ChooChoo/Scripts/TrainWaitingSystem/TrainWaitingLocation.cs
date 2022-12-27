using System;
using Bindito.Core;
using Timberborn.ConstructibleSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainWaitingLocation : MonoBehaviour, IFinishedStateListener
    {
        private TrainWaitingLocationsRepository _trainWaitingLocationsRepository;

        public TrainDestination TrainDestinationComponent { get; private set; }
        
        public bool Occupied { get; set; }

        [Inject]
        public void InjectDependencies(TrainWaitingLocationsRepository trainWaitingLocationsRepository)
        {
            _trainWaitingLocationsRepository = trainWaitingLocationsRepository;
        }

        void Awake()
        {
            TrainDestinationComponent = GetComponent<TrainDestination>();
        }

        public void OnEnterFinishedState()
        {
            _trainWaitingLocationsRepository.Register(this);
        }

        public void OnExitFinishedState()
        {
            _trainWaitingLocationsRepository.UnRegister(this);
        }
    }
}