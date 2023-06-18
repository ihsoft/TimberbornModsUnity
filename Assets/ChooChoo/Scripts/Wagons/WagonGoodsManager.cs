using System;
using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using UnityEngine;

namespace ChooChoo
{
    // TODO rename to: TrainWagonsGoodsManager+
    public class WagonGoodsManager : MonoBehaviour
    {
        private ChooChooCarryAmountCalculator _chooChooCarryAmountCalculator;
        private List<TrainWagonGoodsManager> Wagons { get; } = new();
        public List<TrainWagonGoodsManager> MostRecentWagons { get; } = new();

        public bool IsCarrying => Wagons.Any(wagon => wagon.IsCarrying);

        public bool IsFullOrReserved => MostRecentWagons.All(wagon => wagon.IsFullOrReserved);
        
        public bool IsCarryingOrReserved => MostRecentWagons.Any(wagon => wagon.IsCarryingOrReserved);

        public bool HasReservedCapacity => MostRecentWagons.Any(wagon => wagon.HasReservedCapacity);
        
        public bool HasReservedStock => MostRecentWagons.Any(wagon => wagon.HasReservedStock);

        [Inject]
        public void InjectDependencies(ChooChooCarryAmountCalculator chooChooCarryAmountCalculator)
        {
            _chooChooCarryAmountCalculator = chooChooCarryAmountCalculator;
        }

        private void Start()
        {
            foreach (var trainWagon in GetComponent<WagonManager>().Wagons)
            {
                var trainWagonGoodsManager = trainWagon.GetComponentFast<TrainWagonGoodsManager>();
                Wagons.Add(trainWagonGoodsManager);
                MostRecentWagons.Add(trainWagonGoodsManager);
            }
        }

        // Important to remember that reserving can be from a different inventory every time. So the stock reservation might be from a different inventory.
        public void TryReservingGood(TrainDistributableGoodAmount trainDistributableGoodAmount, Inventory sendingInventory)
        {
            int remainingToBeReservedAmount = trainDistributableGoodAmount.GoodAmount.Amount;
            
            foreach (var currentWagon in Wagons)
            {
                if (currentWagon.TryReservingGood(trainDistributableGoodAmount, sendingInventory, ref remainingToBeReservedAmount))
                {
                    MostRecentWagons.MoveItemToFront(currentWagon);
                }

                if (remainingToBeReservedAmount == 0)
                    break;
            }
        }

        public void TryDeliveringGoods(Inventory currentInvetory)
        {
            foreach (var trainWagon in Wagons)
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
            foreach (var trainWagon in Wagons)
                trainWagon.GoodCarrier.EmptyHands();
        }

        public void UnreserveCapacity()
        {
            foreach (var trainWagon in Wagons)
                trainWagon.GoodReserver.UnreserveCapacity();
        }
        
        public void UnreserveStock()
        {
            foreach (var trainWagon in Wagons)
                trainWagon.GoodReserver.UnreserveStock();
        }

        public void TryRetrievingGoods()
        {
            foreach (var trainWagon in Wagons) 
                trainWagon.TryRetrievingGoods();
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