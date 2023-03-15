using System;
using System.Linq;
using Timberborn.Goods;
using UnityEngine;

namespace ChooChoo
{
    public class WagonGoodsManager : MonoBehaviour
    {
        private TrainWagonManager _trainWagonManager;
        
        public bool IsCarrying => _trainWagonManager.TrainWagons.Any(wagon => wagon.GoodCarrier.IsCarrying);

        public int LiftingCapacity => _trainWagonManager.TrainWagons.Sum(wagon => _trainWagonManager.WagonTypes.Contains(wagon.ActiveWagonType) ? wagon.GoodCarrier.LiftingCapacity : 0);

        private void Awake()
        {
            _trainWagonManager = GetComponent<TrainWagonManager>();
        }

        public void EmptyWagons()
        {
            foreach (var trainWagon in _trainWagonManager.TrainWagons)
                trainWagon.GoodCarrier.EmptyHands();
        }

        public void PutInWagons(GoodAmount goodAmount)
        {
            int wagonCount = _trainWagonManager.TrainWagons.Count;
            int remainingGoodAmount = goodAmount.Amount;
            for (var index = 0; index < _trainWagonManager.TrainWagons.Count; index++)
            {
                var amount = (int)Math.Ceiling((float)remainingGoodAmount / (wagonCount - index));
                remainingGoodAmount -= amount;
                var trainWagon = _trainWagonManager.TrainWagons[index];
                trainWagon.GoodCarrier.PutGoodsInHands(new GoodAmount(goodAmount.GoodId, amount));
            }
        }
    }
}