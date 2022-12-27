using System.Collections.Generic;
using System.Linq;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;

namespace ChooChoo
{
    public class TrainDestinationConnectedRepository : ITickableSingleton, ILoadableSingleton
    {
        private readonly EventBus _eventBus;

        private readonly TrainDestinationsRepository _trainDestinationsRepository;

        private List<TrainDestination>[] _trainDestinationConnections;

        private bool _tracksUpdated;

        public List<TrainDestination>[] TrainDestinations => _trainDestinationConnections;

        TrainDestinationConnectedRepository(EventBus eventBus, TrainDestinationsRepository trainDestinationsRepository)
        {
            _eventBus = eventBus;
            _trainDestinationsRepository = trainDestinationsRepository;
        }

        public void Load()
        {
            _eventBus.Register(this);
            FindDestinationConnections();
        }
        
        public void Tick()
        {
            if (!_tracksUpdated) 
                return;
            FindDestinationConnections();
            _tracksUpdated = false;
        }

        [OnEvent]
        public void OnTrackUpdate(OnTracksUpdatedEvent onTracksUpdatedEvent)
        {
            _tracksUpdated = true;
        }

        private void FindDestinationConnections()
        {
            var list = new List<List<TrainDestination>>();
            foreach (var checkingDestination in _trainDestinationsRepository.TrainDestinations)
            {
                if (list.Any(destinations => destinations.Any(destination => destination == checkingDestination)))
                    continue;
                
                var trainDestinationsConnected = new List<TrainDestination>();
                var checkedTrackPieces = new List<TrackPiece>();
                CheckNextTrackPiece(checkingDestination.GetComponent<TrackPiece>(), checkedTrackPieces, trainDestinationsConnected);
                list.Add(trainDestinationsConnected);
            }
            
            _trainDestinationConnections = list.ToArray();
        }
        
        private void CheckNextTrackPiece(TrackPiece checkingTrackPiece, List<TrackPiece> checkedTrackPieces, List<TrainDestination> trainDestinationsConnected)
        {
            checkedTrackPieces.Add(checkingTrackPiece);

            if (checkingTrackPiece.TryGetComponent(out TrainDestination trainDestination))
                trainDestinationsConnected.Add(trainDestination);
            
            foreach (var trackConnection in checkingTrackPiece.TrackConnections)
            {
                if (trackConnection.ConnectedTrackPiece == null)
                    continue;

                if (checkedTrackPieces.Contains(trackConnection.ConnectedTrackPiece))
                    continue;

                CheckNextTrackPiece(trackConnection.ConnectedTrackPiece, checkedTrackPieces, trainDestinationsConnected);
            }

            checkedTrackPieces.Remove(checkingTrackPiece);
        }
    }
}