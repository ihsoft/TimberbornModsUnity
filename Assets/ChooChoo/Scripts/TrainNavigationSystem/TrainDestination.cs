using Bindito.Core;
using Timberborn.ConstructibleSystem;
using Timberborn.EntitySystem;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;

namespace ChooChoo
{
    public class TrainDestination : TickableComponent, IFinishedStateListener, IRegisteredComponent
    {
        private TrainDestinationsRepository _trainDestinationsRepository;
        private TrainYardService _trainYardService;
        private EventBus _eventBus;

        private bool _tracksUpdated;

        public bool ConnectedToTrainYard { get; private set; }

        [Inject]
        public void InjectDependencies(TrainDestinationsRepository trainDestinationsRepository, TrainYardService trainYardService, EventBus eventBus)
        {
            _trainDestinationsRepository = trainDestinationsRepository;
            _trainYardService = trainYardService;
            _eventBus = eventBus;
        }

        public void OnEnterFinishedState()
        {
            _eventBus.Register(this);
            _trainDestinationsRepository.Register(this);
            CheckConnectedToTrainYard();
        }

        public void OnExitFinishedState()
        {
            _eventBus.Unregister(this);
            _trainDestinationsRepository.UnRegister(this);
        }
        
        public override void Tick()
        {
            if (!_tracksUpdated) 
                return;
            CheckConnectedToTrainYard();
            _tracksUpdated = false;
        }
        
        [OnEvent]
        public void OnTrackUpdate(OnTracksUpdatedEvent onTracksUpdatedEvent)
        {
            _tracksUpdated = true;
        }

        private void CheckConnectedToTrainYard()
        {
            ConnectedToTrainYard = _trainYardService.ConnectedToTrainYard(this);
        }
    }
}