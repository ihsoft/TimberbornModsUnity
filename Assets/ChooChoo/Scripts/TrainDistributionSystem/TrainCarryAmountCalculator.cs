using System;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using UnityEngine;

namespace ChooChoo
{
  public class TrainCarryAmountCalculator
  {
    private readonly IGoodService _goodService;

    public TrainCarryAmountCalculator(IGoodService goodService) => _goodService = goodService;

    public GoodAmount AmountToCarry(
      int liftingCapacity,
      string goodId,
      IAmountProvider input,
      IAmountProvider output)
    {
      GoodAmount good = new GoodAmount(goodId, output.UnreservedAmountInStock(goodId));
      return AmountToCarry(liftingCapacity, good);
    }

    public GoodAmount AmountToCarry(
      int liftingCapacity,
      GoodAmount good)
    {
      GoodSpecification good1 = _goodService.GetGood(good.GoodId);
      int num1 = Math.Max(liftingCapacity / good1.Weight, 1);
      int amount = Mathf.Min(new []
      {
        num1,
        good.Amount
      });
      // Plugin.Log.LogError(amount + " " + num1 + " " + good.Amount);
      return new GoodAmount(good.GoodId, amount);
    }
  }
}
