using Timberborn.Goods;
using Timberborn.Persistence;

namespace ChooChoo
{
  public class TrainDistributableGoodObjectSerializer : IObjectSerializer<TrainDistributableGoodAmount>
  {
    private static readonly PropertyKey<GoodAmount> GoodAmountKey = new("GoodAmount");
    private static readonly PropertyKey<GoodsStation> DestinationGoodsStationKey = new("DestinationGoodsStation");

    private readonly GoodAmountSerializer _goodAmountSerializer;

    public TrainDistributableGoodObjectSerializer(GoodAmountSerializer goodAmountSerializer)
    {
      _goodAmountSerializer = goodAmountSerializer;
    }

    public void Serialize(TrainDistributableGoodAmount value, IObjectSaver objectSaver)
    {
      objectSaver.Set(GoodAmountKey, value.GoodAmount, _goodAmountSerializer);
      objectSaver.Set(DestinationGoodsStationKey, value.DestinationGoodsStation);
    }

    public Obsoletable<TrainDistributableGoodAmount> Deserialize(IObjectLoader objectLoader)
    {
      return new TrainDistributableGoodAmount(objectLoader.Get(GoodAmountKey, _goodAmountSerializer), objectLoader.Get(DestinationGoodsStationKey));
      
    }
  }
}
