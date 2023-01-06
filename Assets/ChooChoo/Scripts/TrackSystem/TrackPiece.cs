using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrackPiece : MonoBehaviour, IFinishedStateListener
    {
        private BlockService _blockService;

        private EventBus _eventBus;

        private TrackArrayProvider _trackArrayProvider;

        private TrackRouteWeightCache _trackRouteWeightCache;

        private BlockObject _blockObject;

        private BlockObjectCenter _blockObjectCenter;

        private TrackRoute[] _trackConnections;

        public TrackSection TrackSection;

        public Vector3 CenterCoordinates { get; private set; }
        
        public bool CanPathFindOverIt { get; set; }

        public bool IsIntersection { get; set; }

        [Inject]
        public void InjectDependencies(
            BlockService blockService, 
            EventBus eventBus, 
            TrackArrayProvider trackArrayProvider, 
            TrackRouteWeightCache trackRouteWeightCache)
        {
            _blockService = blockService;
            _eventBus = eventBus;
            _trackArrayProvider = trackArrayProvider;
            _trackRouteWeightCache = trackRouteWeightCache;
        }
        
        public TrackRoute[] TrackRoutes
        {
            get
            {
                if (_trackConnections == null || !Application.isPlaying)
                {
                    var trackRoutes = _trackArrayProvider.GetConnections(gameObject.name);
                    foreach (var trackRoute in trackRoutes)
                    {
                        _trackRouteWeightCache.Add(trackRoute);
                        var position = _blockObjectCenter.WorldCenterGrounded;
                        trackRoute.RouteCorners = trackRoute.RouteCorners.Select(vector3 => _blockObject.Orientation.TransformInWorldSpace(vector3) + position).ToArray();
                    }
                    _trackConnections = trackRoutes;
                }
                return _trackConnections;
            }
        }

        void Awake()
        {
            TrackSection = new TrackSection(this);
            _blockObject = GetComponent<BlockObject>();
            _blockObjectCenter = GetComponent<BlockObjectCenter>();
            CanPathFindOverIt = !TryGetComponent(out TrainWaitingLocation _);
        }

        public void OnEnterFinishedState()
        {
            LookForTrackSection();
            CenterCoordinates = GetComponent<BlockObjectCenter>().WorldCenterGrounded;
            _eventBus.Post(new OnTracksUpdatedEvent());
            
            // foreach (var track in TrackSection.TrackPieces)
            // {
            //     if (track != null)
            //     {
            //         Plugin.Log.LogInfo(track.transform.position.ToString());
            //     }
            //     else
            //     {
            //         Plugin.Log.LogWarning("");
            //     }
            // }
            //
            //
            // foreach (var trackRoute in TrackRoutes)
            // {
            //     if (trackRoute.Exit.ConnectedTrackRoutes == null)
            //         continue;
            //
            //     Plugin.Log.LogWarning($"{trackRoute.Exit.Coordinates} {trackRoute.Exit.Direction}: {trackRoute.Exit.ConnectedTrackRoutes.Length}");
            //
            //     foreach (var trackRoute1 in trackRoute.Exit.ConnectedTrackRoutes)
            //     {
            //         if (trackRoute1.Exit.ConnectedTrackPiece != null)
            //         {
            //             Plugin.Log.LogInfo(trackRoute1.Exit.ConnectedTrackPiece.GetComponent<BlockObjectCenter>().WorldCenterGrounded + "");
            //         }
            //         else
            //         {
            //             Plugin.Log.LogInfo("");
            //         }
            //     }
            // }
        }

        public void OnExitFinishedState()
        {
            TrackSection.Dissolve(this);
            _eventBus.Post(new OnTracksUpdatedEvent());
            _trackRouteWeightCache.Remove(TrackRoutes);
        }

        public void ResetSection()
        {
            TrackSection = new TrackSection(this);
        }
        
        public void LookForTrackSection()
        {
            if (this == null)
                return;
            
            foreach (var directionalTrackRoute in TrackRoutes.GroupBy(route => route.Exit.Direction).Select(group => group.First()))
            {
                
                // Plugin.Log.LogWarning(directionalTrackRoute.Exit.Direction.ToString());
                // _trackConnectionService.CanConnectInDirection(trackConnection.Coordinates, trackConnection.Direction);
                // Plugin.Log.LogInfo("Offset " + trackConnection.Coordinates);
                // Plugin.Log.LogInfo("Direciton offset " + trackConnection.Direction.ToOffset());
                // Plugin.Log.LogInfo("Together " + (trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                // Plugin.Log.LogInfo("Final" + _blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));

                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(directionalTrackRoute.Exit.Coordinates - directionalTrackRoute.Exit.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponent(out TrackPiece trackPiece)) 
                    continue;
                // Plugin.Log.LogWarning("Place to check: " + _blockObject.Transform(directionalTrackRoute.Exit.Coordinates - directionalTrackRoute.Exit.Direction.ToOffset()));
                var myTrackRouteEntrances = CheckAndGetConnection(trackPiece).ToArray();
                var myTrackRouteExits = CheckAndGetExits(trackPiece).ToArray();
                
                // var connection = GetConnection(trackPiece, trackConnection.Direction);
                var otherTrackRoutesEntrances = trackPiece.CheckAndGetConnection(this).ToArray();
                var otherTrackRoutesExits = trackPiece.CheckAndGetExits(this).ToArray();
                // Plugin.Log.LogInfo(obj.Coordinates.ToString());
                // Plugin.Log.LogError(
                //     "My Entrances: " + myTrackRouteEntrances.Length + 
                //     " My Exits: " + myTrackRouteExits.Length + 
                //     " Other Entrances: " + otherTrackRoutesEntrances.Length + 
                //     " Other Exits: " + otherTrackRoutesExits.Length);
                // if (myTrackRouteEntrances.Length < 1 || myTrackRouteExits.Length < 1 || otherTrackRoutesEntrances.Length < 1 || otherTrackRoutesExits.Length < 1)
                if (myTrackRouteExits.Length < 1 || otherTrackRoutesExits.Length < 1)
                    continue;
                MakeConnection(trackPiece, myTrackRouteEntrances, myTrackRouteExits, otherTrackRoutesEntrances, otherTrackRoutesExits);
            }
        }
        
        private IEnumerable<TrackRoute> CheckAndGetConnection(TrackPiece previousTrackPiece)
        {
            foreach (var trackRoute in TrackRoutes)
            {
                // Plugin.Log.LogError(_blockObject.Transform(trackRoute.Entrance.Coordinates - trackRoute.Entrance.Direction.ToOffset()) + " Entrance");
                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackRoute.Entrance.Coordinates - trackRoute.Entrance.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponent(out TrackPiece trackPiece))
                    continue;
                // Plugin.Log.LogWarning((trackPiece == previousTrackPiece) + "");
                if (trackPiece == previousTrackPiece)
                {
                    yield return trackRoute;
                }
            }
        }
        
        private IEnumerable<TrackRoute> CheckAndGetExits(TrackPiece previousTrackPiece)
        {
            foreach (var trackRoute in TrackRoutes)
            {
                // Plugin.Log.LogError(_blockObject.Transform(trackRoute.Exit.Coordinates - trackRoute.Exit.Direction.ToOffset()) + " Exit");
                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackRoute.Exit.Coordinates - trackRoute.Exit.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponent(out TrackPiece trackPiece))
                    continue;
                // Plugin.Log.LogWarning((trackPiece == previousTrackPiece) + "");
                if (trackPiece == previousTrackPiece)
                {
                    yield return trackRoute;
                }
            }
        }

        private void MakeConnection(TrackPiece trackPiece, TrackRoute[] myTrackRouteEntrances, TrackRoute[] myTrackRouteExits, TrackRoute[] otherTrackRoutesEntrances, TrackRoute[] otherTrackRoutesExits)
        {
            foreach (var trackRoute in myTrackRouteExits)
            {
                trackRoute.Exit.ConnectedTrackPiece = trackPiece;
                trackRoute.Exit.ConnectedTrackRoutes = otherTrackRoutesEntrances;
            }
            
            foreach (var trackRoute in otherTrackRoutesExits)
            {
                trackRoute.Exit.ConnectedTrackPiece = this;
                trackRoute.Exit.ConnectedTrackRoutes = myTrackRouteEntrances;
            }

            var flag1 = TryGetComponent(out TrackIntersection _);
            var flag2 = trackPiece != null && trackPiece.TryGetComponent(out TrackIntersection _);

            if (flag1 || flag2)
            {
                if (flag1)
                    trackPiece.TrackSection.Add(this);
                if (flag2)
                    TrackSection.Add(trackPiece);
                return;
            }

            if (trackPiece.TrackSection != TrackSection)
                trackPiece.TrackSection.Merge(TrackSection);
        }
    }
}