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
        
        public bool FindRailTrackPath(Transform transform, TrainDestination destination, List<TrackRoute> tempPathTrackRoutes, bool isStuck)
        {
            // Plugin.Log.LogWarning("Start finding path");
            _stopwatch.Restart();
            if (destination == null)
                return false;
            var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(transform.position.ToBlockServicePosition());
            if (startTrackPiece == null) 
                return false;
            var endTrackPiece = destination.GetComponent<TrackPiece>();
            // Plugin.Log.LogWarning("endTrackPiece " + (endTrackPiece == null));
            if (endTrackPiece == null) 
                return false;
            
            // Plugin.Log.LogWarning("TrackPieces valid");

            var startTrainDestination = startTrackPiece.GetComponent<TrainDestination>();
            if (!_trainDestinationService.TrainDestinationsConnected(startTrainDestination, destination))
            {
                // Plugin.Log.LogError("Destinations Not Connected");
                if (!_trainDestinationService.DestinationReachable(startTrackPiece, destination))
                {
                    // Plugin.Log.LogError("Destinations Unreachable");
                    return false;
                }
            }

            var facingDirectionRoutes = GetFacingDirections(transform, startTrackPiece, isStuck);

            foreach (var trackRoute in facingDirectionRoutes)
            {
                // Plugin.Log.LogWarning("startTrackPiece == endTrackPiece: "+ (startTrackPiece == endTrackPiece));
                if (startTrackPiece == endTrackPiece)
                {
                    tempPathTrackRoutes.Add(trackRoute);
                    tempPathTrackRoutes.Add(trackRoute);
                    return true;
                }
                
                // Plugin.Log.LogWarning("Finding next trail track");
                int? maxDistance = null;
                int distance = 0;
                var trackRouteWeights = new Dictionary<TrackRoute, int?>(_trackRouteWeightCache.TrackRouteWeights);
                FindNextRailTrack(trackRoute, endTrackPiece, trackRouteWeights, distance, ref maxDistance);
            
                if (maxDistance == null)
                    continue;
                // _stopwatch.Stop();
                // var firstPart = _stopwatch.ElapsedTicks;
                // _stopwatch.Restart();
                var trackRoutes = new List<TrackRoute>();

                var test = !FindPathInNodes(trackRoute, endTrackPiece, trackRoutes, trackRouteWeights,
                    (int)maxDistance);
                // Plugin.Log.LogWarning("Find Path in nodes: " + test);
                if (test)
                    continue;
            
                // if (!FindNextRailTrack(previousTrackRoute, endTrackPiece, trackRoutes, trackRouteWeights, previousDistance))
                //     return false;
            
                tempPathTrackRoutes.AddRange(trackRoutes);
                
                // _stopwatch.Stop();
                // var secondPart = _stopwatch.ElapsedTicks;
                // Plugin.Log.LogWarning("First: " + firstPart + " Second: " + secondPart + " Total: " + (firstPart + secondPart) + " (10.000 Ticks = 1ms)");
                return true;
            }
            
            // Plugin.Log.LogInfo("Couldnt find path");
            
            // _stopwatch.Stop();
            // var secondPart = _stopwatch.ElapsedTicks;
            // Plugin.Log.LogWarning("First: " + firstPart + " Second: " + secondPart + " Total: " + (firstPart + secondPart) + " (10.000 Ticks = 1ms)");
            return false;
        }
            
         private IEnumerable<TrackRoute> GetFacingDirections(Transform transform, TrackPiece startTrackPiece, bool isStuck)
         {
             var list = new List<TrackRoute>();
             
             var facingDirection = transform.eulerAngles.y.ToDirection2D();
             var correctedFacingDirection = startTrackPiece.GetComponent<BlockObject>().Orientation.CorrectedTransform(facingDirection);
             var previousTrackRoute1 = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == correctedFacingDirection);
             if (previousTrackRoute1 != null)
                 list.Add(previousTrackRoute1);
             
             
             var rightOfCorrectlyFacingDirection = correctedFacingDirection.Next();
             var previousTrackRoute2 = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == rightOfCorrectlyFacingDirection);
             if (previousTrackRoute2 != null)
                 list.Add(previousTrackRoute2);
             
             
             var leftOfCorrectlyFacingDirection = correctedFacingDirection.Next().Next().Next();
             var previousTrackRoute3 = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == leftOfCorrectlyFacingDirection);
             if (previousTrackRoute3 != null)
                 list.Add(previousTrackRoute3);

             Direction2D? oppositeOfCorrectlyFacingDirection = null;
             
             if (isStuck)
             {
                 oppositeOfCorrectlyFacingDirection = correctedFacingDirection.Next().Next();
                 var previousTrackRoute4 = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == oppositeOfCorrectlyFacingDirection);
                 if (previousTrackRoute4 != null)
                     list.Add(previousTrackRoute4);
             }

             // Plugin.Log.LogInfo(transform.eulerAngles + "   " + facingDirection + "      " + correctedFacingDirection + "  " + rightOfCorrectlyFacingDirection + "   " + leftOfCorrectlyFacingDirection + "    " + oppositeOfCorrectlyFacingDirection);
             
             return list;
         }

        private void FindNextRailTrack(TrackRoute previousTrackRoute, TrackPiece destinationTrackPiece, Dictionary<TrackRoute, int?> trackRouteWeights, int previousDistance, ref int? maxDistance)
        {
            if (!trackRouteWeights.ContainsKey(previousTrackRoute) || previousTrackRoute.Exit.ConnectedTrackRoutes == null)
            {
                // Plugin.Log.LogWarning("Is null   ");
                return;
            }

            var currentDistance = previousDistance + previousTrackRoute.Exit.ConnectedTrackPiece.TrackDistance;

            if (currentDistance > maxDistance)
                return;
            
            if (trackRouteWeights[previousTrackRoute] == null)
                trackRouteWeights[previousTrackRoute] = currentDistance;
            else
            {
                var weight = (int)trackRouteWeights[previousTrackRoute];
                if (weight <= currentDistance)
                    return;
                trackRouteWeights[previousTrackRoute] = Math.Min(weight, currentDistance);
            }

            // Plugin.Log.LogError("Checking Route");
            foreach (var trackRoute in previousTrackRoute.Exit.ConnectedTrackRoutes
                         .Where(trackRoute => trackRoute.Exit.ConnectedTrackRoutes != null)
                         .OrderBy(trackRoute => Vector3.Distance(trackRoute.Exit.ConnectedTrackPiece.CenterCoordinates, destinationTrackPiece.CenterCoordinates))
                     )
            {
                // Plugin.Log.LogWarning("Checking: " + trackRoute.Exit.ConnectedTrackPiece.CenterCoordinates + " Current weight: " + trackRouteWeights[trackRoute] + " New previousDistance: " + newDistance);

                if (!trackRoute.Exit.ConnectedTrackPiece.CanPathFindOverIt && !(trackRoute.Exit.ConnectedTrackPiece == destinationTrackPiece))
                {
                    // Plugin.Log.LogError("Cannot pathfind over it");
                    continue;
                }

                if (trackRoute.Exit.ConnectedTrackPiece == destinationTrackPiece)
                {
                    // Plugin.Log.LogError("Found Destination");
                    
                    var newNewDistance = currentDistance + trackRoute.Exit.ConnectedTrackPiece.TrackDistance;

                    if (!trackRouteWeights.ContainsKey(trackRoute))
                        continue;
                    
                    if (trackRouteWeights[trackRoute] == null)
                        trackRouteWeights[trackRoute] = newNewDistance;
                    else
                        trackRouteWeights[trackRoute] = Math.Min((int)trackRouteWeights[trackRoute], newNewDistance);

                    maxDistance = maxDistance == null ? newNewDistance : Math.Min((int)maxDistance, newNewDistance);
                    break;
                }

                FindNextRailTrack(trackRoute, destinationTrackPiece, trackRouteWeights, currentDistance, ref maxDistance);
            }
            // Plugin.Log.LogError("Dead end");
        }

        private bool FindPathInNodes(TrackRoute previousTrackRoute, TrackPiece destinationTrackPiece, List<TrackRoute> trackConnections, Dictionary<TrackRoute, int?> nodes, int maxLength)
        {
            // trackRoutes.Add(previousTrackRoute);
            var test =  FindPath(previousTrackRoute, nodes, destinationTrackPiece, trackConnections, maxLength);

            // Plugin.Log.LogInfo(test + "");
            return test;
        }

        private bool FindPath(TrackRoute previousRoute, Dictionary<TrackRoute, int?> nodes, TrackPiece destinationTrackPiece, List<TrackRoute> trackConnections, int maxLength)
        {
            // Plugin.Log.LogInfo(nodes[previousRoute] + "      " + maxLength);
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
                // Plugin.Log.LogError("Checking route: " + nodes[trackRoute] + "  " + nodes[previousRoute] + "  " + trackRoute.Exit.ConnectedTrackPiece.TrackDistance);
                if (!nodes.ContainsKey(trackRoute))
                    continue;
                if (nodes[previousRoute] + trackRoute.Exit.ConnectedTrackPiece.TrackDistance == nodes[trackRoute])
                {
                    // var test = "Route found: " + nodes[trackRoute] + " ";
                    // if (trackRoute.Exit.ConnectedTrackPiece != null)
                    //     test += trackRoute.Exit.ConnectedTrackPiece.CenterCoordinates;
                    // Plugin.Log.LogWarning(test);
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
