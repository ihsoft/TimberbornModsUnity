using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace ChooChoo
{
  public class DistributionRouteCalculator
  {
    private static readonly int MinLiftingCapacityUsedDivider = 4;
    private readonly IDayNightCycle _dayNightCycle;
    private readonly IGoodService _goodService;

    public DistributionRouteCalculator(IDayNightCycle dayNightCycle, IGoodService goodService)
    {
      this._dayNightCycle = dayNightCycle;
      this._goodService = goodService;
    }

    public float? RouteSatisfaction(DistributionRoute distributionRoute, int liftingCapacity)
    {
      if (distributionRoute.HasProblem || distributionRoute.IsPaused || !this.ThereIsMinAmountOfStockAndCapacity(distributionRoute, liftingCapacity))
        return new float?();
      if (!(bool) (Object) distributionRoute.End.District)
        return new float?(1f);
      return this.MissingAmountToHighLimit(distributionRoute) == 0 ? new float?() : new float?(DistributionRouteCalculator.LowLimitSatisfaction(distributionRoute));
    }

    public int MissingAmountToHighLimit(DistributionRoute distributionRoute)
    {
      DistrictCenter district = distributionRoute.End.District;
      if ((bool) (Object) district)
      {
        string goodId = distributionRoute.GoodId;
        int highLimit;
        if (district.GetComponent<DistrictDistributionLimits>().TryGetHighLimit(goodId, out highLimit))
        {
          int num = district.GetComponent<DistrictGoods>().UnreservedAmountInStockAndIncoming(goodId);
          return Mathf.Max(highLimit - num, 0);
        }
      }
      return int.MaxValue;
    }

    public float HoursToCompleteRoute(
      DistributionRoute distributionRoute,
      float normalWalkingSpeed,
      float slowedWalkingSpeed)
    {
      return this._dayNightCycle.SecondsToHours(distributionRoute.Length / slowedWalkingSpeed + distributionRoute.Length / normalWalkingSpeed);
    }

    private bool ThereIsMinAmountOfStockAndCapacity(
      DistributionRoute distributionRoute,
      int liftingCapacity)
    {
      GoodsStation start = distributionRoute.Start;
      DropOffPoint end = distributionRoute.End;
      string goodId = distributionRoute.GoodId;
      int weight = this._goodService.GetGood(goodId).Weight;
      int amount = Mathf.Max(liftingCapacity / weight / DistributionRouteCalculator.MinLiftingCapacityUsedDivider, 1);
      return start.Inventory.HasUnreservedStock(new GoodAmount(goodId, amount)) && end.Inventory.HasUnreservedCapacity(new GoodAmount(goodId, 1));
    }

    private static float LowLimitSatisfaction(DistributionRoute distributionRoute)
    {
      DistrictCenter district = distributionRoute.End.District;
      string goodId = distributionRoute.GoodId;
      int num = district.GetComponent<DistrictGoods>().UnreservedAmountInStockAndIncoming(goodId);
      int lowLimit = district.GetComponent<DistrictDistributionLimits>().GetLowLimit(goodId);
      return num < lowLimit ? (float) num / (float) lowLimit : 1f;
    }
  }
}
