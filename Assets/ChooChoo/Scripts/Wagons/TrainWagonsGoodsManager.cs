using System.Collections.Generic;
using System.Linq;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainWagonsGoodsManager : MonoBehaviour
    {
        private List<TrainWagonGoodsManager> Wagons { get; } = new();
        public List<TrainWagonGoodsManager> MostRecentWagons { get; } = new();

        public bool IsCarrying => Wagons.Any(wagon => wagon.IsCarrying);

        public bool IsFullOrReserved => MostRecentWagons.All(wagon => wagon.IsFullOrReserved);
        
        public bool IsCarryingOrReserved => MostRecentWagons.Any(wagon => wagon.IsCarryingOrReserved);

        public bool HasReservedCapacity => MostRecentWagons.Any(wagon => wagon.HasReservedCapacity);
        
        public bool HasReservedStock => MostRecentWagons.Any(wagon => wagon.HasReservedStock);

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
        public void TryReservingGood(TrainDistributableGoodAmount trainDistributableGoodAmount, GoodsStation goodsStation)
        {
            int remainingToBeReservedAmount = trainDistributableGoodAmount.GoodAmount.Amount;
            
            foreach (var currentWagon in Wagons)
            {
                if (currentWagon.TryReservingGood(trainDistributableGoodAmount, goodsStation.SendingInventory, ref remainingToBeReservedAmount))
                {
                    MostRecentWagons.MoveItemToFront(currentWagon);
                }

                if (remainingToBeReservedAmount <= 0)
                    break;
            }
            Plugin.Log.LogInfo("remainingToBeReservedAmount: " + remainingToBeReservedAmount);
            if (remainingToBeReservedAmount <= 0 || remainingToBeReservedAmount == trainDistributableGoodAmount.GoodAmount.Amount)
                return;
            trainDistributableGoodAmount.LowerAmount(remainingToBeReservedAmount);
            goodsStation.AddToQueue(TrainDistributableGoodAmount.CreateWithoutAgent(new GoodAmount(trainDistributableGoodAmount.GoodAmount.GoodId, remainingToBeReservedAmount), goodsStation));
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

        public void TryRetrievingGoods(TrainDestination destination)
        {
            foreach (var trainWagon in Wagons) 
                trainWagon.TryRetrievingGoods(destination);
        }
    }
}