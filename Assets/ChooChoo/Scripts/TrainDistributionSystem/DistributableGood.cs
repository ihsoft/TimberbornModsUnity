using System.Collections.Generic;
using Timberborn.Goods;

namespace ChooChoo
{
    public class TrainDistributableGood
    {
        public readonly List<TrainWagon> ResolvingTrainWagons = new();
        
        public readonly GoodAmount GoodAmount;

        public readonly GoodsStation DestinationGoodsStation;

        public TrainDistributableGood(GoodAmount goodAmount, GoodsStation destinationGoodsStation)
        {
            GoodAmount = goodAmount;
            DestinationGoodsStation = destinationGoodsStation;
        }

        public void AddResolvingTrainWagon(TrainWagon trainWagon)
        {
            ResolvingTrainWagons.Add(trainWagon);
        }
    }
}