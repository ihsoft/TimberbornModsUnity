using System.Collections.Generic;
using Bindito.Core;
using Timberborn.BaseComponentSystem;
using Timberborn.Carrying;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainWagonGoodsManager : BaseComponent
    {
        private static bool _shouldLog = true;
        
        private ChooChooCarryAmountCalculator _chooChooCarryAmountCalculator;

        private List<TrainDistributableGoodAmount> _currentTrainDistributableGoods = new();

        public GoodCarrier GoodCarrier { get; private set; }
        public GoodReserver GoodReserver { get; private set; }

        [Inject]
        public void InjectDependencies(ChooChooCarryAmountCalculator chooChooCarryAmountCalculator)
        {
            _chooChooCarryAmountCalculator = chooChooCarryAmountCalculator;
        }
        
        public bool IsCarrying => GoodCarrier.IsCarrying;
        
        public bool IsFullOrReserved =>
            GoodCarrier.IsCarrying ||
                (GoodReserver.HasReservedStock && 
                 _chooChooCarryAmountCalculator.IsAtMaximumCarryCapacity(GoodCarrier.LiftingCapacity, GoodReserver.StockReservation.GoodAmount));
        
        public bool IsCarryingOrReserved => GoodCarrier.IsCarrying || GoodReserver.HasReservedStock;
        
        public bool HasReservedCapacity => GoodReserver.HasReservedCapacity;
        
        public bool HasReservedStock => GoodReserver.HasReservedStock;
        
        public void Awake()
        {
            GoodCarrier = GetComponentFast<GoodCarrier>();
            GoodReserver = GetComponentFast<GoodReserver>();
        }

        public bool TryReservingGood(TrainDistributableGoodAmount trainDistributableGoodAmount, Inventory sendingInventory, ref int remainingToBeReservedAmount)
        {
            var toBeReservedGoodId = trainDistributableGoodAmount.GoodAmount.GoodId;
            if (_shouldLog) Plugin.Log.LogInfo("Looking to reserve: " + remainingToBeReservedAmount + " " + toBeReservedGoodId);
            
            GoodAmount carry = _chooChooCarryAmountCalculator.AmountToCarry(GoodCarrier.LiftingCapacity, MaxTakeableAmount(sendingInventory, trainDistributableGoodAmount.GoodAmount));
            if (_shouldLog) Plugin.Log.LogInfo("Carry Amount: " + carry.Amount);
            if (carry.Amount <= 0)
                return false;
            
            var maxAmountToCarry = _chooChooCarryAmountCalculator.MaxAmountToCarry(GoodCarrier.LiftingCapacity, toBeReservedGoodId);
            if (_shouldLog) Plugin.Log.LogInfo("Max Amount able to carry: " + maxAmountToCarry);
            
            var receivingInventory = trainDistributableGoodAmount.DestinationGoodsStation.ReceivingInventory;
            
            // there are 2 cases: toBeReservedAmount is 60 or 10 and carrying capacity is to be expected 50

            if (IsCarrying && HasReservedStock)
            {
                if (_shouldLog) Plugin.Log.LogError("Both Carrying AND already has Reserved Stock.");
                return false;
            }

            if (IsCarrying || HasReservedStock)
            {
                if (_shouldLog) Plugin.Log.LogWarning("Is carrying OR has Reserved");
                var currentAmount = IsCarrying ? GoodCarrier.CarriedGoods.Amount : GoodReserver.StockReservation.GoodAmount.Amount;
                // 30 = 50 - 20 which means that fillable amount is 30.
                var fillableAmount = FillableAmount(maxAmountToCarry, currentAmount);
                if (_shouldLog) Plugin.Log.LogInfo("Fillable amount: " + fillableAmount);
                // 30 > 0 which means that there the wagon can be topped up.
                if (SameOriginAndDestinationAndGood(sendingInventory, receivingInventory, toBeReservedGoodId) && fillableAmount > 0)
                {
                    if (_shouldLog) Plugin.Log.LogInfo("Fillable");
                    if (remainingToBeReservedAmount > fillableAmount)
                    {
                        // 60 > 30 which means that there is more to reserve than the amount that can be filled. It will try to top up the wagon and there will be a remainder that has to be reserved. 
                        Reserve(sendingInventory, receivingInventory, toBeReservedGoodId, maxAmountToCarry);
                        remainingToBeReservedAmount -= fillableAmount;
                        _currentTrainDistributableGoods.Add(trainDistributableGoodAmount);
                        return true;
                    }
                    // 10 > 30 which means that there is less to reserve than the amount that can be filled. Means it can still be filled, but the current queue item is completed. 
                    var combinedAmount = remainingToBeReservedAmount + currentAmount;
                    Reserve(sendingInventory, receivingInventory, toBeReservedGoodId, combinedAmount);
                    remainingToBeReservedAmount -= remainingToBeReservedAmount;
                    _currentTrainDistributableGoods.Add(trainDistributableGoodAmount);
                    return true;
                }
                
                if (_shouldLog) Plugin.Log.LogInfo("Not fillable");
                return false;
            }

            if (_shouldLog) Plugin.Log.LogWarning("Is Empty");
            // 60 > 50 
            if (remainingToBeReservedAmount > carry.Amount)
            {
                Reserve(sendingInventory, receivingInventory, toBeReservedGoodId, carry.Amount);
                remainingToBeReservedAmount -= carry.Amount;
                _currentTrainDistributableGoods.Add(trainDistributableGoodAmount);
                return true;
            }
            
            // 10 > 50
            Reserve(sendingInventory, receivingInventory, toBeReservedGoodId, remainingToBeReservedAmount);
            remainingToBeReservedAmount -= remainingToBeReservedAmount;
            _currentTrainDistributableGoods.Add(trainDistributableGoodAmount);
            return true;
        }

        public void TryRetrievingGoods()
        {
            if (!HasReservedStock)
                return;
            GoodReservation stockReservation = GoodReserver.StockReservation;
            GoodReserver.UnreserveStock();
            stockReservation.Inventory.Take(stockReservation.GoodAmount);
            if (IsCarrying)
            {
                GoodCarrier.PutGoodsInHands(new GoodAmount(stockReservation.GoodAmount.GoodId, GoodCarrier.CarriedGoods.Amount + stockReservation.GoodAmount.Amount));
            }
            else
            {
                GoodCarrier.PutGoodsInHands(stockReservation.GoodAmount);
            }
            var goodsStation = stockReservation.Inventory.GetComponentFast<GoodsStation>();
            foreach (var trainDistributableGoodAmount in _currentTrainDistributableGoods)
                goodsStation.ResolveRetrieval(trainDistributableGoodAmount);
        }

        private bool SameOriginAndDestinationAndGood(Inventory origin, Inventory destination, string  goodId)
        {
            Plugin.Log.LogInfo("Same origin: " + (GoodReserver.CapacityReservation.Inventory == destination) + "Same destination: " + (GoodReserver.CapacityReservation.Inventory == destination) + " And goodId: " + (GoodReserver.CapacityReservation.GoodAmount.GoodId == goodId));
            return GoodReserver.StockReservation.Inventory == origin && GoodReserver.CapacityReservation.Inventory == destination && GoodReserver.CapacityReservation.GoodAmount.GoodId == goodId;
        }

        private int FillableAmount(int maxAmount, int currentAmount)
        {
            if (_shouldLog) Plugin.Log.LogInfo($"Calculate fillable amount (max amount - current amount): {maxAmount} - {currentAmount} = {maxAmount - currentAmount}");
            return maxAmount - currentAmount;
        }

        private void Reserve(Inventory sendingInventory, Inventory receivingInventory, string goodId, int amountToBeReserved)
        {
            GoodReserver.ReserveExactStockAmount(sendingInventory, new GoodAmount(goodId, amountToBeReserved));
            GoodReserver.ReserveCapacity(receivingInventory, new GoodAmount(goodId, amountToBeReserved));
        }

        private GoodAmount MaxTakeableAmount(
            Inventory inventory,
            GoodAmount lackingGood)
        {
            int amount = Mathf.Min(inventory.UnreservedAmountInStock(lackingGood.GoodId), lackingGood.Amount);
            return new GoodAmount(lackingGood.GoodId, amount);
        }
    }
}