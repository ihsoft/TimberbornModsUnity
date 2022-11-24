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

        private int PathLength => _randomNumberGenerator.Range(0, 5);
        
        TrainNavigationService(RandomTrainDestinationPicker randomTrainDestinationPicker, IRandomNumberGenerator randomNumberGenerator, BlockService blockService)
        {
            _randomTrainDestinationPicker = randomTrainDestinationPicker;
            _randomNumberGenerator = randomNumberGenerator;
            _blockService = blockService;
        }
        
        public bool FindRailTrackPath(Vector3 start, Vector3 destination, List<Vector3> _tempPathCorners)
        {
            var distance = 0;
            var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(Vector3Int.FloorToInt(new Vector3(start.x, start.z, start.y)));
            var endCoordinate = Vector3Int.FloorToInt(new Vector3(destination.x, destination.z, destination.y));
            var endTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(endCoordinate);
            // Plugin.Log.LogError((startTrackPiece != null) + "   " + (endTrackPiece != null));
            if (startTrackPiece != null && endTrackPiece != null && startTrackPiece != endTrackPiece)
            {
                var tracks = new List<TrackPiece>();
                foreach (var trackConnection in startTrackPiece.TrackConnections)
                {
                    if (FindNextRailTrack(startTrackPiece, trackConnection, endTrackPiece, distance, tracks))
                    {
                        foreach (var trackPiece in tracks)
                        {
                            Plugin.Log.LogWarning(trackPiece.transform.position.ToString());
                        }

                        foreach (var trackPiece in tracks)
                            _tempPathCorners.AddRange(trackPiece.PathCorners);
                        

                        _tempPathCorners.AddRange(endTrackPiece.PathCorners);
                    
                        

                        return true;
                    }
                }
            }

            
            // _tempPathCorners.Add(start);
            // _tempPathCorners.Add(destination);
            return false;
        }

        private bool FindNextRailTrack(TrackPiece previousTrackPiece, TrackConnection previousTrackConnection, TrackPiece destinationTrackPiece, int distance, List<TrackPiece> tracks)
        {
            distance += 1;
            tracks.Add(previousTrackPiece);
            foreach (var trackConnection in previousTrackPiece.TrackConnections)
            {
                Plugin.Log.LogError(distance + "    " + trackConnection.Direction + "   " + trackConnection.ConnectedTrackPiece + "   " + previousTrackPiece.PathCorners[0]);
                
                // if (distance >= MaxDistance)
                //     return false;

                // if (trackConnection.Direction == Direction2DExtensions.ToOppositeDirection(previousTrackConnection.Direction))
                //  continue;

                if (tracks.Contains(trackConnection.ConnectedTrackPiece))
                    continue;

                if (trackConnection.ConnectedTrackPiece == destinationTrackPiece)
                {
                    // _tempPathCorners.AddRange(previousTrackPiece.PathCorners);
                    return true;
                }
                
                if (trackConnection.ConnectedTrackPiece == null)
                    continue;

                if (FindNextRailTrack(trackConnection.ConnectedTrackPiece, trackConnection, destinationTrackPiece, distance, tracks))
                {
                    // _tempPathCorners.AddRange(previousTrackPiece.PathCorners);
                    return true;
                }
            }

            tracks.Remove(previousTrackPiece);
            return false;
        }
    }
}
