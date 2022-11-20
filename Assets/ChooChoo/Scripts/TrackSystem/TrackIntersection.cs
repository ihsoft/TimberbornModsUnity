using System;
using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    public class TrackIntersection : MonoBehaviour, IFinishedStateListener
    {
        private BlockService _blockService;

        private TrackConnectionsArrayProvider _trackConnectionsArrayProvider;

        private TrackConnectionService _trackConnectionService;

        private TrackConnection[] _trackConnections;
        
        private BlockObject _blockObject;

        [Inject]
        public void InjectDependencies(BlockService blockService,
            TrackConnectionsArrayProvider trackConnectionsArrayProvider, TrackConnectionService trackConnectionService)
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

        private void Awake()
        {
            _blockObject = GetComponent<BlockObject>();
        }

        public void OnEnterFinishedState()
        {
            LookForConnections();
        }

        public void OnExitFinishedState()
        {

        }


        private void LookForConnections()
        {
            foreach (var trackConnection in TrackConnections)
            {
                var obj = _blockService.GetFloorObjectAt(_blockObject.Transform(trackConnection.Coordinates - trackConnection.Direction.ToOffset()));
                if (obj != null && obj.TryGetComponent(out TrackPiece trackPiece))
                {
                    trackConnection.TrackSection = trackPiece.TrackSection;
                }
            }
        }
    }
}