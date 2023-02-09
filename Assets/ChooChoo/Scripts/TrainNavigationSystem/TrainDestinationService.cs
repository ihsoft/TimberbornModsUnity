using System.Collections.Generic;
using Timberborn.BlockSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainDestinationService
    {
        private readonly TrainDestinationConnectedRepository _trainDestinationConnectedRepository;
        private readonly BlockService _blockService;

        public TrainDestinationService(TrainDestinationConnectedRepository trainDestinationConnectedRepository, BlockService blockService)
        {
            _trainDestinationConnectedRepository = trainDestinationConnectedRepository;
            _blockService = blockService;
        }

        public bool TrainDestinationsConnected(TrainDestination a, TrainDestination b)
        {
            return TrainDestinationConnectedOneWay(a, b) && TrainDestinationConnectedOneWay(b, a);
        }

        public bool DestinationReachable(TrackPiece start, TrainDestination end)
        {
            if (start == null || end == null)
                return false;
            var checkedTrackPieces = new List<TrackPiece>();
            var connectedDestination = FindTrainDestination(start, checkedTrackPieces);
            return TrainDestinationsConnected(connectedDestination, end);
        }
        
        public bool DestinationReachable(Vector3 startPosition, TrainDestination end)
        {
            var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(startPosition.ToBlockServicePosition());
            if (startTrackPiece == null || end == null)
                return false;
            var checkedTrackPieces = new List<TrackPiece>();
            var connectedDestination = FindTrainDestination(startTrackPiece, checkedTrackPieces);
            return TrainDestinationsConnected(connectedDestination, end);
        }

        private bool TrainDestinationConnectedOneWay(TrainDestination originTrainDestination, TrainDestination checkingTrainDestination)
        {
            if (originTrainDestination == null)
                return false;

            var trainDestinations = _trainDestinationConnectedRepository.TrainDestinations;
            
            return trainDestinations.ContainsKey(originTrainDestination) && trainDestinations[originTrainDestination].Contains(checkingTrainDestination);
        }
        
        private TrainDestination FindTrainDestination(TrackPiece checkingTrackPiece, List<TrackPiece> checkedTrackPieces)
        {
            checkedTrackPieces.Add(checkingTrackPiece);

            if (checkingTrackPiece.TryGetComponent(out TrainDestination trainDestination))
                return trainDestination;
            
            foreach (var trackConnection in checkingTrackPiece.TrackRoutes)
            {
                if (trackConnection.Exit.ConnectedTrackPiece == null)
                    continue;

                if (checkedTrackPieces.Contains(trackConnection.Exit.ConnectedTrackPiece))
                    continue;

                var destination = FindTrainDestination(trackConnection.Exit.ConnectedTrackPiece, checkedTrackPieces);

                if (destination != null)
                    return destination;
            }

            checkedTrackPieces.Remove(checkingTrackPiece);
            return null;
        }
    }
}