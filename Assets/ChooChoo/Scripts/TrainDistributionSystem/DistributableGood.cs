using Timberborn.Carrying;
using Timberborn.Goods;

namespace ChooChoo
{
    public class TrainDistributableGoodAmount
    {
        private DeliveryStatus _deliveryStatus = DeliveryStatus.Running;
        
        public GoodAmount GoodAmount;
        public readonly GoodsStation DestinationGoodsStation;
        public readonly CarryRootBehavior Agent;

        public bool BeaverDeliveryCompleted => _deliveryStatus == DeliveryStatus.Success;

        TrainDistributableGoodAmount(GoodAmount goodAmount, GoodsStation destinationGoodsStation, CarryRootBehavior agent)
        {
            GoodAmount = goodAmount;
            DestinationGoodsStation = destinationGoodsStation;
            Agent = agent;
        }

        public static TrainDistributableGoodAmount CreateWithAgent(GoodAmount goodAmount, GoodsStation destinationGoodsStation, CarryRootBehavior agent)
        {
            return new TrainDistributableGoodAmount(goodAmount, destinationGoodsStation, agent);
        }
        
        public static TrainDistributableGoodAmount CreateWithoutAgent(GoodAmount goodAmount, GoodsStation destinationGoodsStation)
        {
            return new TrainDistributableGoodAmount(goodAmount, destinationGoodsStation, null);
        }

        public void LowerAmount(int amount)
        {
            var goodAmount = GoodAmount;
            GoodAmount = new GoodAmount(goodAmount.GoodId, goodAmount.Amount - amount);
        }

        public bool TryDelivering(CarryRootBehavior agent)
        {
            if (Agent == agent)
            {
                _deliveryStatus = DeliveryStatus.Success;
                return true;
            }

            return false;   
        }

        public bool TryDeliveryFailed(CarryRootBehavior agent)
        {
            if (Agent != agent) 
                return false;
            if (_deliveryStatus == DeliveryStatus.Success)
                return false;
            _deliveryStatus = DeliveryStatus.Failure;
            return true;
        }
    }
}