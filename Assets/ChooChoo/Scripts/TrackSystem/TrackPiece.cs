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

        private TrackConnectionsArrayProvider _trackConnectionsArrayProvider;

        private TrackConnectionService _trackConnectionService;

        private BlockObject _blockObject;

        private TrackConnection[] _trackConnections;

        public TrackSection TrackSection;

        [Inject]
        public void InjectDependencies(BlockService blockService, TrackConnectionsArrayProvider trackConnectionsArrayProvider, TrackConnectionService trackConnectionService)
        {
            _blockService = blockService;
            _trackConnectionsArrayProvider = trackConnectionsArrayProvider;
            _trackConnectionService = trackConnectionService;
        }
        
        public TrackConnection[] TrackConnections
        {
            get
            {
                if (_trackConnections == null || !Application.isPlaying)
                    _trackConnections = _trackConnectionsArrayProvider.GetConnections(gameObject.name);
                return _trackConnections;
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
            foreach (var trackPiece in TrackSection.TrackPieces)
            {
                Plugin.Log.LogWarning(trackPiece.transform.position.ToString());
            }
        }

        public void OnExitFinishedState()
        {
            TrackSection.Remove(this);
        }

        private void LookForTrackSection()
        {
            foreach (var trackConnection in TrackConnections)
            {
                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                if (obj != null && obj.TryGetComponent(out TrackPiece trackPiece))
                {
                    if (CanConnect(trackPiece, trackConnection.Direction))
                    {
                        if (trackPiece.TrackSection != TrackSection)
                        {
                            trackPiece.TrackSection.Merge(TrackSection);
                        } 
                    }
                }
            }
        }

        private bool CanConnect(TrackPiece trackPiece, Direction2D direction)
        {
            return trackPiece.TrackConnections.Any(connection => connection.Direction == Direction2DExtensions.ToOppositeDirection(direction));
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