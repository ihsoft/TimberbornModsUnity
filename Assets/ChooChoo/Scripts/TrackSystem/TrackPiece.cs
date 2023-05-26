using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrackPiece : BaseComponent, IFinishedStateListener
    {
        [SerializeField]
        private int _trackDistance;
        
        protected EventBus EventBus;
        
        private BlockService _blockService;

        private TrackArrayProvider _trackArrayProvider;

        private TrackRouteWeightCache _trackRouteWeightCache;

        private BlockObject _blockObject;

        private BlockObjectCenter _blockObjectCenter;

        private TrackRoute[] _trackConnections;

        public TrackSection TrackSection;

        public Vector3 CenterCoordinates { get; private set; }
        
        public bool CanPathFindOverIt { get; set; }

        public bool DividesSection;

        public int TrackDistance => _trackDistance;

        [Inject]
        public void InjectDependencies(
            BlockService blockService, 
            EventBus eventBus, 
            TrackArrayProvider trackArrayProvider, 
            TrackRouteWeightCache trackRouteWeightCache)
        {
            _blockService = blockService;
            EventBus = eventBus;
            _trackArrayProvider = trackArrayProvider;
            _trackRouteWeightCache = trackRouteWeightCache;
        }
        
        public virtual TrackRoute[] TrackRoutes
        {
            get
            {
                if (_trackConnections == null || !Application.isPlaying)
                {
                    var trackRoutes = _trackArrayProvider.GetConnections(GameObjectFast.name);
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
            // protected set => _trackConnections = value;
        }

        public void Awake()
        {
            TrackSection = new TrackSection(this);
            _blockObject = GetComponentFast<BlockObject>();
            _blockObjectCenter = GetComponentFast<BlockObjectCenter>();
            CanPathFindOverIt = !TryGetComponentFast(out TrainWaitingLocation _);
        }

        public void OnEnterFinishedState()
        {
            LookForTrackSection();
            CenterCoordinates = GetComponentFast<BlockObjectCenter>().WorldCenterGrounded;
            EventBus.Post(new OnTracksUpdatedEvent());
            
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
            EventBus.Post(new OnTracksUpdatedEvent());
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
            
            foreach (var directionalTrackRoute in TrackRoutes)
            {
                // Plugin.Log.LogWarning(directionalTrackRoute.Exit.Direction.ToString());
                // _trackConnectionService.CanConnectInDirection(trackConnection.Coordinates, trackConnection.Direction);
                // Plugin.Log.LogInfo("Offset " + trackConnection.Coordinates);
                // Plugin.Log.LogInfo("Direciton offset " + trackConnection.Direction.ToOffset());
                // Plugin.Log.LogInfo("Together " + (trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                // Plugin.Log.LogInfo("Final" + _blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));

                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(directionalTrackRoute.Exit.Coordinates - directionalTrackRoute.Exit.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponentFast(out TrackPiece trackPiece)) 
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
                // if ((myTrackRouteEntrances.Length < 1 && otherTrackRoutesExits.Length) < 1 || (otherTrackRoutesEntrances.Length < 1 && myTrackRouteExits.Length < 1))
                //     continue;
                if (myTrackRouteExits.Length < 1 || (otherTrackRoutesExits.Length < 1 && otherTrackRoutesEntrances.Length < 1))
                    continue;
                MakeConnection(trackPiece, myTrackRouteEntrances, myTrackRouteExits, otherTrackRoutesEntrances, otherTrackRoutesExits);
            }

            foreach (var directionalTrackRoute in TrackRoutes)
            {
                // Plugin.Log.LogWarning(directionalTrackRoute.Exit.Direction.ToString());
                // _trackConnectionService.CanConnectInDirection(trackConnection.Coordinates, trackConnection.Direction);
                // Plugin.Log.LogInfo("Offset " + trackConnection.Coordinates);
                // Plugin.Log.LogInfo("Direciton offset " + trackConnection.Direction.ToOffset());
                // Plugin.Log.LogInfo("Together " + (trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                // Plugin.Log.LogInfo("Final" + _blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
            
                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(directionalTrackRoute.Entrance.Coordinates - directionalTrackRoute.Entrance.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponentFast(out TrackPiece trackPiece)) 
                    continue;
                // Plugin.Log.LogWarning("Place to check: " + _blockObject.Transform(directionalTrackRoute.Exit.Coordinates - directionalTrackRoute.Exit.Direction.ToOffset()));
                var myTrackRouteEntrances = CheckAndGetConnection(trackPiece).ToArray();
                var myTrackRouteExits = CheckAndGetExits(trackPiece).ToArray();
                
                // var connection = GetConnection(trackPiece, trackConnection.Direction);
                var otherTrackRoutesEntrances = trackPiece.CheckAndGetConnection(this).ToArray();
                var otherTrackRoutesExits = trackPiece.CheckAndGetExits(this).ToArray();
                // Plugin.Log.LogInfo(obj.Coordinates.ToString());
                // Plugin.Log.LogWarning(
                //     "My Entrances: " + myTrackRouteEntrances.Length + 
                //     " My Exits: " + myTrackRouteExits.Length + 
                //     " Other Entrances: " + otherTrackRoutesEntrances.Length + 
                //     " Other Exits: " + otherTrackRoutesExits.Length);
                // if (myTrackRouteEntrances.Length < 1 || myTrackRouteExits.Length < 1 || otherTrackRoutesEntrances.Length < 1 || otherTrackRoutesExits.Length < 1)
                if (myTrackRouteEntrances.Length < 1 || (otherTrackRoutesEntrances.Length < 1 && otherTrackRoutesExits.Length < 1))
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
                if (obj == null || !obj.TryGetComponentFast(out TrackPiece trackPiece))
                    continue;
                if (!obj.TryGetComponentFast(out BlockObject blockObject) || !blockObject.Finished)
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
                if (obj == null || !obj.TryGetComponentFast(out TrackPiece trackPiece))
                    continue;
                if (!obj.TryGetComponentFast(out BlockObject blockObject) || !blockObject.Finished)
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
            // Plugin.Log.LogWarning("Connecting");

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
            
            foreach (var trackRoute in myTrackRouteEntrances)
            {
                trackRoute.Entrance.ConnectedTrackPiece = trackPiece;
                // trackRoute.Entrance.ConnectedTrackRoutes = otherTrackRoutesExits;
            }
        
            foreach (var trackRoute in otherTrackRoutesEntrances)
            {
                trackRoute.Entrance.ConnectedTrackPiece = this;
                // trackRoute.Entrance.ConnectedTrackRoutes = myTrackRouteExits;
            }

            var flag4 = TryGetComponentFast(out TrainWaitingLocation _);
            var flag3 = trackPiece.TryGetComponentFast(out TrainWaitingLocation _);
            
            if (flag3 || flag4)
            {
                return;
            }
            
            var flag1 = DividesSection;
            var flag2 = trackPiece.DividesSection;

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