using Timberborn.Goods;
using Timberborn.Persistence;

namespace ChooChoo
{
  public class TrainDistributableGoodObjectSerializer : IObjectSerializer<TrainDistributableGood>
  {
    private static readonly ListKey<TrainWagon> ResolvingTrainWagonsKey = new("ResolvingTrainWagons");
    private static readonly PropertyKey<GoodAmount> GoodAmountKey = new("GoodAmount");
    private static readonly PropertyKey<GoodsStation> DestinationGoodsStationKey = new("DestinationGoodsStation");

    private readonly GoodAmountSerializer _goodAmountSerializer;

    public TrainDistributableGoodObjectSerializer(GoodAmountSerializer goodAmountSerializer)
    {
      _goodAmountSerializer = goodAmountSerializer;
    }

    public void Serialize(TrainDistributableGood value, IObjectSaver objectSaver)
    {
      objectSaver.Set(ResolvingTrainWagonsKey, value.ResolvingTrainWagons);
      objectSaver.Set(GoodAmountKey, value.GoodAmount, _goodAmountSerializer);
      objectSaver.Set(DestinationGoodsStationKey, value.DestinationGoodsStation);
    }

    public Obsoletable<TrainDistributableGood> Deserialize(IObjectLoader objectLoader)
    {
      var trainDistributableGood = new TrainDistributableGood(objectLoader.Get(GoodAmountKey, _goodAmountSerializer), objectLoader.Get(DestinationGoodsStationKey));
      foreach (var trainWagon in objectLoader.Get(ResolvingTrainWagonsKey))
      {
        trainDistributableGood.AddResolvingTrainWagon(trainWagon);
      }
      return trainDistributableGood;
      
    }
  }
}
