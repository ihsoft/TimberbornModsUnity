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
            _stopwatch.Restart();
            if (destination == null)
                return false;
            var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(transform.position.ToBlockServicePosition());
            var endTrackPiece = destination.GetComponent<TrackPiece>();
            if (startTrackPiece == null || endTrackPiece == null) 
                return false;
            
            // Plugin.Log.LogWarning("TrackPieces valid");

            var startTrainDestination = startTrackPiece.GetComponent<TrainDestination>();
            if (!_trainDestinationService.TrainDestinationsConnectedOneWay(startTrainDestination, destination))
            {
                // Plugin.Log.LogError("Destinations Not Connected");
                if (!_trainDestinationService.DestinationReachableOneWay(startTrackPiece, destination))
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
                int? maxDistance = null;
                int distance = 0;
                var trackRouteWeights = new Dictionary<TrackRoute, int?>(_trackRouteWeightCache.TrackRouteWeights);
                FindNextRailTrack(trackRoute, endTrackPiece, trackRouteWeights, distance, ref maxDistance);
            
                if (maxDistance == null)
                    continue;
                var trackRoutes = new List<TrackRoute>();
                if (!FindPath(trackRoute, trackRouteWeights, endTrackPiece, trackRoutes, (int)maxDistance))
                    continue;

                tempPathTrackRoutes.AddRange(trackRoutes);
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
             var directionalTrackRoutes = new List<TrackRoute>();
             var facingDirection = transform.eulerAngles.y.ToDirection2D();
             var correctedFacingDirection = startTrackPiece.GetComponent<BlockObject>().Orientation.CorrectedTransform(facingDirection);
             AddFacingTrackRoute(correctedFacingDirection, directionalTrackRoutes, startTrackPiece);
             var rightOfCorrectlyFacingDirection = correctedFacingDirection.Next();
             AddFacingTrackRoute(rightOfCorrectlyFacingDirection, directionalTrackRoutes, startTrackPiece);
             var leftOfCorrectlyFacingDirection = correctedFacingDirection.Next().Next().Next();
             AddFacingTrackRoute(leftOfCorrectlyFacingDirection, directionalTrackRoutes, startTrackPiece);
             Direction2D? oppositeOfCorrectlyFacingDirection = null;
             if (!isStuck) 
                 return directionalTrackRoutes;
             oppositeOfCorrectlyFacingDirection = correctedFacingDirection.Next().Next();
             AddFacingTrackRoute((Direction2D)oppositeOfCorrectlyFacingDirection, directionalTrackRoutes, startTrackPiece);
             // Plugin.Log.LogInfo(transform.eulerAngles + "   " + facingDirection + "      " + correctedFacingDirection + "  " + rightOfCorrectlyFacingDirection + "   " + leftOfCorrectlyFacingDirection + "    " + oppositeOfCorrectlyFacingDirection);
             return directionalTrackRoutes;
         }

         private void AddFacingTrackRoute(Direction2D direction, List<TrackRoute> directionalTrackRoutes, TrackPiece startTrackPiece)
         {
             var trackRoute = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == direction);
             if (trackRoute != null)
                 directionalTrackRoutes.Add(trackRoute);
         }

        private void FindNextRailTrack(TrackRoute previousTrackRoute, TrackPiece destinationTrackPiece, Dictionary<TrackRoute, int?> trackRouteWeights, int previousDistance, ref int? maxDistance)
        {
            if (!trackRouteWeights.ContainsKey(previousTrackRoute) || previousTrackRoute.Exit.ConnectedTrackRoutes == null)
                return;

            var currentDistance = previousDistance + previousTrackRoute.Exit.ConnectedTrackPiece.TrackDistance;

            if (currentDistance > maxDistance)
                return;
            
            if (trackRouteWeights[previousTrackRoute] == null)
            {
                trackRouteWeights[previousTrackRoute] = currentDistance;
            }
            else
            {
                var previousWeight = (int)trackRouteWeights[previousTrackRoute];
                if (previousWeight <= currentDistance)
                    return;
                trackRouteWeights[previousTrackRoute] = Math.Min(previousWeight, currentDistance);
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
                    foreach (var destinationTrackRoute in destinationTrackPiece.TrackRoutes)
                    {
                        if (destinationTrackRoute.Entrance.ConnectedTrackPiece == previousTrackRoute.Exit.ConnectedTrackPiece)
                        {
                            // Plugin.Log.LogError("Found Destination");

                            var newNewDistance = currentDistance + trackRoute.Exit.ConnectedTrackPiece.TrackDistance;

                            if (!trackRouteWeights.ContainsKey(trackRoute))
                                continue;

                            if (trackRouteWeights[trackRoute] == null)
                                trackRouteWeights[trackRoute] = newNewDistance;
                            else
                                trackRouteWeights[trackRoute] =
                                    Math.Min((int)trackRouteWeights[trackRoute], newNewDistance);

                            maxDistance = maxDistance == null
                                ? newNewDistance
                                : Math.Min((int)maxDistance, newNewDistance);
                            break;
                        }
                    }
                }

                FindNextRailTrack(trackRoute, destinationTrackPiece, trackRouteWeights, currentDistance, ref maxDistance);
            }
            // Plugin.Log.LogError("Dead end");
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
                        foreach (var destinationTrackRoute in destinationTrackPiece.TrackRoutes)
                        {
                            if (destinationTrackRoute.Entrance.ConnectedTrackPiece == previousRoute.Exit.ConnectedTrackPiece)
                            {
                                // Plugin.Log.LogError("Found Destination");
                                trackConnections.Add(trackRoute);
                                if (!trackRoute.Exit.ConnectedTrackRoutes.Any())
                                    return true;
                        
                                trackConnections.Add(trackRoute.Exit.ConnectedTrackRoutes[0]);
                                return true;
                            }
                        }

                        return false;
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
