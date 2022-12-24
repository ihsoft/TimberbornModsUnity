using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Timberborn.BlockSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainNavigationService
    {
        private readonly TrainDestinationService _trainDestinationService;

        private readonly BlockService _blockService;

        private int MaxDistance = 50;
        
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        TrainNavigationService(TrainDestinationService trainDestinationService, BlockService blockService)
        {
            _trainDestinationService = trainDestinationService;
            _blockService = blockService;
        }
        
        public bool FindRailTrackPath(Vector3 start, TrainDestination destination, ref TrackConnection previouslyLastTrackConnection, List<TrackConnection> tempPathTrackConnections)
        {
            _stopwatch.Restart();

            var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(Vector3Int.FloorToInt(new Vector3(start.x, start.z, start.y)));
            var endTrackPiece = destination.GetComponent<TrackPiece>();
            if (startTrackPiece == null || endTrackPiece == null || startTrackPiece == endTrackPiece) 
                return false;

            var startTrainDestination = startTrackPiece.GetComponent<TrainDestination>();
            if (!_trainDestinationService.TrainDestinationsConnected(startTrainDestination, destination))
                if (!_trainDestinationService.DestinationReachable(startTrackPiece, destination))
                    return false;

            var trackConnections = new List<TrackConnection>();
            if (!FindNextRailTrack(previouslyLastTrackConnection, startTrackPiece, endTrackPiece, trackConnections))
                return false;

            // foreach (var trackPiece in tracks)
            // {
            //     Plugin.Log.LogWarning(trackPiece.transform.position.ToString());
            // }
            // trackConnections.Reverse();
            previouslyLastTrackConnection = GetNextTrackConnection(trackConnections[^2], trackConnections[^3].ConnectedTrackPiece,
                endTrackPiece);
            tempPathTrackConnections.AddRange(trackConnections);
            // tempPathTrackConnections.Add(endTrackPiece.TrackConnections[0]);
            // _tempPathCorners.Add(start);
            // _tempPathCorners.Add(destination);

            _stopwatch.Stop();
            Plugin.Log.LogWarning(_stopwatch.ElapsedTicks.ToString());
            
            return true;

        }

        private bool FindNextRailTrack(TrackConnection previousTrackConnection, TrackPiece previousTrackPiece, TrackPiece destinationTrackPiece, List<TrackConnection> trackConnections)
        {
            trackConnections.Add(previousTrackConnection);
            foreach (var trackConnection in previousTrackConnection.ConnectedTrackPiece.TrackConnections
                         .Where(connection => connection.ConnectedTrackPiece != null)
                         .OrderBy(connection => Vector3.Distance(connection.ConnectedTrackPiece.CenterCoordinates, destinationTrackPiece.CenterCoordinates)))
            {
                // Plugin.Log.LogError(trackConnection.Direction + "   " + trackConnection.ConnectedTrackPiece.CenterCoordinates + "   " + previousTrackPiece.CenterCoordinates);

                if (trackConnection.ConnectedTrackPiece == previousTrackPiece)
                {
                    continue;
                }
                
                if (trackConnections.Contains(trackConnection))
                {
                    continue;
                }

                if (trackConnection.ConnectedTrackPiece == destinationTrackPiece)
                {
                    trackConnections.Add(trackConnection);
                    trackConnections.Add(trackConnection.ConnectedTrackConnection);
                    // foreach (var VARIABLE in trackConnection.ConnectedTrackConnection.PathCorners)
                    // {
                    //     Plugin.Log.LogWarning(VARIABLE.ToString());
                    // }
                    return true;
                }

                if (FindNextRailTrack(trackConnection, previousTrackConnection.ConnectedTrackPiece, destinationTrackPiece, trackConnections))
                {
                    return true;
                }
            }

            trackConnections.Remove(previousTrackConnection);
            return false;
        }

        private TrackConnection GetNextTrackConnection(TrackConnection previousTrackConnection, TrackPiece previousTrackPiece, TrackPiece destinationTrackPiece)
        {
            foreach (var trackConnection in previousTrackConnection.ConnectedTrackPiece.TrackConnections)
            {
                if (trackConnection.ConnectedTrackPiece == previousTrackPiece)
                {
                    continue;
                }
                
                return trackConnection;
            }

            return null;
        }
    }
}
