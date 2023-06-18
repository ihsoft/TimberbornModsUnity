using System.Collections.Generic;
using Timberborn.BatchControl;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;

namespace ChooChoo
{
  internal class DistributionBatchControlTab : BatchControlTab
  {
    public static readonly int TabIndex = 9;
    private readonly GoodsStationRegistry _goodsStationRegistry;
    private readonly DistributionBatchControlRowGroupFactory _distributionBatchControlRowGroupFactory;

    public DistributionBatchControlTab(
      VisualElementLoader visualElementLoader,
      BatchControlDistrict batchControlDistrict,
      GoodsStationRegistry goodsStationRegistry,
      DistributionBatchControlRowGroupFactory distributionBatchControlRowGroupFactory)
      : base(visualElementLoader, batchControlDistrict)
    {
      _goodsStationRegistry = goodsStationRegistry;
      _distributionBatchControlRowGroupFactory = distributionBatchControlRowGroupFactory;
    }

    public override string TabNameLocKey => "Tobbert.BatchControl.GoodsStation";

    public override string TabImage => "Distribution";

    protected override IEnumerable<BatchControlRowGroup> GetRowGroups(IEnumerable<EntityComponent> entities)
    {
      foreach (GoodsStation finishedGoodsStation in _goodsStationRegistry.FinishedGoodsStations)
        yield return _distributionBatchControlRowGroupFactory.Create(finishedGoodsStation);
    }
  }
}
