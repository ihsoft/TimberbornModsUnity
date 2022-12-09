using System;
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

        private TrackConnection[] _trackConnections;

        public TrackSection TrackSection;

        public Vector3 CenterCoordinates { get; private set; }

        [Inject]
        public void InjectDependencies(BlockService blockService, EventBus eventBus, TrackArrayProvider trackArrayProvider, TrackConnectionService trackConnectionService)
        {
            _blockService = blockService;
            _eventBus = eventBus;
            _trackArrayProvider = trackArrayProvider;
            _trackConnectionService = trackConnectionService;
        }
        
        public TrackConnection[] TrackConnections
        {
            get
            {
                if (_trackConnections == null || !Application.isPlaying)
                {
                    var list = _trackArrayProvider.GetConnections(gameObject.name);
                    foreach (var trackConnection in list)
                    {
                        var position = _blockObjectCenter.WorldCenterGrounded;
                        trackConnection.PathCorners = trackConnection.PathCorners.Select(vector3 =>
                        {
                            return _blockObject.Orientation.TransformInWorldSpace(vector3) + position;
                            // switch (_blockObject.Orientation)
                            // {
                            //     case Orientation.Cw0:
                            //         return vector3 + position;
                            //     case Orientation.Cw90:
                            //         return new Vector3(vector3.z, vector3.y, -vector3.x) + position;
                            //     case Orientation.Cw180:
                            //         return new Vector3(-vector3.x, vector3.y, -vector3.z) + position;
                            //     case Orientation.Cw270:
                            //         return new Vector3(-vector3.z, vector3.y, vector3.x) + position;
                            //     default:
                            //         throw new ArgumentException($"Unexpected Orientation: {_blockObject.Orientation}");
                            // }
                        }).ToArray();
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
        }

        public void OnEnterFinishedState()
        {
            LookForTrackSection();
            CenterCoordinates = GetComponent<BlockObjectCenter>().WorldCenterGrounded;
            _eventBus.Post(new OnTracksUpdatedEvent());
            foreach (var track in TrackSection.TrackPieces)
            {
                if (track != null)
                {
                    Plugin.Log.LogInfo(track.transform.position.ToString());
                }
                else
                {
                    Plugin.Log.LogWarning("");
                }
            }
            
            
            // foreach (var trackConnection in _trackConnections)
            // {
            //     if (trackConnection.ConnectedTrackPiece != null)
            //     {
            //         Plugin.Log.LogWarning(trackConnection.ConnectedTrackPiece.transform.position.ToString());
            //     }
            //     else
            //     {
            //         Plugin.Log.LogWarning("");
            //     }
            // }
        }

        public void OnExitFinishedState()
        {
            TrackSection.Dissolve();
        }

        public void ResetSection()
        {
            TrackSection = new TrackSection(this);
        }
        
        public void LookForTrackSection()
        {
            foreach (var trackConnection in TrackConnections)
            {
                // _trackConnectionService.CanConnectInDirection(trackConnection.Coordinates, trackConnection.Direction);
                // Plugin.Log.LogInfo("Offset " + trackConnection.Coordinates);
                // Plugin.Log.LogInfo("Direciton offset " + trackConnection.Direction.ToOffset());
                // Plugin.Log.LogInfo("Together " + (trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                // Plugin.Log.LogInfo("Final" + _blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponent(out TrackPiece trackPiece)) 
                    continue;
                
                // var connection = GetConnection(trackPiece, trackConnection.Direction);
                var connection = trackPiece.CheckAndGetConnection(this);
                if (connection != null)
                    MakeConnection(trackConnection, trackPiece, connection);
            }
        }
        
        private TrackConnection CheckAndGetConnection(TrackPiece previousTrackPiece)
        {
            foreach (var trackConnection in TrackConnections)
            {
                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponent(out TrackPiece trackPiece))
                    continue;
                
                if (trackPiece == previousTrackPiece)
                {
                    return trackConnection;
                }
            }

            return null;
        }

        private void MakeConnection(TrackConnection thisTrackConnection, TrackPiece trackPiece, TrackConnection trackConnection)
        {
            thisTrackConnection.ConnectedTrackPiece = trackPiece;
            thisTrackConnection.ConnectedTrackConnection = trackConnection;
            trackConnection.ConnectedTrackPiece = this;
            trackConnection.ConnectedTrackConnection = thisTrackConnection;
            
            if (TryGetComponent(out TrainStation _))
                return;
            

            if (TryGetComponent(out TrackIntersection _))
            {
                trackPiece.TrackSection.Add(this);
                return;
            }


            if (trackPiece.TrackSection != TrackSection)
                trackPiece.TrackSection.Merge(TrackSection);
        }




        // public void TryAddNextStepOfRoute(List<Vector3Int> route, Direction2D previousDirection)
        // {
        //     route.Add(_blockObject.Coordinates);
        //     var oppositeDirection = Direction2DExtensions.ToOppositeDirection(previousDirection);
        //
        //     foreach (Direction2D direction in Enum.GetValues(typeof(Direction2D)))
        //     {
        //         if (direction == oppositeDirection)
        //             continue;
        //
        //         if (_trackConnectionService.CanConnectInDirection(_blockObject.Coordinates, direction))
        //         {
        //             var obj = _blockService.GetFloorObjectAt(_blockObject.Coordinates + direction.ToOffset());
        //             if (obj != null && obj.TryGetComponent(out TrackPiece rail))
        //                 rail.TryAddNextStepOfRoute(route, direction);
        //             if (obj != null && obj.TryGetComponent(out TrainStation trainStation))
        //                 trainStation.MarkEndOfRoute(route);
        //         }
        //     }
        // }
        
    }
}