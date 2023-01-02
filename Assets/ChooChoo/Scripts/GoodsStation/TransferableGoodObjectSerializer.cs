using Timberborn.Persistence;

namespace ChooChoo
{
  public class TransferableGoodObjectSerializer : IObjectSerializer<TransferableGood>
  {
    private static readonly PropertyKey<string> GoodIdKey = new("GoodId");
    private static readonly PropertyKey<bool> EnabledKey = new("Enabled");
    private static readonly PropertyKey<bool> CanReceiveGoodsKey = new("CanReceiveGoods");

    public void Serialize(TransferableGood value, IObjectSaver objectSaver)
    {
      objectSaver.Set(GoodIdKey, value.GoodId);
      objectSaver.Set(EnabledKey, value.Enabled);
      objectSaver.Set(CanReceiveGoodsKey, value.SendingGoods);
    }

    public Obsoletable<TransferableGood> Deserialize(IObjectLoader objectLoader)
    {
      return new TransferableGood(objectLoader.Get(GoodIdKey), objectLoader.Get(EnabledKey), objectLoader.Get(CanReceiveGoodsKey));
    }
  }
}
