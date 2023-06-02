using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.Navigation;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace ChooChoo
{
    public class PassengerStationDistrictObject : BaseComponent, IFinishedStateListener
    {
        [SerializeField]
        private Vector3Int _coordinateOffset;
        private IDistrictService _districtService;
        private EventBus _eventBus;
        private BlockObject _blockObject;
        
        public bool GoesAcrossDistrict { get; private set; }

        [Inject]
        public void InjectDependencies(IDistrictService districtService, EventBus eventBus)
        {
            _districtService = districtService;
            _eventBus = eventBus;
        }

        public void Awake()
        {
            enabled = false;
            _eventBus.Register(this);
            _blockObject = GetComponentFast<BlockObject>();
        }

        public void OnEnterFinishedState()
        {
            enabled = true;
        }

        public void OnExitFinishedState()
        {
            enabled = false;
        }
        
        public void UpdateDistrictObject(bool newValue)
        {
            GoesAcrossDistrict = newValue;
            if (GoesAcrossDistrict)
            {
                Enable();
            }
            else
            {
                Disable();
            }
            _eventBus.Post(new OnConnectedPassengerStationsUpdated());
        }

        private void Enable()
        {
            _districtService.SetObstacle(ObstacleCoordinates);
        }

        private void Disable()
        {
            _districtService.UnsetObstacle(ObstacleCoordinates);
        }

        private Vector3Int ObstacleCoordinates => _blockObject.Transform(_coordinateOffset);
    }
}