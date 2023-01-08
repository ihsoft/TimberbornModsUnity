// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using Timberborn.BlockSystem;
// using Timberborn.Coordinates;
// using UnityEngine;
//
// namespace ChooChoo
// {
//     public class TrainNavigationService
//     {
//         private readonly TrainDestinationService _trainDestinationService;
//
//         private readonly TrackRouteWeightCache _trackRouteWeightCache;
//
//         private readonly BlockService _blockService;
//
//         private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
//
//         TrainNavigationService(TrainDestinationService trainDestinationService, TrackRouteWeightCache trackRouteWeightCache, BlockService blockService)
//         {
//             _trainDestinationService = trainDestinationService;
//             _trackRouteWeightCache = trackRouteWeightCache;
//             _blockService = blockService;
//         }
//         
//         public bool FindRailTrackPath(Transform transform, TrainDestination destination, ref TrackRoute previouslyLastTrackRoute, List<TrackRoute> tempPathTrackRoutes)
//         {
//             // Plugin.Log.LogWarning("Start finding path");
//             _stopwatch.Restart();
//             // Plugin.Log.LogWarning(new Vector3(start.x, start.z, start.y).ToString());
//             // Plugin.Log.LogWarning(vector3Int.ToString());
//             // Plugin.Log.LogWarning( _blockService.GetFloorObjectComponentAt<TrackPiece>(vector3Int) + "");
//             
//             
//             
//             var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(transform.position.ToBlockServicePosition());
//             if (startTrackPiece == null) 
//                 return false;
//
//             var endTrackPiece = destination.GetComponent<TrackPiece>();
//             // Plugin.Log.LogWarning("endTrackPiece " + (endTrackPiece == null));
//             if (endTrackPiece == null) 
//                 return false;
//             
//             // var facingDirection = transform.eulerAngles.y.ToDirection2D();
//             // var correctedFacingDirection = startTrackPiece.GetComponent<BlockObject>().Orientation.CorrectedTransform(facingDirection);
//             //   
//             // var previousTrackRoute = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == correctedFacingDirection);
//             //
//             // var rightOfCorrectlyFacingDirection = correctedFacingDirection.Next();
//             // var leftOfCorrectlyFacingDirection = correctedFacingDirection.Next().Next();
//             // previousTrackRoute ??= startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == rightOfCorrectlyFacingDirection || route.Exit.Direction == leftOfCorrectlyFacingDirection);
//
//             var previousTrackRoutes = GetFacingDirections(transform, startTrackPiece);
//             
//             
//             // Plugin.Log.LogWarning("startTrackPiece == endTrackPiece: "+ (startTrackPiece == endTrackPiece));
//             if (startTrackPiece == endTrackPiece)
//             {
//                 var previousTrackRoute = previousTrackRoutes.FirstOrDefault();
//
//                 if (previousTrackRoute == null)
//                     return false;
//
//                 tempPathTrackRoutes.Add(previousTrackRoute);
//                 tempPathTrackRoutes.Add(previousTrackRoute);
//                 return true;
//             }
//
//             long elapsedTicks;
//             
//             // Plugin.Log.LogWarning("TrackPieces valid");
//
//             var startTrainDestination = startTrackPiece.GetComponent<TrainDestination>();
//             if (!_trainDestinationService.TrainDestinationsConnected(startTrainDestination, destination))
//             {
//                 // Plugin.Log.LogError("Destinations Not Connected");
//                 if (!_trainDestinationService.DestinationReachable(startTrackPiece, destination))
//                 {
//                     // Plugin.Log.LogError("Destinations Unreachable");
//                     return false;
//                 }
//             }
//        
//
//             foreach (var previousTrackRoute in previousTrackRoutes)
//             {
//                 // Plugin.Log.LogWarning("Finding next trail track");
//                 int? maxDistance = null;
//                 int distance = 0;
//                 var trackRouteWeights = new Dictionary<TrackRoute, int?>(_trackRouteWeightCache.TrackRouteWeights);
//                 FindNextRailTrack(previousTrackRoute, endTrackPiece, trackRouteWeights, distance, ref maxDistance);
//                 
//                 if (maxDistance == null)
//                     continue;
//                 var trackRoutes = new List<TrackRoute>();
//                 if (!FindPathInNodes(previousTrackRoute, endTrackPiece, trackRoutes, trackRouteWeights, (int)maxDistance))
//                     continue;
//
//                 previouslyLastTrackRoute = trackRoutes.Last();
//                 tempPathTrackRoutes.AddRange(trackRoutes);
//                 _stopwatch.Stop();
//                 elapsedTicks = _stopwatch.ElapsedTicks;
//                 Plugin.Log.LogWarning("Path found! Calculation time: " + elapsedTicks + " (10.000 Ticks = 1ms)");
//                 return true;
//             }
//             
//             _stopwatch.Stop();
//             elapsedTicks = _stopwatch.ElapsedTicks;
//             Plugin.Log.LogWarning("No path was found. Calculation time: " + elapsedTicks + " (10.000 Ticks = 1ms)");
//             return false;
//         }
//
//         private IEnumerable<TrackRoute> GetFacingDirections(Transform transform, TrackPiece startTrackPiece)
//         {
//             var list = new List<TrackRoute>();
//             
//             var facingDirection = transform.eulerAngles.y.ToDirection2D();
//             var correctedFacingDirection = startTrackPiece.GetComponent<BlockObject>().Orientation.CorrectedTransform(facingDirection);
//             var previousTrackRoute1 = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == correctedFacingDirection);
//             if (previousTrackRoute1 != null)
//                 list.Add(previousTrackRoute1);
//             
//             
//             var rightOfCorrectlyFacingDirection = correctedFacingDirection.Next();
//             var previousTrackRoute2 = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == rightOfCorrectlyFacingDirection);
//             if (previousTrackRoute2 != null)
//                 list.Add(previousTrackRoute2);
//             
//             
//             var leftOfCorrectlyFacingDirection = correctedFacingDirection.Next().Next();
//             var previousTrackRoute3 = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == leftOfCorrectlyFacingDirection);
//             if (previousTrackRoute3 != null)
//                 list.Add(previousTrackRoute3);
//             
//             // var oppositeOfCorrectlyFacingDirection = correctedFacingDirection.Next().Next().Next();
//             // var previousTrackRoute4 = startTrackPiece.TrackRoutes.FirstOrDefault(route => route.Exit.Direction == oppositeOfCorrectlyFacingDirection);
//             // if (previousTrackRoute4 != null)
//             //     list.Add(previousTrackRoute4);
//
//                  
//             Plugin.Log.LogInfo(transform.eulerAngles + "   " + facingDirection + "      " + correctedFacingDirection + "  " + rightOfCorrectlyFacingDirection + "   " + leftOfCorrectlyFacingDirection);
//
//             
//             return list;
//         }
//
//         private void FindNextRailTrack(TrackRoute previousTrackRoute, TrackPiece destinationTrackPiece, Dictionary<TrackRoute, int?> trackRouteWeights, int distance, ref int? maxDistance)
//         {
//             if (!trackRouteWeights.ContainsKey(previousTrackRoute) || previousTrackRoute.Exit.ConnectedTrackRoutes == null)
//             {
//                 // Plugin.Log.LogWarning("Is null   ");
//                 return;
//             }
//
//             var newDistance = distance + 1;
//
//             if (newDistance > maxDistance)
//             {
//                 return;
//             }
//             
//             if (trackRouteWeights[previousTrackRoute] == null)
//                 trackRouteWeights[previousTrackRoute] = newDistance;
//             else
//             {
//                 var currentWeight = (int)trackRouteWeights[previousTrackRoute];
//                 if (currentWeight <= newDistance)
//                 {
//                     return;
//                 }
//                 trackRouteWeights[previousTrackRoute] = Math.Min(currentWeight, newDistance);
//             }
//
//             // if (!trackRouteWeights.ContainsKey(previousTrackRoute))
//             //     trackRouteWeights.Add(previousTrackRoute, newLength);
//             // else
//             //     trackRouteWeights[previousTrackRoute] = Math.Min(trackRouteWeights[previousTrackRoute], newLength);
//
//             // Plugin.Log.LogError("Checking Route");
//             foreach (var trackConnection in previousTrackRoute.Exit.ConnectedTrackRoutes
//                          .Where(connection => connection.Exit.ConnectedTrackRoutes != null)
//                          .OrderBy(connection => Vector3.Distance(connection.Exit.ConnectedTrackPiece.CenterCoordinates, destinationTrackPiece.CenterCoordinates))
//                      )
//             {
//                 // Plugin.Log.LogWarning("Checking: " + trackConnection.Exit.ConnectedTrackPiece.CenterCoordinates + " Current weight: " + currentWeight + " New distance: " + newLength);
//
//                 if (!trackConnection.Exit.ConnectedTrackPiece.CanPathFindOverIt && !(trackConnection.Exit.ConnectedTrackPiece == destinationTrackPiece))
//                 {
//                     // Plugin.Log.LogError("Cannot pathfind over it");
//                     continue;
//                 }
//
//                 if (trackConnection.Exit.ConnectedTrackPiece == destinationTrackPiece)
//                 {
//                     // Plugin.Log.LogError("Found Destination");
//                     
//                     var newNewDistance = newDistance + 1;
//
//                     if (!trackRouteWeights.ContainsKey(trackConnection))
//                         continue;
//                     
//                     if (trackRouteWeights[trackConnection] == null)
//                         trackRouteWeights[trackConnection] = newNewDistance;
//                     else
//                         trackRouteWeights[trackConnection] = Math.Min((int)trackRouteWeights[trackConnection], newNewDistance);
//
//                     maxDistance = maxDistance == null ? newNewDistance : Math.Min((int)maxDistance, newNewDistance);
//                     break;
//                 }
//
//                 FindNextRailTrack(trackConnection, destinationTrackPiece, trackRouteWeights, newDistance, ref maxDistance);
//             }
//             // Plugin.Log.LogError("Dead end");
//         }
//
//         private bool FindPathInNodes(TrackRoute previousTrackRoute, TrackPiece destinationTrackPiece, List<TrackRoute> trackConnections, Dictionary<TrackRoute, int?> nodes, int maxLength)
//         {
//             // trackRoutes.Add(previousTrackRoute);
//             var test =  FindPath(previousTrackRoute, nodes, destinationTrackPiece, trackConnections, maxLength);
//
//             // Plugin.Log.LogInfo(test + "");
//             return test;
//         }
//
//         private bool FindPath(TrackRoute previousRoute, Dictionary<TrackRoute, int?> nodes, TrackPiece destinationTrackPiece, List<TrackRoute> trackConnections, int maxLength)
//         {
//
//             if (!nodes.ContainsKey(previousRoute) || previousRoute.Exit.ConnectedTrackRoutes == null || nodes[previousRoute] > maxLength)
//             {
//                 // Plugin.Log.LogWarning("Is null   ");
//                 return false;
//             }
//
//             trackConnections.Add(previousRoute);
//             
//             foreach (var trackRoute in previousRoute.Exit.ConnectedTrackRoutes
//                      .Where(connection => connection.Exit.ConnectedTrackRoutes != null)
//                      .OrderBy(connection => Vector3.Distance(connection.Exit.ConnectedTrackPiece.CenterCoordinates, destinationTrackPiece.CenterCoordinates))
//                     )
//             {
//                 // Plugin.Log.LogError("Checking route");
//                 if (!nodes.ContainsKey(trackRoute))
//                     continue;
//
//                 if (nodes[previousRoute] + 1 == nodes[trackRoute])
//                 {
//                     // var test = "Route found: " + trackRouteWeights[trackRoute] + " ";
//                     // if (trackRoute.Exit.ConnectedTrackPiece != null)
//                     //     test += trackRoute.Exit.ConnectedTrackPiece.CenterCoordinates;
//                     // Plugin.Log.LogError(test);
//                     if (trackRoute.Exit.ConnectedTrackPiece == destinationTrackPiece)
//                     {
//                         // Plugin.Log.LogError("Found Destination");
//                         trackConnections.Add(trackRoute);
//                         if (!trackRoute.Exit.ConnectedTrackRoutes.Any())
//                             return true;
//                         
//                         trackConnections.Add(trackRoute.Exit.ConnectedTrackRoutes[0]);
//                         return true;
//                     }
//
//                     if (FindPath(trackRoute, nodes, destinationTrackPiece, trackConnections, maxLength))
//                     {
//                         return true;   
//                     }
//                 }
//             }
//             
//             trackConnections.Remove(previousRoute);
//             return false;
//         }
//     }
// }
