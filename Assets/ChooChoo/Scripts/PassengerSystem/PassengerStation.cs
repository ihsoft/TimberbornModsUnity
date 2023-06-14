using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.BlockSystemNavigation;
using Timberborn.Common;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Persistence;
using Timberborn.PrefabSystem;
using Timberborn.SingletonSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace ChooChoo
{
  public class PassengerStation : BaseComponent, IFinishedStateListener, IPersistentEntity, IRegisteredComponent
  {
    private readonly ComponentKey PassengerStationKey = new(nameof(PassengerStation));
    private readonly ListKey<PassengerStationLink> PassengerStationLinksKey = new("PassengerStationLinks");
    
    [SerializeField]
    private Vector3Int teleportationEdge;
    [SerializeField]
    private float movementSpeedMultiplier = 1f;
    [SerializeField]
    private bool uiEnabled;
    [SerializeField]
    private bool connectsTwoWay;
    [SerializeField] 
    private int maxPassengers;
    
    private PassengerStationLinkObjectSerializer _passengerStationLinkObjectSerializer;
    private PassengerStationLinkRepository _passengerStationLinkRepository;
    private IDayNightCycle _dayNightCycle;
    private EventBus _eventBus;
    
    private PassengerStationDistrictObject _passengerStationDistrictObject;
    private BlockObjectNavMeshSettings _blockObjectNavMeshSettings;
    private BlockObjectNavMesh _blockObjectNavMesh;
    private DistrictBuilding _districtBuilding;
    private BlockObject _blockObject;
    private Prefab _prefab;
    private BlockObjectNavMeshEdgeSpecification[] _cachedSpecifications;

    public bool UIEnabledEnabled => uiEnabled;
    public string PrefabName => _prefab.PrefabName;
    public float MovementSpeedMultiplier => movementSpeedMultiplier;
    public PassengerStationDistrictObject PassengerStationDistrictObject => _passengerStationDistrictObject;
    public DistrictBuilding DistrictBuilding => _districtBuilding;
    public bool ConnectsTwoWay => connectsTwoWay;
    public List<Passenger> PassengerQueue { get; } = new();
    public List<Passenger> ReservedPassengerQueue { get; } = new();

    public List<Passenger> UnreservedPassengerQueue => PassengerQueue.Where(passenger => !ReservedPassengerQueue.Contains(passenger)).ToList();
    public int MaxPassengers => maxPassengers;

    public Vector3 Location
    {
      get
      {
        var gridCenter = _blockObject.GetPositionedBlock(teleportationEdge).Coordinates - new Vector3(-0.5f, -0.5f, 0.5f);
        var gridCenterGrounded = new Vector3(gridCenter.x, gridCenter.y, _blockObject.Coordinates.z);
        var worldCenterGrounded = CoordinateSystem.GridToWorld(gridCenterGrounded);

        return worldCenterGrounded;
      }
    }

    [Inject]
    public void InjectDependencies(
      PassengerStationLinkObjectSerializer passengerStationLinkObjectSerializer,
      PassengerStationLinkRepository passengerStationLinkRepository,
      IDayNightCycle dayNightCycle,
      EventBus eventBus)
    {
      _passengerStationLinkObjectSerializer = passengerStationLinkObjectSerializer;
      _passengerStationLinkRepository = passengerStationLinkRepository;
      _dayNightCycle = dayNightCycle;
      _eventBus = eventBus;
    }

    private void Awake()
    {
      _passengerStationDistrictObject = GetComponentFast<PassengerStationDistrictObject>();
      _blockObjectNavMeshSettings = GetComponentFast<BlockObjectNavMeshSettings>();
      _blockObjectNavMesh = GetComponentFast<BlockObjectNavMesh>();
      _districtBuilding = GetComponentFast<DistrictBuilding>();
      GetComponentFast<TrainDestination>();
      _blockObject = GetComponentFast<BlockObject>();
      _prefab = GetComponentFast<Prefab>();
      var navMeshEdgeSpecifications = (BlockObjectNavMeshEdgeSpecification[])ChooChooCore.GetInaccessibleField(_blockObjectNavMeshSettings, "_addedEdges");
      _cachedSpecifications = navMeshEdgeSpecifications.ToArray();
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
      _passengerStationLinkRepository.RemoveInvalidLinks();
      _eventBus.Post(new OnConnectedPassengerStationsUpdated());
      _eventBus.Unregister(this);
    }

    public void Save(IEntitySaver entitySaver) => entitySaver.GetComponent(PassengerStationKey).Set(PassengerStationLinksKey, _passengerStationLinkRepository.PathLinks(this).ToList(), _passengerStationLinkObjectSerializer);

    public void Load(IEntityLoader entityLoader)
    {
      if (!entityLoader.HasComponent(PassengerStationKey) || !entityLoader.GetComponent(PassengerStationKey).Has(PassengerStationLinksKey))
        return;
      foreach (PassengerStationLink pathLink in entityLoader.GetComponent(PassengerStationKey).Get(PassengerStationLinksKey, _passengerStationLinkObjectSerializer))
        _passengerStationLinkRepository.AddNew(pathLink);
    }

    public void Connect(PassengerStation endPoint)
    {
      if (
        DistrictBuilding.District != null && endPoint.DistrictBuilding.District != null &&
        DistrictBuilding.District != endPoint.DistrictBuilding.District &&
        (!PassengerStationDistrictObject.GoesAcrossDistrict || !endPoint.PassengerStationDistrictObject.GoesAcrossDistrict))
      {
        return;
      }
      
      float waitingTimeInHours = CalculateWaitingTimeInHours(endPoint);
      // Plugin.Log.LogError(waitingTimeInHours + "");
      _passengerStationLinkRepository.AddNew(new PassengerStationLink(this, endPoint, waitingTimeInHours));
      if (connectsTwoWay)
        _passengerStationLinkRepository.AddNew(new PassengerStationLink(endPoint, this, waitingTimeInHours));
    }

    [OnEvent]
    public void OnPathLinksUpdated(OnConnectedPassengerStationsUpdated onPathLinksUpdated) => UpdateNavMesh();

    private float CalculateWaitingTimeInHours(PassengerStation endPoint) => _dayNightCycle.SecondsToHours(Vector3.Distance(Location, endPoint.Location) / (2.7f * movementSpeedMultiplier));

    private void UpdateNavMesh()
    {
      List<BlockObjectNavMeshEdgeSpecification> list = _cachedSpecifications.ToList();
      foreach (PassengerStationLink pathLink in _passengerStationLinkRepository.PathLinks(this))
      {
        Vector3Int vector =(pathLink.EndLinkPoint.Location - Location).FloorToInt();
        var newVector = new Vector3Int(vector.x, vector.z, vector.y);
        Vector3Int end = _blockObject.Orientation.Untransform(newVector);
        // Plugin.Log.LogWarning(pathLink.EndLinkPoint.Location + "   " + Location);
        // Plugin.Log.LogError(end.ToString());
        list.Add(CreateNewBlockObjectNavMeshEdgeSpecification(teleportationEdge, end, connectsTwoWay));
      }
      _blockObjectNavMesh.NavMeshObject?.RemoveFromNavMesh();
      _blockObjectNavMesh.NavMeshObject?.RemoveFromPreviewNavMesh();
      ChooChooCore.SetInaccessibleField(_blockObjectNavMeshSettings, "_addedEdges", list.ToArray());
      _blockObjectNavMesh.RecalculateNavMeshObject();
      _blockObjectNavMesh.NavMeshObject.AddToNavMesh();
    }

    private BlockObjectNavMeshEdgeSpecification CreateNewBlockObjectNavMeshEdgeSpecification(
      Vector3Int start,
      Vector3Int end,
      bool isTwoWay)
    {
      BlockObjectNavMeshEdgeSpecification instance = new BlockObjectNavMeshEdgeSpecification();
      ChooChooCore.SetInaccessibleField(instance, "_start", start);
      ChooChooCore.SetInaccessibleField(instance, "_end", end);
      ChooChooCore.SetInaccessibleField(instance, "_isTwoWay", isTwoWay);
      return instance;
    }
  }
}
