using System.Collections.Generic;
using System.Collections.ObjectModel;
using Timberborn.Buildings;
using Timberborn.ConstructibleSystem;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.InventorySystem;
using Timberborn.SimpleOutputBuildings;
using UnityEngine;

namespace ChooChoo
{
  public class DropOffPoint : 
    MonoBehaviour,
    IRegisteredComponent,
    IFinishedStateListener,
    IPausableComponent
  {
    private SimpleOutputInventory _simpleOutputInventory;
    private DistrictBuilding _districtBuilding;
    private readonly List<DistributionRoute> _distributionRoutes = new List<DistributionRoute>();

    public ReadOnlyCollection<DistributionRoute> DistributionRoutes { get; private set; }

    public Inventory Inventory => this._simpleOutputInventory.Inventory;

    public DistrictCenter District => this._districtBuilding.District;

    public DistrictCenter InstantDistrict => this._districtBuilding.InstantDistrict;

    public void Awake()
    {
      this._simpleOutputInventory = this.GetComponent<SimpleOutputInventory>();
      this._districtBuilding = this.GetComponent<DistrictBuilding>();
      this.DistributionRoutes = this._distributionRoutes.AsReadOnly();
      this.enabled = false;
    }

    public void OnEnterFinishedState() => this.enabled = true;

    public void OnExitFinishedState()
    {
      this.RemoveAllRoutes();
      this.enabled = false;
    }

    public void LinkRoute(DistributionRoute distributionRoute) => this._distributionRoutes.Add(distributionRoute);

    public void UnlinkRoute(DistributionRoute distributionRoute) => this._distributionRoutes.Remove(distributionRoute);

    private void RemoveAllRoutes()
    {
      for (int index = this._distributionRoutes.Count - 1; index >= 0; --index)
      {
        DistributionRoute distributionRoute = this._distributionRoutes[index];
        distributionRoute.Start.RemoveRoute(distributionRoute);
      }
    }
  }
}
