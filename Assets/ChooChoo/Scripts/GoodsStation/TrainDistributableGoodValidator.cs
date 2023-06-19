using Timberborn.SingletonSystem;

namespace ChooChoo
{
    public class TrainDistributableGoodValidator : ILoadableSingleton
    {
        private readonly GoodsStationRegistry _goodsStationRegistry;
        private readonly EventBus _eventBus;
        
        TrainDistributableGoodValidator(GoodsStationRegistry goodsStationRegistry, EventBus eventBus)
        {
            _goodsStationRegistry = goodsStationRegistry;
            _eventBus = eventBus;
        }

        public void Load()
        {
            _eventBus.Register(this);
        }
        
        [OnEvent]
        public void OnDeliveryCompletedEvent(DeliveryCompletedEvent deliveryCompletedEvent)
        {
            foreach (var goodsStation in _goodsStationRegistry.FinishedGoodsStations)
            {
                goodsStation.OnDeliveryCompletedEvent(deliveryCompletedEvent.Agent);
            }
        }
        
        [OnEvent]
        public void OnEmptyingHandsEvent(EmptyingHandsEvent emptyingHandsEvent)
        {
            foreach (var goodsStation in _goodsStationRegistry.FinishedGoodsStations)
            {
                goodsStation.OnEmptyingHandsEvent(emptyingHandsEvent.Agent);
            }
        }
    }
}