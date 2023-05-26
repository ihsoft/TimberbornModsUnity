using Bindito.Core;
using System;
using System.Linq;
using Timberborn.BehaviorSystem;
using Timberborn.Common;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using UnityEngine;

namespace ChooChoo
{
  public class TrainCarryRootBehavior : RootBehavior
  {
    private TrainCarryAmountCalculator _trainCarryAmountCalculator;
    private WagonGoodsManager _wagonGoodsManager;
    private MoveToStationExecutor _moveToStationExecutor;

    [Inject]
    public void InjectDependencies(TrainCarryAmountCalculator trainCarryAmountCalculator) 
    {
      _trainCarryAmountCalculator = trainCarryAmountCalculator;
    }

    public void Awake()
    {
      _wagonGoodsManager = GetComponentFast<WagonGoodsManager>();
      _moveToStationExecutor = GetComponentFast<MoveToStationExecutor>();
    }

    public override Decision Decide(BehaviorAgent agent)
    {
      if (_wagonGoodsManager.IsCarrying)
      {
        if (!_wagonGoodsManager.HasReservedCapacity 
            // && !ReserveCapacityForCarriedGoods()
            )
        {
          _wagonGoodsManager.EmptyWagons();
          return Decision.ReleaseNow();
        }

        var currentInventory = _wagonGoodsManager.MostRecentWagons.First(wagon => wagon.GoodReserver.HasReservedCapacity).GoodReserver.CapacityReservation.Inventory;
        var executorStatus = _moveToStationExecutor.Launch(currentInventory.GetEnabledComponent<TrainDestination>());
        // Plugin.Log.LogError("CapacityReservation " + executorStatus); 
        switch (executorStatus)
        {
          case ExecutorStatus.Success:
            return CompleteDelivery(currentInventory);
          case ExecutorStatus.Failure:
            _wagonGoodsManager.UnreserveCapacity();
            return Decision.ReleaseNextTick();
          case ExecutorStatus.Running:
            return Decision.ReturnWhenFinished(_moveToStationExecutor);
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      if (!_wagonGoodsManager.HasReservedCapacity)
        return Decision.ReleaseNow();
      if (_wagonGoodsManager.HasReservedStock)
      {
        var executorStatus = _moveToStationExecutor.Launch(_wagonGoodsManager.MostRecentWagons.First(wagon => wagon.GoodReserver.HasReservedStock).GoodReserver.StockReservation.Inventory.GetEnabledComponent<TrainDestination>());
        // Plugin.Log.LogError("StockReservation " + executorStatus);
        switch (executorStatus)
        {
          case ExecutorStatus.Success:
            return CompleteRetrieval();
          case ExecutorStatus.Failure:
            return UnreserveGood();
          case ExecutorStatus.Running:
            return Decision.ReturnWhenFinished(_moveToStationExecutor);
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      _wagonGoodsManager.UnreserveCapacity();
      return Decision.ReleaseNextTick();
    }

    private Decision CompleteDelivery(Inventory currentInventory)
    {
      _wagonGoodsManager.TryDeliveringGoods(currentInventory);
      return Decision.ReturnNextTick();
    }

    private Decision CompleteRetrieval()
    {
      _wagonGoodsManager.TryRetrievingGoods();
      return Decision.ReturnNextTick();
    }

    private Decision UnreserveGood()
    {
      _wagonGoodsManager.UnreserveStock();
      return Decision.ReleaseNextTick();
    }
    
    // private GoodAmount RecalculateAmountToRetrieve(GoodReservation goodReservation)
    // {
    //   GoodReservation capacityReservation = _goodReserver.CapacityReservation;
    //   _goodReserver.UnreserveCapacity();
    //   GoodAmount carry = _trainCarryAmountCalculator.AmountToCarry(_wagonGoodsManager.LiftingCapacity, goodReservation.GoodAmount.GoodId, capacityReservation.Inventory, goodReservation.Inventory);
    //   _goodReserver.ReserveCapacity(capacityReservation.Inventory, carry);
    //   return carry;
    // }
  }
}
