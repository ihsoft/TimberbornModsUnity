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

        private TrackConnectionService _trackConnectionService;

        private BlockObject _blockObject;

        private BlockObjectCenter _blockObjectCenter;

        private TrackRoute[] _trackConnections;

        public TrackSection TrackSection;

        public Vector3 CenterCoordinates { get; private set; }
        
        public bool CanPathFindOverIt { get; set; }

        public bool IsIntersection { get; set; }

        [Inject]
        public void InjectDependencies(BlockService blockService, EventBus eventBus, TrackArrayProvider trackArrayProvider, TrackConnectionService trackConnectionService)
        {
            _blockService = blockService;
            _eventBus = eventBus;
            _trackArrayProvider = trackArrayProvider;
            _trackConnectionService = trackConnectionService;
        }
        
        public TrackRoute[] TrackRoutes
        {
            get
            {
                if (_trackConnections == null || !Application.isPlaying)
                {
                    var list = _trackArrayProvider.GetConnections(gameObject.name);
                    foreach (var trackConnection in list)
                    {
                        var position = _blockObjectCenter.WorldCenterGrounded;
                        trackConnection.RouteCorners = trackConnection.RouteCorners.Select(vector3 => _blockObject.Orientation.TransformInWorldSpace(vector3) + position).ToArray();
                    }
                    _trackConnections = list;
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
            //             Plugin.Log.LogInfo(trackRoute1.Exit.ConnectedTrackPiece.CenterCoordinates + "");
            //         }
            //         else
            //         {
            //             Plugin.Log.LogInfo("");
            //         }
            //     }
            //     
            // }
        }

        public void OnExitFinishedState()
        {
            TrackSection.Dissolve();
            _eventBus.Post(new OnTracksUpdatedEvent());
        }

        public void ResetSection()
        {
            TrackSection = new TrackSection(this);
        }
        
        public void LookForTrackSection()
        {
            foreach (var trackConnection in TrackRoutes.GroupBy(route => route.Exit.Direction).Select(group => group.First()))
            {
                
                // Plugin.Log.LogWarning(trackConnection.Exit.Direction.ToString());
                // _trackConnectionService.CanConnectInDirection(trackConnection.Coordinates, trackConnection.Direction);
                // Plugin.Log.LogInfo("Offset " + trackConnection.Coordinates);
                // Plugin.Log.LogInfo("Direciton offset " + trackConnection.Direction.ToOffset());
                // Plugin.Log.LogInfo("Together " + (trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                // Plugin.Log.LogInfo("Final" + _blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));

                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackConnection.Exit.Coordinates - trackConnection.Exit.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponent(out TrackPiece trackPiece)) 
                    continue;
                // Plugin.Log.LogWarning("PLace to check: " + _blockObject.Transform(trackConnection.Exit.Coordinates - trackConnection.Exit.Direction.ToOffset()));
                var myTrackRouteEntrances = CheckAndGetConnection(trackPiece).ToArray();
                var myTrackRouteExits = CheckAndGetExits(trackPiece).ToArray();
                
                // var connection = GetConnection(trackPiece, trackConnection.Direction);
                var otherTrackRoutesEntrances = trackPiece.CheckAndGetConnection(this).ToArray();
                var otherTrackRoutesExits = trackPiece.CheckAndGetExits(this).ToArray();
                // Plugin.Log.LogInfo(obj.Coordinates.ToString());
                // Plugin.Log.LogError("My Entrances length: " + myTrackRouteExits.Length + " Other Entrances length: " + otherTrackRoutesExits.Length);
                if (myTrackRouteExits.Length < 1 || otherTrackRoutesEntrances.Length < 1)
                    return;
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

            // foreach (var trackRoute in otherTrackRoutesEntrances)
            // {
            //     trackRoute.Entrance.ConnectedTrackPiece = this;
            //     trackRoute.Entrance.ConnectedTrackConnection = myTrackRouteEntrances;
            // }


            // if (TryGetComponent(out TrainDestination _) || trackPiece.TryGetComponent(out TrainDestination _))
            //     return;
            

            if (TryGetComponent(out TrackIntersection _))
            {
                // trackPiece.TrackSection.Add(this);
                return;
            }

            if (trackPiece.TryGetComponent(out TrackIntersection _))
            {
                TrackSection.Add(trackPiece);
                return;
            }


            if (trackPiece.TrackSection != TrackSection)
                trackPiece.TrackSection.Merge(TrackSection);
        }
    }
}