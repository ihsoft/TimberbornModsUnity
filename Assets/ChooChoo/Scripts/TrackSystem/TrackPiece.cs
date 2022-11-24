using System;
using System.Linq;
using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    public class TrackPiece : MonoBehaviour, IFinishedStateListener
    {
        private BlockService _blockService;

        private TrackArrayProvider _trackArrayProvider;

        private TrackConnectionService _trackConnectionService;

        private BlockObject _blockObject;

        private TrackConnection[] _trackConnections;
        
        private Vector3[] _pathCorners;

        public TrackSection TrackSection;

        [Inject]
        public void InjectDependencies(BlockService blockService, TrackArrayProvider trackArrayProvider, TrackConnectionService trackConnectionService)
        {
            _blockService = blockService;
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
                        // Plugin.Log.LogInfo("Before: " + trackConnection.Direction);
                        // trackConnection.Direction = _blockObject.Transform(trackConnection.Direction);
                        trackConnection.Direction = trackConnection.Direction;
                        
                        // Plugin.Log.LogInfo("After: " + trackConnection.Direction);
                    }
                    _trackConnections = list;

                    // foreach (var track in _trackConnections)
                    // {
                    //     Plugin.Log.LogInfo(track.Direction.ToString());
                    // }
                }
                return _trackConnections;
            }
        }
        
        public Vector3[] PathCorners
        {
            get
            {
                if (_pathCorners == null || !Application.isPlaying)
                {
                    var list = _trackArrayProvider.GetPathCorners(gameObject.name);
                    var position = _blockObject.Coordinates;
                    var coord = new Vector3(position.x, position.z, position.y);
                    _pathCorners = list.Select(vector3 => vector3 + coord).ToArray();
                }
                return _pathCorners;
            }
        }

        void Awake()
        {
            TrackSection = new TrackSection(this);
            _blockObject = GetComponent<BlockObject>();
        }

        public void OnEnterFinishedState()
        {
            LookForTrackSection();
            foreach (var trackConnection in _trackConnections)
            {
                if (trackConnection.ConnectedTrackPiece != null)
                {
                    Plugin.Log.LogWarning(trackConnection.ConnectedTrackPiece.transform.position.ToString());
                }
                else
                {
                    Plugin.Log.LogWarning("");
                }
            }
        }

        public void OnExitFinishedState()
        {
            TrackSection.Remove(this);
        }

        public TrackConnection CheckAndGetConnection(TrackConnection connection, TrackPiece previousTrackPiece)
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

        private void LookForTrackSection()
        {
            foreach (var trackConnection in TrackConnections)
            {
                // _trackConnectionService.CanConnectInDirection(trackConnection.Coordinates, trackConnection.Direction);
                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                if (obj == null || !obj.TryGetComponent(out TrackPiece trackPiece)) 
                    continue;
                
                // var connection = GetConnection(trackPiece, trackConnection.Direction);
                var connection = trackPiece.CheckAndGetConnection(trackConnection, this);
                if (connection != null)
                    MakeConnection(trackConnection, trackPiece, connection);
            }
        }

        private TrackConnection GetConnection(TrackPiece trackPiece, Direction2D direction)
        {
            foreach (var trackConnection in TrackConnections)
            {
                // var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                // if (obj == null || !obj.TryGetComponent(out TrackPiece trackPiece)) 
                //     continue;
            }
                
            
            return trackPiece.TrackConnections.FirstOrDefault(connection => connection.Direction == Direction2DExtensions.ToOppositeDirection(direction));
        }

        private void MakeConnection(TrackConnection thisTrackConnection, TrackPiece trackPiece, TrackConnection trackConnection)
        {
            thisTrackConnection.ConnectedTrackPiece = trackPiece;
            trackConnection.ConnectedTrackPiece = this;
            // if (trackPiece.TrackSection != TrackSection)
            // {
            //     trackPiece.TrackSection.Merge(TrackSection);
            // } 
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