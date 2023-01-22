using System.Collections.Generic;
using System.Linq;
using Timberborn.SingletonSystem;

namespace ChooChoo
{
    public class TrainDestinationConnectedRepository : ILoadableSingleton
    {
        private readonly EventBus _eventBus;

        private readonly TrainDestinationsRepository _trainDestinationsRepository;

        private List<TrainDestination>[] _trainDestinationConnections = {};

        private bool _tracksUpdated = true;

        public List<TrainDestination>[] TrainDestinations {
            get
            {
                if (_tracksUpdated)
                    Update();
                return _trainDestinationConnections;
            }
        }

        TrainDestinationConnectedRepository(EventBus eventBus, TrainDestinationsRepository trainDestinationsRepository)
        {
            _eventBus = eventBus;
            _trainDestinationsRepository = trainDestinationsRepository;
        }

        public void Load()
        {
            _eventBus.Register(this);
        }

        [OnEvent]
        public void OnTrackUpdate(OnTracksUpdatedEvent onTracksUpdatedEvent)
        {
            _tracksUpdated = true;
        }

        private void Update()
        {
            FindDestinationConnections();
            _tracksUpdated = false;
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
            // Plugin.Log.LogWarning(list.Count + "");
            // foreach (var l in list)
            // {
            //     Plugin.Log.LogInfo(l.Count + "");
            // }
            
            _trainDestinationConnections = list.ToArray();
        }
        
        private void CheckNextTrackPiece(TrackPiece checkingTrackPiece, List<TrackPiece> checkedTrackPieces, List<TrainDestination> trainDestinationsConnected)
        {
            // Plugin.Log.LogError(checkingTrackPiece.CenterCoordinates + "");
            checkedTrackPieces.Add(checkingTrackPiece);

            if (checkingTrackPiece.TryGetComponent(out TrainDestination trainDestination))
                trainDestinationsConnected.Add(trainDestination);
            
            foreach (var trackRoute in checkingTrackPiece.TrackRoutes.GroupBy(route => route.Exit.Direction).Select(group => group.First()))
            {
                if (trackRoute.Exit.ConnectedTrackPiece == null)
                    continue;

                if (checkedTrackPieces.Contains(trackRoute.Exit.ConnectedTrackPiece))
                    continue;

                CheckNextTrackPiece(trackRoute.Exit.ConnectedTrackPiece, checkedTrackPieces, trainDestinationsConnected);
            }
        }
    }
}