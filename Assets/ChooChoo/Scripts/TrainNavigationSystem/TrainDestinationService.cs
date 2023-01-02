using System.Collections.Generic;
using System.Linq;
using Timberborn.BlockSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainDestinationService
    {
        private readonly TrainDestinationConnectedRepository _trainDestinationConnectedRepository;
        private readonly TrainDestinationsRepository _trainDestinationsRepository;
        private readonly BlockService _blockService;

        public TrainDestinationService(TrainDestinationConnectedRepository trainDestinationConnectedRepository, TrainDestinationsRepository trainDestinationsRepository, BlockService blockService)
        {
            _trainDestinationConnectedRepository = trainDestinationConnectedRepository;
            _trainDestinationsRepository = trainDestinationsRepository;
            _blockService = blockService;
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
            if (start == null || end == null)
                return false;
            var checkedTrackPieces = new List<TrackPiece>();
            var connectedDestination = FindTrainDestination(start, checkedTrackPieces);
            return TrainDestinationsConnected(connectedDestination, end);
        }

        public List<TrainDestination> ReachableTrainDestinations(Vector3 start)
        {
            var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(Vector3Int.FloorToInt(new Vector3(start.x, start.z, start.y)));

            if (!startTrackPiece.TryGetComponent(out TrainDestination trainDestination))
                return null;

            return _trainDestinationConnectedRepository.TrainDestinations.FirstOrDefault(connectedTrainDestinations => connectedTrainDestinations.Contains(trainDestination));
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