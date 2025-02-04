﻿using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BehaviorSystem;
using Timberborn.Carrying;
using Timberborn.Common;
using Timberborn.Emptying;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TimeSystem;
using Timberborn.WorkSystem;

namespace ChooChoo
{
  internal class GoodsStationWorkplaceBehavior : WorkplaceBehavior
  {
    private static readonly float PrioritizeEmptyingDistanceSquared = 4f;
    private TrainDestinationService _trainDestinationService;
    private IDayNightCycle _dayNightCycle;
    private TrainDestination _trainDestination;
    private GoodsStation _goodsStation;
    private readonly List<TrainDistributableGood> _otherGoodsStationDistributableGoods = new();

    [Inject]
    public void InjectDependencies(TrainDestinationService trainDestinationService, IDayNightCycle dayNightCycle)
    {
      _trainDestinationService = trainDestinationService;
      _dayNightCycle = dayNightCycle;
    }

    public void Awake()
    {
      _trainDestination = GetComponentFast<TrainDestination>();
      _goodsStation = GetComponentFast<GoodsStation>();
    }

    public override Decision Decide(BehaviorAgent agent) => _goodsStation.CanDistribute ? TryDistribute(agent) : Decision.ReleaseNow();

    private Decision TryDistribute(BehaviorAgent agent)
    {
      if (IsCloseToDistrictCrossing(agent))
      {
        if (TryEmptying(agent) || TryExport(agent))
          return Decision.ReleaseNextTick();
      }
      else if (TryExport(agent) || TryEmptying(agent))
        return Decision.ReleaseNextTick();
      return Decision.ReleaseNow();
    }

    private bool IsCloseToDistrictCrossing(BehaviorAgent agent)
    {
      return (double)(agent.TransformFast.position.XZ() - _goodsStation.TransformFast.position.XZ()).sqrMagnitude < (double)PrioritizeEmptyingDistanceSquared;
    }

    private bool TryEmptying(BehaviorAgent agent)
    {
      return _goodsStation.ReceivingInventory.OutputGoods.Count > 0 && agent.GetComponentFast<EmptyingStarter>()
        .StartEmptying(_goodsStation.ReceivingInventory);
    }

    private bool TryExport(BehaviorAgent agent)
    {
      foreach (var connectedTrainDestination in _trainDestinationService.GetConnectedTrainDestinations(_trainDestination))
      {
        if (_trainDestination == connectedTrainDestination)
          continue;
        if (!connectedTrainDestination.TryGetComponentFast(out GoodsStation goodsStation)) 
          continue;
        if (!goodsStation.CanDistribute)
          continue;
        // Plugin.Log.LogInfo("Can distribute");
        goodsStation.GoodsStationDistributableGoodProvider.GetDistributableGoodsForImport(_otherGoodsStationDistributableGoods);
        // Plugin.Log.LogInfo("_otherGoodsStationDistributableGoods Count: " + _otherGoodsStationDistributableGoods.Count);
        var canExport = TryExportDistributableGoods(agent, goodsStation);
        // Plugin.Log.LogInfo("Can Export: " + canExport);
        _otherGoodsStationDistributableGoods.Clear();
        if (canExport)
          return true;
      }

      return false;
    }

    private bool TryExportDistributableGoods(BehaviorAgent agent, GoodsStation destination)
    {
      return _otherGoodsStationDistributableGoods.Any(good => TryExportGood(agent, good, destination));
    }

    private bool TryExportGood(BehaviorAgent agent, TrainDistributableGood linkedTrainDistributableGood, GoodsStation destination)
    {
      // Plugin.Log.LogInfo("Trying to export");
      string goodId = linkedTrainDistributableGood.GoodId;
      TrainDistributableGood trainDistributableGood = _goodsStation.GetMyDistributableGood(goodId);
      if (_goodsStation.CanExport(trainDistributableGood))
      {
        // Plugin.Log.LogInfo("CAN Export");
        int amountToExport = _goodsStation.GetAmountToExport(trainDistributableGood, linkedTrainDistributableGood);
        if (TryStartCarrying(agent, goodId, amountToExport, linkedTrainDistributableGood, destination))
          return true;
      }
      // Plugin.Log.LogInfo("CANNOT Export");
      return false;
    }

    private bool TryStartCarrying(
      BehaviorAgent agent,
      string goodId,
      int amountToBring,
      TrainDistributableGood linkedTrainDistributableGood,
      GoodsStation destination)
    {
      int val2 = _goodsStation.SendingInventory.UnreservedCapacity(goodId);
      int maxAmount = Math.Min(amountToBring, val2);
      if (maxAmount <= 0 || !agent.GetComponentFast<CarrierInventoryFinder>().TryCarryFromAnyInventoryLimited(goodId, _goodsStation.SendingInventory, maxAmount))
        return false;
      var trainDistributableGoodAmount = TrainDistributableGoodAmount.CreateWithAgent(new GoodAmount(goodId, agent.GetComponentFast<GoodReserver>().CapacityReservation.GoodAmount.Amount), destination, agent.GetComponentFast<CarryRootBehavior>());
      _goodsStation.AddToQueue(trainDistributableGoodAmount);
      linkedTrainDistributableGood.UpdateLastImportTimestamp(_dayNightCycle.PartialDayNumber);
      return true;
    }
  }
}
