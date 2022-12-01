using System.Collections.Generic;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    public class TrainNavigationService
    {
        private readonly RandomTrainDestinationPicker _randomTrainDestinationPicker;

        private readonly IRandomNumberGenerator _randomNumberGenerator;

        private BlockService _blockService;

        private int MaxDistance = 30;

        TrainNavigationService(RandomTrainDestinationPicker randomTrainDestinationPicker, IRandomNumberGenerator randomNumberGenerator, BlockService blockService)
        {
            _randomTrainDestinationPicker = randomTrainDestinationPicker;
            _randomNumberGenerator = randomNumberGenerator;
            _blockService = blockService;
        }
        
        public bool FindRailTrackPath(Vector3 start, Vector3 destination, List<TrackConnection> _tempPathTrackConnections)
        {
            var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(Vector3Int.FloorToInt(new Vector3(start.x, start.z, start.y)));
            var endCoordinate = Vector3Int.FloorToInt(new Vector3(destination.x, destination.z, destination.y));
            var endTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(endCoordinate);
            // Plugin.Log.LogError((startTrackPiece != null) + "   " + (endTrackPiece != null));
            if (startTrackPiece == null || endTrackPiece == null || startTrackPiece == endTrackPiece) 
                return false;
            
            var checkedTracks = new List<TrackPiece>();
            var trackConnections = new List<TrackConnection>();
            if (!FindNextRailTrack(startTrackPiece, endTrackPiece, checkedTracks, trackConnections)) 
                return false;
            // foreach (var trackPiece in tracks)
            // {
            //     Plugin.Log.LogWarning(trackPiece.transform.position.ToString());
            // }
            trackConnections.Reverse();
            _tempPathTrackConnections.AddRange(trackConnections);
            _tempPathTrackConnections.Add(endTrackPiece.TrackConnections[0]);
            // _tempPathCorners.Add(start);
            // _tempPathCorners.Add(destination);
            return true;

        }

        private bool FindNextRailTrack(TrackPiece previousTrackPiece, TrackPiece destinationTrackPiece, List<TrackPiece> checkedTracks, List<TrackConnection> trackConnections)
        {
            checkedTracks.Add(previousTrackPiece);
            foreach (var trackConnection in previousTrackPiece.TrackConnections)
            {

                // Plugin.Log.LogError(trackConnection.Direction + "   " + trackConnection.ConnectedTrackPiece + "   " + previousTrackPiece.PathCorners[0]);
                
                if (trackConnection.ConnectedTrackPiece == null)
                    continue;
                
                if (checkedTracks.Contains(trackConnection.ConnectedTrackPiece))
                    continue;

                if (trackConnection.ConnectedTrackPiece == destinationTrackPiece)
                {
                    trackConnections.Add(trackConnection);
                    return true;
                }

                if (FindNextRailTrack(trackConnection.ConnectedTrackPiece, destinationTrackPiece, checkedTracks, trackConnections))
                {
                    trackConnections.Add(trackConnection);
                    return true;
                }
            }

            checkedTracks.Remove(previousTrackPiece);
            return false;
        }
    }
}
