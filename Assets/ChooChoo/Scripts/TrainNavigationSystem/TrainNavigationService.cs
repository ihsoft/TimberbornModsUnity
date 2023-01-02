using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    public class TrainNavigationService
    {
        private readonly TrainDestinationService _trainDestinationService;

        private readonly BlockService _blockService;

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        TrainNavigationService(TrainDestinationService trainDestinationService, BlockService blockService)
        {
            _trainDestinationService = trainDestinationService;
            _blockService = blockService;
        }
        
        public bool FindRailTrackPath(Transform transform, TrainDestination destination, ref TrackRoute previouslyLastTrackRoute, List<TrackRoute> tempPathTrackRoutes)
        {
            // Plugin.Log.LogWarning("Start finding path");
            _stopwatch.Restart();
            var start = transform.position;

            var vector3Int = new Vector3Int((int)Math.Floor(start.x), (int)Math.Floor(start.z), (int)Math.Round(start.y));
            
            // Plugin.Log.LogWarning(new Vector3(start.x, start.z, start.y).ToString());
            // Plugin.Log.LogWarning(vector3Int.ToString());
            // Plugin.Log.LogWarning( _blockService.GetFloorObjectComponentAt<TrackPiece>(vector3Int) + "");
            
            var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(vector3Int);
            if (startTrackPiece == null) 
                return false;
            var facingDirection = transform.eulerAngles.y.ToDirection2D();
            var correctedFacingDirection = startTrackPiece.GetComponent<BlockObject>().Orientation.CorrectedTransform(facingDirection);
            
            var previousTrackRoute = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == correctedFacingDirection);

            var rightOfCorrectlyFacingDirection = correctedFacingDirection.Next();
            var leftOfCorrectlyFacingDirection = correctedFacingDirection.Next().Next();
            previousTrackRoute ??= startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == rightOfCorrectlyFacingDirection || route.Exit.Direction == leftOfCorrectlyFacingDirection);
            // Plugin.Log.LogInfo(transform.eulerAngles + "   " + facingDirection + "      " + correctedFacingDirection + "  " + rightOfCorrectlyFacingDirection + "   " + leftOfCorrectlyFacingDirection);
            
            // var previousTrackRoute = startTrackPiece.TrackRoutes.OrderByDescending(route =>
            // {
            //     var badCoords = route.Exit.ConnectedTrackPiece.CenterCoordinates;
            //     var goodCoords = new Vector3(badCoords.x, badCoords.z, badCoords.y);
            //     var angle = Vector3.SignedAngle(start, goodCoords, Vector3.up);
            //     Plugin.Log.LogInfo(angle + "");
            //     return angle;
            // }).FirstOrDefault();
            if (previousTrackRoute == null)
                return false;
            var endTrackPiece = destination.GetComponent<TrackPiece>();
            // Plugin.Log.LogWarning((startTrackPiece == null) +"      "+ (endTrackPiece == null) +"     "+ (startTrackPiece == endTrackPiece));
            if (endTrackPiece == null || startTrackPiece == endTrackPiece) 
                return false;
            // Plugin.Log.LogWarning("TrackPieces valid");

            var startTrainDestination = startTrackPiece.GetComponent<TrainDestination>();
            if (!_trainDestinationService.TrainDestinationsConnected(startTrainDestination, destination))
            {
                // Plugin.Log.LogWarning("Destinations Not Connected");
                if (!_trainDestinationService.DestinationReachable(startTrackPiece, destination))
                {
                    // Plugin.Log.LogWarning("Destinations Unreachable");
                    return false;
                }
            }
                
            // Plugin.Log.LogWarning("Finding next trail track");
    
            var trackRoutes = new List<TrackRoute>();
            if (!FindNextRailTrack(previousTrackRoute, endTrackPiece, trackRoutes))
                return false;

            // foreach (var trackPiece in tracks)
            // {
            //     Plugin.Log.LogWarning(trackPiece.transform.position.ToString());
            // }
            // trackConnections.Reverse();
            previouslyLastTrackRoute = trackRoutes.Last();
            tempPathTrackRoutes.AddRange(trackRoutes);
            // tempPathTrackConnections.Add(endTrackPiece.TrackConnections[0]);
            // _tempPathCorners.Add(start);
            // _tempPathCorners.Add(destination);

            _stopwatch.Stop();
            Plugin.Log.LogWarning(_stopwatch.ElapsedTicks.ToString());
            
            return true;

        }

        private bool FindNextRailTrack(TrackRoute previousTrackRoute, TrackPiece destinationTrackPiece, List<TrackRoute> trackConnections)
        {
            if (previousTrackRoute.Exit.ConnectedTrackRoutes == null)
            {
                // Plugin.Log.LogWarning("Is null   ");
                return false;
            }
            
            // Plugin.Log.LogError("Checking Route");
            trackConnections.Add(previousTrackRoute);
            foreach (var trackConnection in previousTrackRoute.Exit.ConnectedTrackRoutes
                         .Where(connection => connection.Exit.ConnectedTrackRoutes != null)
                         .OrderBy(connection => Vector3.Distance(connection.Exit.ConnectedTrackPiece.CenterCoordinates, destinationTrackPiece.CenterCoordinates)))
            {
                // Plugin.Log.LogError("Checking Route at: " + trackConnection.Exit.ConnectedTrackPiece.CenterCoordinates);

                // if (trackConnection.Entrance.ConnectedTrackPiece == previousTrackPiece)
                // {
                //     continue;
                // }

                if (!trackConnection.Exit.ConnectedTrackPiece.CanPathFindOverIt && !(trackConnection.Exit.ConnectedTrackPiece == destinationTrackPiece))
                {
                    // Plugin.Log.LogError("Cannot pathfind over it");
                    continue;
                }
                
                if (trackConnections.Contains(trackConnection))
                {
                    // Plugin.Log.LogError("Route already exists");
                    continue;
                }

                if (trackConnection.Exit.ConnectedTrackPiece == destinationTrackPiece)
                {
                    // Plugin.Log.LogError("Found Destination");
                    trackConnections.Add(trackConnection);
                    trackConnections.Add(trackConnection.Exit.ConnectedTrackRoutes[0]);
                    // foreach (var VARIABLE in trackConnection.ConnectedTrackConnection.PathCorners)
                    // {
                    //     Plugin.Log.LogWarning(VARIABLE.ToString());
                    // }
                    return true;
                }

                if (FindNextRailTrack(trackConnection, destinationTrackPiece, trackConnections))
                {
                    return true;
                }
            }
            // Plugin.Log.LogError("Dead end");
            trackConnections.Remove(previousTrackRoute);
            return false;
        }
    }
}
