using System;
using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using UnityEngine;

namespace ChooChoo
{
    public class WagonGoodsManager : MonoBehaviour
    {
        private TrainCarryAmountCalculator _trainCarryAmountCalculator;
        
        private WagonManager _wagonManager;

        [NonSerialized]
        public List<TrainWagon> MostRecentWagons;

        public bool IsCarrying => _wagonManager.Wagons.Any(wagon => wagon.GoodCarrier.IsCarrying);

        public bool IsFull =>
            _wagonManager.Wagons.All(wagon => 
                wagon.GoodCarrier.IsCarrying ||
                (wagon.GoodReserver.HasReservedStock && 
                 _trainCarryAmountCalculator.IsAtMaximumCarryCapacity(wagon.GoodCarrier.LiftingCapacity, wagon.GoodReserver.StockReservation.GoodAmount)));

        public bool HasReservedCapacity => _wagonManager.Wagons.Any(wagon => wagon.GoodReserver.HasReservedCapacity);
        
        public bool HasReservedStock => _wagonManager.Wagons.Any(wagon => wagon.GoodReserver.HasReservedStock);

        [Inject]
        public void InjectDependencies(TrainCarryAmountCalculator trainCarryAmountCalculator)
        {
            _trainCarryAmountCalculator = trainCarryAmountCalculator;
        }
        
        private void Awake()
        {
            _wagonManager = GetComponent<WagonManager>();
        }

        private void Start()
        {
            MostRecentWagons = _wagonManager.Wagons.ToList();
        }

        public void TryReservingGood(TrainDistributableGood trainDistributableGood, Inventory sendingInventory)
        {
            var toBeReservedGoodAmount = trainDistributableGood.GoodAmount;
            int remainingToBeReservedAmount = toBeReservedGoodAmount.Amount;
            
            foreach (var currentWagon in _wagonManager.Wagons)
            {
                Plugin.Log.LogWarning("Looking to reserve: " + remainingToBeReservedAmount);
                var destinationGoodsStation = trainDistributableGood.DestinationGoodsStation;
                var goodReserver = currentWagon.GoodReserver;
                var goodCarrier = currentWagon.GoodCarrier;
                
                GoodAmount carry = _trainCarryAmountCalculator.AmountToCarry(goodCarrier.LiftingCapacity, MaxTakeableAmount(sendingInventory, trainDistributableGood.GoodAmount));
                Plugin.Log.LogError("Carry Amount: " + carry.Amount);
                if (carry.Amount <= 0) 
                    continue;
                
                var maxAmountToCarry = _trainCarryAmountCalculator.MaxAmountToCarry(goodCarrier.LiftingCapacity, toBeReservedGoodAmount.GoodId);
                Plugin.Log.LogError("Max Amount able to carry: " + maxAmountToCarry);
                
                MostRecentWagons.MoveItemToFront(currentWagon);
                
                if (goodCarrier.IsCarrying)
                {
                    var currentlyCarriedGoods = goodCarrier.CarriedGoods;
                    // 30 = 50 - 20
                    var fillableAmount = maxAmountToCarry - currentlyCarriedGoods.Amount;
                    if (
                        // 50 > 20
                        maxAmountToCarry > currentlyCarriedGoods.Amount &&
                        goodReserver.CapacityReservation.Inventory == destinationGoodsStation.ReceivingInventory &&
                        currentlyCarriedGoods.GoodId == toBeReservedGoodAmount.GoodId && 
                        remainingToBeReservedAmount > fillableAmount)
                    {
                        // ReserveAlreadyCarryingWagon(goodReserver, carry, maxAmountToCarry, closestInventory, destinationGoodsStation);
                        goodReserver.UnreserveCapacity();
                        goodReserver.ReserveExactStockAmount(sendingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, fillableAmount));
                        goodReserver.ReserveCapacity(destinationGoodsStation.ReceivingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, maxAmountToCarry));
                        remainingToBeReservedAmount -= fillableAmount;
                        trainDistributableGood.AddResolvingTrainWagon(currentWagon);
                    }
                    continue;
                }

                if (goodReserver.HasReservedStock)
                {
                    var stockReservation = goodReserver.StockReservation;
                    var currentAmount = stockReservation.GoodAmount.Amount;
                    // 30 = 50 - 20
                    var fillableAmount = maxAmountToCarry - currentAmount;
                    Plugin.Log.LogError("StockReservation fillableAmount: " + fillableAmount);
                    if (
                        // 50 > 20
                        maxAmountToCarry > currentAmount &&
                        goodReserver.CapacityReservation.Inventory == destinationGoodsStation.ReceivingInventory &&
                        stockReservation.GoodAmount.GoodId == toBeReservedGoodAmount.GoodId && 
                        fillableAmount > 0)
                    {
                        if (remainingToBeReservedAmount > fillableAmount)
                        {
                            // ReserveAlreadyCarryingWagon(goodReserver, carry, maxAmountToCarry, closestInventory, destinationGoodsStation);
                            goodReserver.UnreserveStock();
                            goodReserver.UnreserveCapacity();
                            goodReserver.ReserveExactStockAmount(sendingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, maxAmountToCarry));
                            goodReserver.ReserveCapacity(destinationGoodsStation.ReceivingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, maxAmountToCarry));
                            remainingToBeReservedAmount -= fillableAmount;
                            trainDistributableGood.AddResolvingTrainWagon(currentWagon);
                            continue;
                        }

                        var combinedAmount = remainingToBeReservedAmount + currentAmount;
                        goodReserver.UnreserveStock();
                        goodReserver.UnreserveCapacity();
                        goodReserver.ReserveExactStockAmount(sendingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, combinedAmount));
                        goodReserver.ReserveCapacity(destinationGoodsStation.ReceivingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, combinedAmount));
                        trainDistributableGood.AddResolvingTrainWagon(currentWagon);
                        break;
                    }
                    continue;
                }

                // 60 > 50 
                if (remainingToBeReservedAmount > carry.Amount)
                {
                    goodReserver.ReserveExactStockAmount(sendingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, carry.Amount));
                    goodReserver.ReserveCapacity(destinationGoodsStation.ReceivingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, carry.Amount));
                    remainingToBeReservedAmount -= carry.Amount;
                    trainDistributableGood.AddResolvingTrainWagon(currentWagon);
                    continue;
                }
                
                // 10 > 50
                goodReserver.ReserveExactStockAmount(sendingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, remainingToBeReservedAmount));
                goodReserver.ReserveCapacity(destinationGoodsStation.ReceivingInventory, new GoodAmount(toBeReservedGoodAmount.GoodId, remainingToBeReservedAmount));
                trainDistributableGood.AddResolvingTrainWagon(currentWagon);
                break;
            }
        }

        public void TryDeliveringGoods(Inventory currentInvetory)
        {
            foreach (var trainWagon in _wagonManager.Wagons)
            {
                var goodReserver = trainWagon.GoodReserver;
                
                GoodReservation capacityReservation = goodReserver.CapacityReservation;

                if (currentInvetory != capacityReservation.Inventory)
                    continue;
                goodReserver.UnreserveCapacity();
                capacityReservation.Inventory.Give(capacityReservation.GoodAmount);
                trainWagon.GoodCarrier.EmptyHands();
            }
        }

        public void EmptyWagons()
        {
            foreach (var trainWagon in _wagonManager.Wagons)
                trainWagon.GoodCarrier.EmptyHands();
        }

        public void UnreserveCapacity()
        {
            foreach (var trainWagon in _wagonManager.Wagons)
                trainWagon.GoodReserver.UnreserveCapacity();
        }
        
        public void UnreserveStock()
        {
            foreach (var trainWagon in _wagonManager.Wagons)
                trainWagon.GoodReserver.UnreserveStock();
        }

        public void TryRetrievingGoods()
        {
            foreach (var trainWagon in _wagonManager.Wagons)
            {
                var goodReserver = trainWagon.GoodReserver;
                if (!goodReserver.HasReservedStock)
                    continue;
                GoodReservation stockReservation = goodReserver.StockReservation;
                goodReserver.UnreserveStock();
                stockReservation.Inventory.Take(stockReservation.GoodAmount);
                var goodCarrier = trainWagon.GoodCarrier;
                if (goodCarrier.IsCarrying)
                {
                    goodCarrier.PutGoodsInHands(new GoodAmount(stockReservation.GoodAmount.GoodId, goodCarrier.CarriedGoods.Amount + stockReservation.GoodAmount.Amount));
                }
                else
                {
                    goodCarrier.PutGoodsInHands(stockReservation.GoodAmount);
                }
                stockReservation.Inventory.GetComponentFast<GoodsStation>().ResolveCorrespondingQueueItems(trainWagon);
            }
        }

        private void OverwriteCurrentReservations(GoodReserver goodReserver, GoodAmount carry, int liftingCapacity, Inventory closestInventory, GoodsStation destinationGoodsStation)
        {
            // goodReserver.UnreserveCapacity();
            // goodReserver.UnreserveStock();
            // var capacity = new GoodAmount(carry.GoodId, liftingCapacity);
            // goodReserver.ReserveExactStockAmount(closestInventory, capacity);
            // goodReserver.ReserveCapacity(destinationGoodsStation.ReceivingInventory, capacity);
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