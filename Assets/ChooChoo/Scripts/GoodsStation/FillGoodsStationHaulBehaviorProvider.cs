using Bindito.Core;
using System.Collections.Generic;
using Timberborn.Buildings;
using Timberborn.Hauling;
using Timberborn.InventorySystem;
using UnityEngine;

namespace ChooChoo
{
  internal class FillGoodsStationHaulBehaviorProvider : MonoBehaviour, IHaulBehaviorProvider
  {
    private InventoryFillCalculator _inventoryFillCalculator;
    private BlockableBuilding _blockableBuilding;
    private Inventories _inventories;
    private FillGoodsStationBehavior _fillGoodsStationBehavior;

    [Inject]
    public void InjectDependencies(InventoryFillCalculator inventoryFillCalculator) => _inventoryFillCalculator = inventoryFillCalculator;

    public void Awake()
    {
      _blockableBuilding = GetComponent<BlockableBuilding>();
      _inventories = GetComponent<Inventories>();
      _fillGoodsStationBehavior = GetComponent<FillGoodsStationBehavior>();
    }

    public IEnumerable<WeightedBehavior> GetWeightedBehaviors()
    {
      foreach (Inventory enabledInventory in _inventories.EnabledInventories)
      {
        if (_blockableBuilding.IsUnblocked)
          yield return new WeightedBehavior(1f - _inventoryFillCalculator.GetInputFillPercentage(enabledInventory), _fillGoodsStationBehavior);
      }
    }
  }
}
