using Timberborn.Goods;

namespace ChooChoo
{
    public class TrainDistributableGoodAmount
    {
        public readonly GoodAmount GoodAmount;

        public readonly GoodsStation DestinationGoodsStation;

        public TrainDistributableGoodAmount(GoodAmount goodAmount, GoodsStation destinationGoodsStation)
        {
            GoodAmount = goodAmount;
            DestinationGoodsStation = destinationGoodsStation;
        }
    }
}