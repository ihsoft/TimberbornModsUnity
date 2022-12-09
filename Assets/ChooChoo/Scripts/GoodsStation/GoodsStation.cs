using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Timberborn.Buildings;
using Timberborn.Common;
using Timberborn.ConstructibleSystem;
using Timberborn.DistributionSystem;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Navigation;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class GoodsStation : 
    MonoBehaviour,
    IFinishedStateListener,
    IPersistentEntity,
    IPostInitializableLoadedEntity,
    INavMeshListener,
    IInstantNavMeshListener
  {
    private static readonly ComponentKey GoodsPostKey = new(nameof (GoodsStation));
    private static readonly ListKey<DistributionRoute> DistributionRoutesKey = new(nameof (DistributionRoutes));
    [SerializeField]
    private int _maxRoutes;
    [SerializeField]
    private int _bufferSizePerRoute;
    private IGoodService _goodService;
    private NavMeshListenerEntityRegistry _navMeshListenerEntityRegistry;
    private INavigationService _navigationService;
    private DistributionRouteObjectSerializer _distributionRouteObjectSerializer;
    private readonly List<DistributionRoute> _distributionRoutes = new();
    private LimitableGoodDisallower _limitableGoodDisallower;
    private DistrictBuilding _districtBuilding;
    private BuildingAccessible _buildingAccessible;

    public event EventHandler RouteInstantProblemToggled;

    public ReadOnlyCollection<DistributionRoute> DistributionRoutes { get; private set; }

    public Inventory Inventory { get; private set; }

    [Inject]
    public void InjectDependencies(
      IGoodService goodService,
      NavMeshListenerEntityRegistry navMeshListenerEntityRegistry,
      INavigationService navigationService,
      DistributionRouteObjectSerializer distributionRouteObjectSerializer)
    {
      _goodService = goodService;
      _navMeshListenerEntityRegistry = navMeshListenerEntityRegistry;
      _navigationService = navigationService;
      _distributionRouteObjectSerializer = distributionRouteObjectSerializer;
    }

    public DistrictCenter InstantDistrict => _districtBuilding.InstantDistrict;

    public bool CanCompleteAnyRouteInstant => enabled && (bool) (UnityEngine.Object) InstantDistrict;

    public int MaxRoutes => _maxRoutes;

    public void Awake()
    {
      _limitableGoodDisallower = GetComponent<LimitableGoodDisallower>();
      _buildingAccessible = GetComponent<BuildingAccessible>();
      _districtBuilding = GetComponent<DistrictBuilding>();
      DistributionRoutes = _distributionRoutes.AsReadOnly();
      enabled = false;
    }

    public void Save(IEntitySaver entitySaver) => entitySaver.GetComponent(GoodsStation.GoodsPostKey).Set<DistributionRoute>(GoodsStation.DistributionRoutesKey, (IReadOnlyCollection<DistributionRoute>) _distributionRoutes, (IObjectSerializer<DistributionRoute>) _distributionRouteObjectSerializer);

    [BackwardCompatible(2021, 8, 9)]
    public void Load(IEntityLoader entityLoader) => _distributionRoutes.AddRange((IEnumerable<DistributionRoute>) (entityLoader.HasComponent(GoodsStation.GoodsPostKey) ? entityLoader.GetComponent(GoodsStation.GoodsPostKey) : entityLoader.GetComponent(new ComponentKey("DistributionCenter"))).Get<DistributionRoute>(GoodsStation.DistributionRoutesKey, (IObjectSerializer<DistributionRoute>) _distributionRouteObjectSerializer));

    public void PostInitializeLoadedEntity()
    {
      for (int index = _distributionRoutes.Count - 1; index >= 0; --index)
      {
        DistributionRoute distributionRoute = _distributionRoutes[index];
        if (!distributionRoute.End.GetComponent<EntityComponent>().Deleted)
          PostRouteAddition(distributionRoute);
        else
          _distributionRoutes.Remove(distributionRoute);
      }
    }

    public void OnNavMeshUpdated(NavMeshUpdate navMeshUpdate)
    {
      if (!navMeshUpdate.UpdatedRoads)
        return;
      UpdateAllRoutes();
    }

    public void OnInstantNavMeshUpdated(NavMeshUpdate navMeshUpdate)
    {
      if (!navMeshUpdate.UpdatedRoads)
        return;
      UpdateAllRoutesInstant();
    }

    public void InitializeInventory(Inventory inventory)
    {
      Asserts.FieldIsNull<GoodsStation>(this, (object) Inventory, "Inventory");
      Inventory = inventory;
    }

    public void OnEnterFinishedState()
    {
      UpdateAllGoodBufferSizes();
      Inventory.Enable();
      _navMeshListenerEntityRegistry.RegisterNavMeshListener(this);
      _navMeshListenerEntityRegistry.RegisterInstantNavMeshListener(this);
      enabled = true;
    }

    public void OnExitFinishedState()
    {
      RemoveAllRoutes();
      Inventory.Disable();
      _navMeshListenerEntityRegistry.UnregisterNavMeshListener((INavMeshListener) this);
      _navMeshListenerEntityRegistry.UnregisterInstantNavMeshListener((IInstantNavMeshListener) this);
      enabled = false;
    }

    public void AddRoute(DropOffPoint end, string goodId)
    {
      if (_distributionRoutes.Count >= _maxRoutes)
        throw new InvalidOperationException("Can't add another route");
      DistributionRoute distributionRoute = new DistributionRoute(this, end, goodId);
      _distributionRoutes.Add(distributionRoute);
      PostRouteAddition(distributionRoute);
    }

    public void RemoveRoute(DistributionRoute distributionRoute)
    {
      if (!_distributionRoutes.Remove(distributionRoute))
        throw new InvalidOperationException(string.Format("Couldn't remove {0} from {1}, it wasn't added.", (object) distributionRoute, (object) this));
      PostRouteRemoval(distributionRoute);
    }

    public void LackingGoods(List<GoodAmount> lackingGoods)
    {
      DistrictCenter district = District;
      if (!(bool) (UnityEngine.Object) district)
        return;
      DistrictDistributionLimits component1 = district.GetComponent<DistrictDistributionLimits>();
      DistrictGoods component2 = district.GetComponent<DistrictGoods>();
      foreach (DistributionRoute distributionRoute in _distributionRoutes)
      {
        string goodId = distributionRoute.GoodId;
        int a = Mathf.Min(Mathf.Max(MaxAllowedAmount(goodId) - Inventory.UnreservedAmountInStockAndIncoming(goodId), 0), Inventory.UnreservedCapacity(goodId));
        if (a > 0 && component2.HasUnreservedAmountInStock(goodId))
        {
          int b = Mathf.Max(component2.UnreservedAmountInStockAndIncoming(goodId) - component1.GetLowLimit(goodId), 0);
          int amount = Mathf.Min(a, b);
          if (amount > 0)
            lackingGoods.Add(new GoodAmount(goodId, amount));
        }
      }
    }

    public int MaxAllowedAmount(string goodId) => _limitableGoodDisallower.AllowedAmount(goodId);

    public bool CanCompleteRoute(DropOffPoint dropOffPoint) => CanCompleteRoute(dropOffPoint, out float _);

    private DistrictCenter District => _districtBuilding.District;

    private bool CanCompleteAnyRoute => enabled && (bool) (UnityEngine.Object) District;

    private bool CanCompleteRoute(DropOffPoint dropOffPoint, out float distance)
    {
      distance = 0.0f;
      return CanCompleteAnyRoute && dropOffPoint.enabled && (UnityEngine.Object) dropOffPoint.District != (UnityEngine.Object) District && FindPathOnRoad(dropOffPoint, out distance);
    }

    private bool FindPathOnRoad(DropOffPoint dropOffPoint, out float distance)
    {
      distance = 0.0f;
      Vector3? unblockedSingleAccess1 = _buildingAccessible.Accessible.UnblockedSingleAccess;
      if (unblockedSingleAccess1.HasValue)
      {
        Vector3 valueOrDefault1 = unblockedSingleAccess1.GetValueOrDefault();
        Vector3? unblockedSingleAccess2 = dropOffPoint.GetEnabledComponent<Accessible>().UnblockedSingleAccess;
        if (unblockedSingleAccess2.HasValue)
        {
          Vector3 valueOrDefault2 = unblockedSingleAccess2.GetValueOrDefault();
          return _navigationService.FindRoadPathUnlimitedRange(valueOrDefault1, valueOrDefault2, out distance);
        }
      }
      return false;
    }

    private bool CanCompleteRouteInstant(DropOffPoint dropOffPoint) => CanCompleteAnyRouteInstant && dropOffPoint.enabled && (UnityEngine.Object) dropOffPoint.InstantDistrict != (UnityEngine.Object) InstantDistrict && FindPathOnRoadInstant(dropOffPoint);

    private bool FindPathOnRoadInstant(DropOffPoint dropOffPoint)
    {
      Vector3? singleAccessInstant1 = _buildingAccessible.Accessible.UnblockedSingleAccessInstant;
      if (singleAccessInstant1.HasValue)
      {
        Vector3 valueOrDefault1 = singleAccessInstant1.GetValueOrDefault();
        Vector3? singleAccessInstant2 = dropOffPoint.GetEnabledComponent<Accessible>().UnblockedSingleAccessInstant;
        if (singleAccessInstant2.HasValue)
        {
          Vector3 valueOrDefault2 = singleAccessInstant2.GetValueOrDefault();
          return _navigationService.FindRoadPathUnlimitedRangeInstant(valueOrDefault1, valueOrDefault2);
        }
      }
      return false;
    }

    private void RemoveAllRoutes()
    {
      for (int index = _distributionRoutes.Count - 1; index >= 0; --index)
        RemoveRoute(_distributionRoutes[index]);
    }

    private void UpdateAllRoutes()
    {
      foreach (DistributionRoute distributionRoute in _distributionRoutes)
        UpdateRoute(distributionRoute);
    }

    private void UpdateAllRoutesInstant()
    {
      foreach (DistributionRoute distributionRoute in _distributionRoutes)
        UpdateRouteInstant(distributionRoute);
    }

    private void UpdateRoute(DistributionRoute distributionRoute)
    {
      float distance;
      if (CanCompleteRoute(distributionRoute.End, out distance))
      {
        distributionRoute.UnmarkAsHavingProblem();
        distributionRoute.UpdateLength(distance);
      }
      else
        distributionRoute.MarkAsHavingProblem();
    }

    private void UpdateRouteInstant(DistributionRoute distributionRoute)
    {
      DropOffPoint end = distributionRoute.End;
      int num1 = distributionRoute.HasProblemInstant ? 1 : 0;
      if (CanCompleteRouteInstant(end))
        distributionRoute.UnmarkAsHavingProblemInstant();
      else
        distributionRoute.MarkAsHavingProblemInstant();
      int num2 = distributionRoute.HasProblemInstant ? 1 : 0;
      if (num1 == num2)
        return;
      EventHandler instantProblemToggled = RouteInstantProblemToggled;
      if (instantProblemToggled == null)
        return;
      instantProblemToggled((object) this, EventArgs.Empty);
    }

    private void UpdateAllGoodBufferSizes()
    {
      foreach (string good in _goodService.Goods)
        UpdateGoodBufferSize(good);
    }

    private void PostRouteAddition(DistributionRoute distributionRoute)
    {
      distributionRoute.End.LinkRoute(distributionRoute);
      UpdateGoodBufferSize(distributionRoute.GoodId);
      UpdateRoute(distributionRoute);
      UpdateRouteInstant(distributionRoute);
    }

    private void PostRouteRemoval(DistributionRoute distributionRoute)
    {
      distributionRoute.End.UnlinkRoute(distributionRoute);
      UpdateGoodBufferSize(distributionRoute.GoodId);
      EventHandler instantProblemToggled = RouteInstantProblemToggled;
      if (instantProblemToggled == null)
        return;
      instantProblemToggled((object) this, EventArgs.Empty);
    }

    private void UpdateGoodBufferSize(string good)
    {
      int amount = _distributionRoutes.Count<DistributionRoute>((Func<DistributionRoute, bool>) (route => route.GoodId == good)) * _bufferSizePerRoute;
      _limitableGoodDisallower.SetAllowedAmount(good, amount);
    }
  }
}
