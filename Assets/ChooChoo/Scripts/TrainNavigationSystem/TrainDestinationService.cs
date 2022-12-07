using System.Collections.Generic;

namespace ChooChoo
{
    public class TrainDestinationService
    {
        private readonly TrainDestinationConnectedRepository _trainDestinationConnectedRepository;

        public TrainDestinationService(TrainDestinationConnectedRepository trainDestinationConnectedRepository)
        {
            _trainDestinationConnectedRepository = trainDestinationConnectedRepository;
        }

        public bool TrainDestinationsConnected(TrainDestination a, TrainDestination b)
        {
            foreach (var connectedTrainDestinations in _trainDestinationConnectedRepository.TrainDestinations)
            {
                var partOfConnectedDestinations1 = connectedTrainDestinations.Contains(a);
                var partOfConnectedDestinations2 = connectedTrainDestinations.Contains(b);

                if (partOfConnectedDestinations1 && partOfConnectedDestinations2)
                    return true;
            }

            return false;
        }

        public bool DestinationReachable(TrackPiece start, TrainDestination end)
        {
            var checkedTrackPieces = new List<TrackPiece>();
            var connectedDestination = FindTrainDestination(start, checkedTrackPieces);
            return TrainDestinationsConnected(connectedDestination, end);
        }
        
        private TrainDestination FindTrainDestination(TrackPiece checkingTrackPiece, List<TrackPiece> checkedTrackPieces)
        {
            checkedTrackPieces.Add(checkingTrackPiece);

            if (checkingTrackPiece.TryGetComponent(out TrainDestination trainDestination))
                return trainDestination;
            
            foreach (var trackConnection in checkingTrackPiece.TrackConnections)
            {
                if (trackConnection.ConnectedTrackPiece == null)
                    continue;

                if (checkedTrackPieces.Contains(trackConnection.ConnectedTrackPiece))
                    continue;

                var destination = FindTrainDestination(trackConnection.ConnectedTrackPiece, checkedTrackPieces);

                if (destination != null)
                    return destination;
            }

            checkedTrackPieces.Remove(checkingTrackPiece);
            return null;
        }
    }
}