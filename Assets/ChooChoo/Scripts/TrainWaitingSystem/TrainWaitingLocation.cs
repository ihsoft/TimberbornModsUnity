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
        
        private GameObject Occupant { get; set; }
        
        public bool Occupied => Occupant != null;

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

        public void Occupy(GameObject occupant) => Occupant = occupant;

        public void UnOccupy() => Occupant = null;
    }
}