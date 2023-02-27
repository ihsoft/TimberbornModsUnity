using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using HarmonyLib;
using TimberApi.EntityLinkerSystem;
using Timberborn.BlockSystem;
using Timberborn.BlockSystemNavigation;
using Timberborn.Common;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.PrefabSystem;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace ChooChoo
{
    public class PathLinkPoint : MonoBehaviour, IFinishedStateListener, IRegisteredComponent
    {
        [SerializeField] private bool uiEnabled;

        private BlockObjectNavMeshSettings _blockObjectNavMeshSettings;

        private BlockObjectNavMesh _blockObjectNavMesh;

        private BlockObjectCenter _blockObjectCenter;

        private BlockObject _blockObject;

        private Prefab _prefab;
        
        private PathLinkPointService _pathLinkPointService;

        private ChooChooCore _chooChooCore;

        private EventBus _eventBus;

        private BlockObjectNavMeshEdgeSpecification[] _cachedSpecifications;

        private readonly HashSet<PathLinkPoint> _linkedPathLinkPoints = new();

        public bool UIEnabledEnabled => uiEnabled;

        public string PrefabName => _prefab.PrefabName;

        public Vector3 Location => _blockObjectCenter.WorldCenterGrounded;

        [Inject]
        public void InjectDependencies(PathLinkPointService pathLinkPointService, ChooChooCore chooChooCore, EventBus eventBus)
        {
            _pathLinkPointService = pathLinkPointService;
            _chooChooCore = chooChooCore;
            _eventBus = eventBus;
        }

        private void Awake()
        {
            _blockObjectNavMeshSettings = GetComponent<BlockObjectNavMeshSettings>();
            _blockObjectNavMesh = GetComponent<BlockObjectNavMesh>();
            _blockObjectCenter = GetComponent<BlockObjectCenter>();
            _blockObject = GetComponent<BlockObject>();
            _prefab = GetComponent<Prefab>();
            _cachedSpecifications = (BlockObjectNavMeshEdgeSpecification[])_chooChooCore.GetInaccessibleField(_blockObjectNavMeshSettings, "_addedEdges");
        }

        public void OnEnterFinishedState()
        {
            _eventBus.Register(this);
        }

        public void OnExitFinishedState()
        {
            _eventBus.Unregister(this);
        }

        public void Connect(PathLinkPoint connector)
        {
            Plugin.Log.LogInfo("Connecting: " + Location + " with: " + connector.Location);
            _linkedPathLinkPoints.Add(connector);
            _eventBus.Post(new OnPathLinkCreated());
        }

        [OnEvent]
        public void OnPathLinkCreated(OnPathLinkCreated onPathLinkCreated)
        {
            var specifications = _cachedSpecifications.ToList();
            
            Plugin.Log.LogInfo("OnPathLinkCreated");
            
            foreach (var pathLinkPoint in _linkedPathLinkPoints)
            {
                var relativeCoordinates = (pathLinkPoint.Location - Location).FloorToInt();
                relativeCoordinates = new Vector3Int(relativeCoordinates.x, relativeCoordinates.z, relativeCoordinates.y);
                relativeCoordinates = _blockObject.Orientation.Untransform(relativeCoordinates);
                Plugin.Log.LogError(relativeCoordinates + "");
                specifications.Add(CreateNewBlockObjectNavMeshEdgeSpecification(new Vector3Int(0,0,0), relativeCoordinates, true));
            }

            _chooChooCore.SetInaccessibleField(_blockObjectNavMeshSettings, "_addedEdges", specifications.ToArray());
            _blockObjectNavMesh.RecalculateNavMeshObject();
        }

        private BlockObjectNavMeshEdgeSpecification CreateNewBlockObjectNavMeshEdgeSpecification(Vector3Int start, Vector3Int end, bool isTwoWay)
        {
            var newBlockObjectNavMeshEdgeSpecification = new BlockObjectNavMeshEdgeSpecification();
            
            _chooChooCore.SetInaccessibleField(newBlockObjectNavMeshEdgeSpecification, "_start", start);
            _chooChooCore.SetInaccessibleField(newBlockObjectNavMeshEdgeSpecification, "_end", end);
            _chooChooCore.SetInaccessibleField(newBlockObjectNavMeshEdgeSpecification, "_isTwoWay", isTwoWay);

            return newBlockObjectNavMeshEdgeSpecification;
        }
    }
}