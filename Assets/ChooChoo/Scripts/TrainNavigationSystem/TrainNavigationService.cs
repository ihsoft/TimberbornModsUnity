using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    public class TrainNavigationService
    {
        private readonly TrainDestinationService _trainDestinationService;

        private readonly TrackRouteWeightCache _trackRouteWeightCache;

        private readonly BlockService _blockService;

        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        TrainNavigationService(TrainDestinationService trainDestinationService, TrackRouteWeightCache trackRouteWeightCache, BlockService blockService)
        {
            _trainDestinationService = trainDestinationService;
            _trackRouteWeightCache = trackRouteWeightCache;
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
            int? maxLength = null;
            int length = 0;
            var trackRoutes = new List<TrackRoute>();
            var nodes = new Dictionary<TrackRoute, int?>(_trackRouteWeightCache.TrackRouteWeights);
            FindNextRailTrack(previousTrackRoute, endTrackPiece, trackRoutes, nodes, length, ref maxLength);
            
            if (maxLength == null)
                return false;
            _stopwatch.Stop();
            var firstPart = _stopwatch.ElapsedTicks;
            _stopwatch.Restart();
            if (!FindPathInNodes(previousTrackRoute, endTrackPiece, trackRoutes, nodes, (int)maxLength))
                return false;
            
            // if (!FindNextRailTrack(previousTrackRoute, endTrackPiece, trackRoutes, nodes, length))
            //     return false;

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
            var secondPart = _stopwatch.ElapsedTicks;
            Plugin.Log.LogWarning("First: " + firstPart + " Second: " + secondPart + " Total: " + (firstPart + secondPart) + " (10.000 Ticks = 1ms)");
            return true;

        }

        private void FindNextRailTrack(TrackRoute previousTrackRoute, TrackPiece destinationTrackPiece, List<TrackRoute> trackConnections, Dictionary<TrackRoute, int?> nodes, int length, ref int? maxLength)
        {
            if (!nodes.ContainsKey(previousTrackRoute) || previousTrackRoute.Exit.ConnectedTrackRoutes == null)
            {
                // Plugin.Log.LogWarning("Is null   ");
                return;
            }

            var newLength = length + 1;

            if (newLength > maxLength)
            {
                return;
            }
            
            if (nodes[previousTrackRoute] == null)
                nodes[previousTrackRoute] = newLength;
            else
            {
                var currentWeight = (int)nodes[previousTrackRoute];
                if (currentWeight <= newLength)
                {
                    return;
                }
                nodes[previousTrackRoute] = Math.Min(currentWeight, newLength);
            }

            // if (!nodes.ContainsKey(previousTrackRoute))
            //     nodes.Add(previousTrackRoute, newLength);
            // else
            //     nodes[previousTrackRoute] = Math.Min(nodes[previousTrackRoute], newLength);

            // Plugin.Log.LogError("Checking Route");
            // trackConnections.Add(previousTrackRoute);
            foreach (var trackConnection in previousTrackRoute.Exit.ConnectedTrackRoutes
                         .Where(connection => connection.Exit.ConnectedTrackRoutes != null)
                         .OrderBy(connection => Vector3.Distance(connection.Exit.ConnectedTrackPiece.CenterCoordinates, destinationTrackPiece.CenterCoordinates))
                     )
            {
                // var currentWeight = nodes[trackConnection];
                
                
                // Plugin.Log.LogWarning("Checking: " + trackConnection.Exit.ConnectedTrackPiece.CenterCoordinates + " Current weight: " + currentWeight + " New length: " + newLength);

                // if (currentWeight <= newLength)
                // {
                //     continue;
                // }

                if (!trackConnection.Exit.ConnectedTrackPiece.CanPathFindOverIt && !(trackConnection.Exit.ConnectedTrackPiece == destinationTrackPiece))
                {
                    // Plugin.Log.LogError("Cannot pathfind over it");
                    continue;
                }
                
                // if (trackConnections.Contains(trackConnection))
                // {
                //     // Plugin.Log.LogError("Route already exists");
                //     continue;
                // }

                if (trackConnection.Exit.ConnectedTrackPiece == destinationTrackPiece)
                {
                    // Plugin.Log.LogError("Found Destination");
                    // trackConnections.Add(trackConnection);
                    // trackConnections.Add(trackConnection.Exit.ConnectedTrackRoutes[0]);
                    
                    var newNewLength = newLength + 1;
                    
                    if (nodes[trackConnection] == null)
                        nodes[trackConnection] = newNewLength;
                    else
                        nodes[trackConnection] = Math.Min((int)nodes[trackConnection], newNewLength);

                    // var destination = trackConnection.Exit.ConnectedTrackRoutes[0];
                    // if (nodes[destination] == null)
                    //     nodes[destination] = newNewLength;
                    // else
                    //     nodes[destination] = Math.Min((int)nodes[trackConnection], newNewLength);
                    
                    // if (!nodes.ContainsKey(trackConnection))
                    //     nodes.Add(trackConnection, newLength);
                    // else
                    //     nodes[trackConnection] = Math.Min(nodes[trackConnection], newLength);
                    
                    maxLength = maxLength == null ? newNewLength : Math.Min((int)maxLength, newNewLength);
                    break;
                    // foreach (var VARIABLE in trackConnection.ConnectedTrackConnection.PathCorners)
                    // {
                    //     Plugin.Log.LogWarning(VARIABLE.ToString());
                    // }
                    // return true;
                }

                FindNextRailTrack(trackConnection, destinationTrackPiece, trackConnections, nodes, newLength, ref maxLength);
                // if (FindNextRailTrack(trackConnection, destinationTrackPiece, trackConnections, nodes, newLength))
                // {
                //     return true;
                // }
            }
            // Plugin.Log.LogError("Dead end");
            // trackConnections.Remove(previousTrackRoute);
        }

        private bool FindPathInNodes(TrackRoute previousTrackRoute, TrackPiece destinationTrackPiece, List<TrackRoute> trackConnections, Dictionary<TrackRoute, int?> nodes, int maxLength)
        {
            trackConnections.Clear();
            
            trackConnections.Add(previousTrackRoute);
            var test =  FindPath(previousTrackRoute, nodes, destinationTrackPiece, trackConnections, maxLength);

            // Plugin.Log.LogInfo(test + "");
            return test;
        }

        private bool FindPath(TrackRoute previousRoute, Dictionary<TrackRoute, int?> nodes, TrackPiece destinationTrackPiece, List<TrackRoute> trackConnections, int maxLength)
        {

            if (!nodes.ContainsKey(previousRoute) || previousRoute.Exit.ConnectedTrackRoutes == null || nodes[previousRoute] > maxLength)
            {
                // Plugin.Log.LogWarning("Is null   ");
                return false;
            }

            trackConnections.Add(previousRoute);
            
            foreach (var trackRoute in previousRoute.Exit.ConnectedTrackRoutes
                     .Where(connection => connection.Exit.ConnectedTrackRoutes != null)
                     .OrderBy(connection => Vector3.Distance(connection.Exit.ConnectedTrackPiece.CenterCoordinates, destinationTrackPiece.CenterCoordinates))
                    )
            {
                // Plugin.Log.LogError("Checking route");
                if (!nodes.ContainsKey(trackRoute))
                    continue;

                if (nodes[previousRoute] + 1 == nodes[trackRoute])
                {
                    // var test = "Route found: " + nodes[trackRoute] + " ";
                    // if (trackRoute.Exit.ConnectedTrackPiece != null)
                    //     test += trackRoute.Exit.ConnectedTrackPiece.CenterCoordinates;
                    // Plugin.Log.LogError(test);
                    if (trackRoute.Exit.ConnectedTrackPiece == destinationTrackPiece)
                    {
                        // Plugin.Log.LogError("Found Destination");
                        trackConnections.Add(trackRoute);
                        if (!trackRoute.Exit.ConnectedTrackRoutes.Any())
                            return true;
                        
                        trackConnections.Add(trackRoute.Exit.ConnectedTrackRoutes[0]);
                        return true;
                    }

                    if (FindPath(trackRoute, nodes, destinationTrackPiece, trackConnections, maxLength))
                    {
                        return true;   
                    }
                }
            }
            
            trackConnections.Remove(previousRoute);
            return false;
        }
    }
}
