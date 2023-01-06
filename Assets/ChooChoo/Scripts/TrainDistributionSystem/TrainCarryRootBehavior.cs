using Bindito.Core;
using System;
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
    private ChooChooCore _chooChooCore;
    private TrainWagonManager _trainWagonManager;
    private GoodReserver _goodReserver;
    private MoveToStationExecutor _moveToStationExecutor;

    [Inject]
    public void InjectDependencies(TrainCarryAmountCalculator trainCarryAmountCalculator, ChooChooCore chooChooCore) 
    {
      _trainCarryAmountCalculator = trainCarryAmountCalculator;
      _chooChooCore = chooChooCore;
    }

    public void Awake()
    {
      _trainWagonManager = GetComponent<TrainWagonManager>();
      _goodReserver = GetComponent<GoodReserver>();
      _moveToStationExecutor = GetComponent<MoveToStationExecutor>();
    }

    public override Decision Decide(GameObject agent)
    {
      if (_trainWagonManager.IsCarrying)
      {
        if (!_goodReserver.HasReservedCapacity 
            // && !ReserveCapacityForCarriedGoods()
            )
        {
          _trainWagonManager.EmptyWagons();
          return Decision.ReleaseNow();
        }

        var test = _moveToStationExecutor.Launch(_goodReserver.CapacityReservation.Inventory.GetEnabledComponent<TrainDestination>());
        // Plugin.Log.LogError("CapacityReservation " + test); 
        switch (test)
        {
          case ExecutorStatus.Success:
            return CompleteDelivery();
          case ExecutorStatus.Failure:
            // _goodReserver.UnreserveCapacity();
            _chooChooCore.SetPrivateProperty(_goodReserver, "CapacityReservation", new GoodReservation());
            return Decision.ReleaseNextTick();
          case ExecutorStatus.Running:
            return Decision.ReturnWhenFinished(_moveToStationExecutor);
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      if (!_goodReserver.HasReservedCapacity)
        return Decision.ReleaseNow();
      if (_goodReserver.HasReservedStock)
      {
        var test = _moveToStationExecutor.Launch(_goodReserver.StockReservation.Inventory.GetEnabledComponent<TrainDestination>());
        // Plugin.Log.LogError("StockReservation " + test);
        switch (test)
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

      // _goodReserver.UnreserveCapacity();
      _chooChooCore.SetPrivateProperty(_goodReserver, "CapacityReservation", new GoodReservation());
      return Decision.ReleaseNextTick();
    }

    private Decision CompleteDelivery()
    {
      GoodReservation capacityReservation = _goodReserver.CapacityReservation;
      // _goodReserver.UnreserveCapacity();
      _chooChooCore.SetPrivateProperty(_goodReserver, "CapacityReservation", new GoodReservation());
      // if (!capacityReservation.Inventory.HasUnreservedCapacity(capacityReservation.GoodAmount))
      //   return Decision.ReleaseNextTick();
      // capacityReservation.Inventory.Give(capacityReservation.GoodAmount);
      var storage = (GoodRegistry)_chooChooCore.GetPrivateField(capacityReservation.Inventory, "_storage");
      storage.Add(capacityReservation.GoodAmount);
      _trainWagonManager.EmptyWagons();
      return Decision.ReturnNextTick();
    }

    private Decision CompleteRetrieval()
    {
      GoodReservation stockReservation = _goodReserver.StockReservation;
      _goodReserver.UnreserveStock();
      GoodAmount goodAmount = stockReservation.FixedAmount ? stockReservation.GoodAmount : RecalculateAmountToRetrieve(stockReservation);
      stockReservation.Inventory.Take(goodAmount);
      _trainWagonManager.PutInWagons(goodAmount);
      return Decision.ReturnNextTick();
    }

    private Decision UnreserveGood()
    {
      _goodReserver.UnreserveStock();
      return Decision.ReleaseNextTick();
    }

    // private bool ReserveCapacityForCarriedGoods()
    // {
    //   Inventory inventoryForCarriedGoods = FindInventoryForCarriedGoods();
    //   if (!((UnityEngine.Object) inventoryForCarriedGoods != (UnityEngine.Object) null))
    //     return false;
    //   _goodReserver.ReserveCapacity(inventoryForCarriedGoods, _trainWagonManager.CarriedGoods);
    //   return true;
    // }

    // private Inventory FindInventoryForCarriedGoods()
    // {
    //   Vector3 start = _navigator.CurrentAccessOrPosition();
    //   GoodAmount carriedGoods = _trainWagonManager.CarriedGoods;
    //   return _citizen.HasAssignedDistrict ? _citizen.AssignedDistrict.GetComponent<DistrictInventoryPicker>().ClosestInventoryWithCapacity(start, carriedGoods) : (Inventory) null;
    // }

    private GoodAmount RecalculateAmountToRetrieve(GoodReservation goodReservation)
    {
      GoodReservation capacityReservation = _goodReserver.CapacityReservation;
      // _goodReserver.UnreserveCapacity();
      _chooChooCore.SetPrivateProperty(_goodReserver, "CapacityReservation", new GoodReservation());
      GoodAmount carry = _trainCarryAmountCalculator.AmountToCarry(_trainWagonManager.LiftingCapacity, goodReservation.GoodAmount.GoodId, capacityReservation.Inventory, goodReservation.Inventory);
      _goodReserver.ReserveCapacity(capacityReservation.Inventory, carry);
      return carry;
    }
  }
}
