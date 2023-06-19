using Timberborn.Carrying;
using Timberborn.Goods;
using Timberborn.Persistence;

namespace ChooChoo
{
  public class TrainDistributableGoodObjectSerializer : IObjectSerializer<TrainDistributableGoodAmount>
  {
    private static readonly PropertyKey<GoodAmount> GoodAmountKey = new("GoodAmount");
    private static readonly PropertyKey<GoodsStation> DestinationGoodsStationKey = new("DestinationGoodsStation");
    private static readonly PropertyKey<CarryRootBehavior> AgentKey = new("Agent");

    private readonly GoodAmountSerializer _goodAmountSerializer;

    public TrainDistributableGoodObjectSerializer(GoodAmountSerializer goodAmountSerializer)
    {
      _goodAmountSerializer = goodAmountSerializer;
    }

    public void Serialize(TrainDistributableGoodAmount value, IObjectSaver objectSaver)
    {
      objectSaver.Set(GoodAmountKey, value.GoodAmount, _goodAmountSerializer);
      objectSaver.Set(DestinationGoodsStationKey, value.DestinationGoodsStation);
      if (value.Agent != null) 
        objectSaver.Set(AgentKey, value.Agent);
    }

    public Obsoletable<TrainDistributableGoodAmount> Deserialize(IObjectLoader objectLoader)
    {
      if (objectLoader.Has(AgentKey))
      {
        return TrainDistributableGoodAmount.CreateWithAgent(objectLoader.Get(GoodAmountKey, _goodAmountSerializer),
          objectLoader.Get(DestinationGoodsStationKey), objectLoader.Get(AgentKey));
      }

      return TrainDistributableGoodAmount.CreateWithoutAgent(objectLoader.Get(GoodAmountKey, _goodAmountSerializer),
        objectLoader.Get(DestinationGoodsStationKey));
    }
  }
}
