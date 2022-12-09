// Decompiled with JetBrains decompiler
// Type: Timberborn.DistributionSystem.DistributionRouteObjectSerializer
// Assembly: Timberborn.DistributionSystem, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C616A16C-F6F1-4897-8555-D530DF676979
// Assembly location: C:\Users\Tobbert\SynologyDrive\C# Projecten\TobbertMods\TobbertMods\Timberborn\Timberborn_Data\Managed\Timberborn.DistributionSystem.dll

using Timberborn.Goods;
using Timberborn.Persistence;

namespace ChooChoo
{
  public class DistributionRouteObjectSerializer : IObjectSerializer<DistributionRoute>
  {
    private static readonly PropertyKey<GoodsStation> StartKey = new PropertyKey<GoodsStation>("Start");
    private static readonly PropertyKey<DropOffPoint> EndKey = new PropertyKey<DropOffPoint>("End");
    private static readonly PropertyKey<SavedGood> GoodKey = new PropertyKey<SavedGood>("Good");
    private static readonly PropertyKey<float> LastCompletionTimestampKey = new PropertyKey<float>("LastCompletionTimestamp");
    private readonly SavedGoodObjectSerializer _savedGoodObjectSerializer;

    public DistributionRouteObjectSerializer(
      SavedGoodObjectSerializer savedGoodObjectSerializer)
    {
      this._savedGoodObjectSerializer = savedGoodObjectSerializer;
    }

    public void Serialize(DistributionRoute value, IObjectSaver objectSaver)
    {
      objectSaver.Set<GoodsStation>(DistributionRouteObjectSerializer.StartKey, value.Start);
      objectSaver.Set<DropOffPoint>(DistributionRouteObjectSerializer.EndKey, value.End);
      objectSaver.Set<SavedGood>(DistributionRouteObjectSerializer.GoodKey, SavedGood.Create(value.GoodId), (IObjectSerializer<SavedGood>) this._savedGoodObjectSerializer);
      objectSaver.Set(DistributionRouteObjectSerializer.LastCompletionTimestampKey, value.LastCompletionTimestamp);
    }

    public Obsoletable<DistributionRoute> Deserialize(
      IObjectLoader objectLoader)
    {
      SavedGood savedGood;
      return objectLoader.GetObsoletable<SavedGood>(DistributionRouteObjectSerializer.GoodKey, (IObjectSerializer<SavedGood>) this._savedGoodObjectSerializer, out savedGood) ? (Obsoletable<DistributionRoute>) new DistributionRoute(objectLoader.Get<GoodsStation>(DistributionRouteObjectSerializer.StartKey), objectLoader.Get<DropOffPoint>(DistributionRouteObjectSerializer.EndKey), savedGood.Id, objectLoader.Get(DistributionRouteObjectSerializer.LastCompletionTimestampKey)) : new Obsoletable<DistributionRoute>();
    }
  }
}
