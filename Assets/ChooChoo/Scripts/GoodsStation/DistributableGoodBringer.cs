using Bindito.Core;
using System;
using System.Collections.Generic;
using Timberborn.Carrying;
using Timberborn.Common;
using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Navigation;
using UnityEngine;

namespace ChooChoo
{
  internal class DistributableGoodBringer : MonoBehaviour
  {
    private CarryAmountCalculator _carryAmountCalculator;
    private GoodCarrier _goodCarrier;
    private GoodReserver _goodReserver;
    private readonly List<GoodAmount> _sortedLackingGoods = new List<GoodAmount>();

    [Inject]
    public void InjectDependencies(CarryAmountCalculator carryAmountCalculator) => this._carryAmountCalculator = carryAmountCalculator;

    public void Awake()
    {
      this._goodCarrier = this.GetComponent<GoodCarrier>();
      this._goodReserver = this.GetComponent<GoodReserver>();
    }

    public bool BringDistributableGoods(GoodsStation distributionPost)
    {
      this.UpdateLackingGoods(distributionPost);
      foreach (GoodAmount sortedLackingGood in this._sortedLackingGoods)
      {
        DistrictCenter district = distributionPost.GetComponent<DistrictBuilding>().District;
        if ((bool) (UnityEngine.Object) district)
        {
          Accessible enabledComponent = distributionPost.GetEnabledComponent<Accessible>();
          Inventory inventory = district.GetComponent<DistrictInventoryPicker>().ClosestInventoryWithStock(enabledComponent, sortedLackingGood.GoodId);
          if ((bool) (UnityEngine.Object) inventory)
          {
            GoodAmount carry = this._carryAmountCalculator.AmountToCarry(this._goodCarrier.LiftingCapacity, DistributableGoodBringer.MaxTakeableAmount(inventory, sortedLackingGood), (IAmountProvider) distributionPost.Inventory);
            if (carry.Amount > 0)
            {
              this.Reserve(distributionPost, carry, inventory);
              return true;
            }
          }
        }
      }
      return false;
    }

    private void UpdateLackingGoods(GoodsStation distributionPost)
    {
      this._sortedLackingGoods.Clear();
      distributionPost.LackingGoods(this._sortedLackingGoods);
      this._sortedLackingGoods.Sort((Comparison<GoodAmount>) ((x, y) => DistributableGoodBringer.CompareLackingGoods(distributionPost, x, y)));
    }

    private static int CompareLackingGoods(
      GoodsStation distributionPost,
      GoodAmount x,
      GoodAmount y)
    {
      float num = DistributableGoodBringer.LackingGoodPriority(distributionPost, x);
      return DistributableGoodBringer.LackingGoodPriority(distributionPost, y).CompareTo(num);
    }

    private static float LackingGoodPriority(
      GoodsStation distributionPost,
      GoodAmount goodAmount)
    {
      return (float) goodAmount.Amount / (float) distributionPost.MaxAllowedAmount(goodAmount.GoodId);
    }

    private static GoodAmount MaxTakeableAmount(
      Inventory inventory,
      GoodAmount lackingGood)
    {
      int amount = Mathf.Min(inventory.UnreservedAmountInStock(lackingGood.GoodId), lackingGood.Amount);
      return new GoodAmount(lackingGood.GoodId, amount);
    }

    private void Reserve(
      GoodsStation distributionPost,
      GoodAmount carriableGood,
      Inventory closestInventory)
    {
      this._goodReserver.ReserveExactStockAmount(closestInventory, carriableGood);
      this._goodReserver.ReserveCapacity(distributionPost.Inventory, carriableGood);
    }
  }
}
