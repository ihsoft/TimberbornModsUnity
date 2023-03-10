using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.BlockSystemNavigation;
using Timberborn.Common;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.Persistence;
using Timberborn.PrefabSystem;
using Timberborn.SingletonSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace PathLinkUtilities
{
  public class PathLinkPoint : 
    MonoBehaviour,
    IFinishedStateListener,
    IPersistentEntity,
    IRegisteredComponent
  {
    private readonly ComponentKey PathLinkPointKey = new(nameof (PathLinkPoint));
    private readonly ListKey<PathLink> PathLinksKey = new("PathLinks");
    [SerializeField]
    private float movementSpeedMultiplier = 1f;
    [SerializeField]
    private bool uiEnabled;
    [SerializeField]
    private bool connectsTwoWay;
    private BlockObjectNavMeshSettings _blockObjectNavMeshSettings;
    private BlockObjectNavMesh _blockObjectNavMesh;
    private BlockObjectCenter _blockObjectCenter;
    private BlockObject _blockObject;
    private Prefab _prefab;
    private PathLinkRepository _pathLinkRepository;
    private PathLinkObjectSerializer _pathLinkObjectSerializer;
    private IDayNightCycle _dayNightCycle;
    private PathLinkUtilitiesCore _pathLinkUtilitiesCore;
    private EventBus _eventBus;
    private BlockObjectNavMeshEdgeSpecification[] _cachedSpecifications;

    public bool UIEnabledEnabled => uiEnabled;

    public string PrefabName => _prefab.PrefabName;

    public Vector3 Location => _blockObjectCenter.WorldCenterGrounded;

    [Inject]
    public void InjectDependencies(
      PathLinkRepository pathLinkRepository,
      PathLinkObjectSerializer pathLinkObjectSerializer,
      IDayNightCycle dayNightCycle,
      PathLinkUtilitiesCore pathLinkUtilitiesCore,
      EventBus eventBus)
    {
      _pathLinkRepository = pathLinkRepository;
      _pathLinkObjectSerializer = pathLinkObjectSerializer;
      _dayNightCycle = dayNightCycle;
      _pathLinkUtilitiesCore = pathLinkUtilitiesCore;
      _eventBus = eventBus;
    }

    private void Awake()
    {
      _blockObjectNavMeshSettings = GetComponent<BlockObjectNavMeshSettings>();
      _blockObjectNavMesh = GetComponent<BlockObjectNavMesh>();
      _blockObjectCenter = GetComponent<BlockObjectCenter>();
      _blockObject = GetComponent<BlockObject>();
      _prefab = GetComponent<Prefab>();
      _cachedSpecifications = (BlockObjectNavMeshEdgeSpecification[]) _pathLinkUtilitiesCore.GetInaccessibleField(_blockObjectNavMeshSettings, "_addedEdges");
      enabled = false;
    }

    public void OnEnterFinishedState()
    {
      enabled = true;
      _eventBus.Register(this);
    }

    public void OnExitFinishedState()
    {
      enabled = false;
      _eventBus.Unregister(this);
      _pathLinkRepository.ValidateLinks();
      _eventBus.Post(new OnPathLinksUpdated());
    }

    public void Save(IEntitySaver entitySaver) => entitySaver.GetComponent(PathLinkPointKey).Set(PathLinksKey, _pathLinkRepository.PathLinks(this).ToList(), _pathLinkObjectSerializer);

    public void Load(IEntityLoader entityLoader)
    {
      if (!entityLoader.HasComponent(PathLinkPointKey) || !entityLoader.GetComponent(PathLinkPointKey).Has(PathLinksKey))
        return;
      foreach (PathLink pathLink in entityLoader.GetComponent(PathLinkPointKey).Get(PathLinksKey, _pathLinkObjectSerializer))
        _pathLinkRepository.AddNew(pathLink);
      UpdateNavMesh();
    }

    public void Connect(PathLinkPoint endPoint)
    {
      float waitingTimeInHours = CalculateWaitingTimeInHours(endPoint);
      Plugin.Log.LogError(waitingTimeInHours + "");
      _pathLinkRepository.AddNew(new PathLink(this, endPoint, waitingTimeInHours));
      if (connectsTwoWay)
        _pathLinkRepository.AddNew(new PathLink(endPoint, this, waitingTimeInHours));
      _eventBus.Post(new OnPathLinksUpdated());
    }

    [OnEvent]
    public void OnPathLinksUpdated(OnPathLinksUpdated onPathLinksUpdated) => UpdateNavMesh();

    public bool AlreadyConnected(PathLinkPoint b)
    {
      if (!connectsTwoWay)
        return _pathLinkRepository.GetPathLink(this, b) != null;
      return _pathLinkRepository.GetPathLink(this, b) != null || _pathLinkRepository.GetPathLink(b, this) != null;
    }

    private float CalculateWaitingTimeInHours(PathLinkPoint endPoint) => _dayNightCycle.SecondsToHours(Vector3.Distance(Location, endPoint.Location) / (2.7f * movementSpeedMultiplier));

    private void UpdateNavMesh()
    {
      List<BlockObjectNavMeshEdgeSpecification> list = _cachedSpecifications.ToList();
      foreach (PathLink pathLink in _pathLinkRepository.PathLinks(this))
      {
        Vector3Int vector = (pathLink.EndLinkPoint.Location - Location).FloorToInt();
        vector = new Vector3Int(vector.x, vector.z, vector.y);
        Vector3Int end = _blockObject.Orientation.Untransform(vector);
        Plugin.Log.LogWarning(pathLink.EndLinkPoint.Location + "   " + Location);
        Plugin.Log.LogWarning(end.ToString() ?? "");
        list.Add(CreateNewBlockObjectNavMeshEdgeSpecification(new Vector3Int(0, 0, 0), end, connectsTwoWay));
      }
      _pathLinkUtilitiesCore.SetInaccessibleField(_blockObjectNavMeshSettings, "_addedEdges", list.ToArray());
      _blockObjectNavMesh.RecalculateNavMeshObject();
      _blockObjectNavMesh.NavMeshObject.AddToNavMesh();
    }

    private BlockObjectNavMeshEdgeSpecification CreateNewBlockObjectNavMeshEdgeSpecification(
      Vector3Int start,
      Vector3Int end,
      bool isTwoWay)
    {
      BlockObjectNavMeshEdgeSpecification instance = new BlockObjectNavMeshEdgeSpecification();
      _pathLinkUtilitiesCore.SetInaccessibleField(instance, "_start", start);
      _pathLinkUtilitiesCore.SetInaccessibleField(instance, "_end", end);
      _pathLinkUtilitiesCore.SetInaccessibleField(instance, "_isTwoWay", isTwoWay);
      return instance;
    }

    private void FindIntermediateCoordinates(Vector3 pointA, Vector3 pointB, float stepSize = 0.1f)
    {
      Vector3 vector3_1 = pointB - pointA;
      int num = Mathf.FloorToInt(vector3_1.magnitude / stepSize);
      Vector3 vector3_2 = vector3_1.normalized * stepSize;
      Vector3 vector3_3 = pointA;
      List<Vector3> vector3List = new List<Vector3>();
      for (int index = 0; index < num; ++index)
      {
        Vector3Int blockServicePosition = vector3_3.ToBlockServicePosition();
        if (!vector3List.Contains(blockServicePosition))
          vector3List.Add(blockServicePosition);
        vector3_3 += vector3_2;
      }
    }
  }
}
