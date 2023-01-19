using System.Linq;
using Timberborn.BehaviorSystem;
using Timberborn.Carrying;
using Timberborn.Emptying;
using Timberborn.InventorySystem;
using Timberborn.StockpilePrioritySystem;
using Timberborn.WorkSystem;
using UnityEngine;

namespace ChooChoo
{
  public class FillGoodsStationBehavior : WorkplaceBehavior
  {
    private Inventories _inventories;
    private Emptiable _emptiable;

    public void Awake()
    {
      _inventories = GetComponent<Inventories>();
      _emptiable = GetComponent<Emptiable>();
    }

    public override Decision Decide(GameObject agent)
    {
      if (_emptiable.IsMarkedForEmptying)
        return Decision.ReleaseNow();
      
      CarrierInventoryFinder component = agent.GetComponent<CarrierInventoryFinder>();
      foreach (Inventory enabledInventory in _inventories.EnabledInventories)
      {
        if (StartCarrying(enabledInventory, component))
          return Decision.ReleaseNextTick();
      }
      return Decision.ReleaseNow();
    }

    private bool StartCarrying(
      Inventory inventory,
      CarrierInventoryFinder carrierInventoryFinder)
    {
      foreach (string goodId in inventory.InputGoods.OrderBy(inventory.UnreservedAmountInStock))
      {
        if (carrierInventoryFinder.TryCarryFromAnyInventory(goodId, inventory, CanObtainFrom))
          return true;
      }
      return false;
    }
    
    private bool CanObtainFrom(Inventory inventory)
    {
      GoodsStation component1 = inventory.GetComponent<GoodsStation>();
      GoodObtainer component2 = inventory.GetComponent<GoodObtainer>();
      return 
        !(bool) (Object) component1 &&
        (!(bool)(Object)component2 ||
         !component2.GoodObtainingEnabled);
    }
  }
}
