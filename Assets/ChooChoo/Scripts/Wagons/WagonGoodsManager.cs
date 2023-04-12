using System;
using System.Linq;
using Timberborn.Goods;
using UnityEngine;

namespace ChooChoo
{
    public class WagonGoodsManager : MonoBehaviour
    {
        private WagonManager _wagonManager;
        
        public bool IsCarrying => _wagonManager.Wagons.Any(wagon => wagon.GoodCarrier.IsCarrying);

        public int LiftingCapacity => _wagonManager.Wagons.Sum(wagon => wagon.GoodCarrier.LiftingCapacity);

        private void Awake()
        {
            _wagonManager = GetComponent<WagonManager>();
        }

        public void EmptyWagons()
        {
            foreach (var trainWagon in _wagonManager.Wagons)
                trainWagon.GoodCarrier.EmptyHands();
        }

        public void PutInWagons(GoodAmount goodAmount)
        {
            int wagonCount = _wagonManager.Wagons.Count;
            int remainingGoodAmount = goodAmount.Amount;
            for (var index = 0; index < _wagonManager.Wagons.Count; index++)
            {
                var amount = (int)Math.Ceiling((float)remainingGoodAmount / (wagonCount - index));
                remainingGoodAmount -= amount;
                var trainWagon = _wagonManager.Wagons[index];
                trainWagon.GoodCarrier.PutGoodsInHands(new GoodAmount(goodAmount.GoodId, amount));
            }
        }
    }
}