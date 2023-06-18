using Timberborn.BatchControl;
using Timberborn.Common;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.EntitySystem;
using Timberborn.Goods;

namespace ChooChoo
{
  internal class DistributionSettingsRowItemFactory
  {
    private readonly DistributionSettingGroupFactory _distributionSettingGroupFactory;
    private readonly GoodsGroupSpecificationService _goodsGroupSpecificationService;
    private readonly VisualElementLoader _visualElementLoader;

    public DistributionSettingsRowItemFactory(
      DistributionSettingGroupFactory distributionSettingGroupFactory,
      GoodsGroupSpecificationService goodsGroupSpecificationService,
      VisualElementLoader visualElementLoader)
    {
      _distributionSettingGroupFactory = distributionSettingGroupFactory;
      _goodsGroupSpecificationService = goodsGroupSpecificationService;
      _visualElementLoader = visualElementLoader;
    }

    public BatchControlRow Create(GoodsStationDistributionSetting goodsStationDistributionSetting)
    {
      return new BatchControlRow(_visualElementLoader.LoadVisualElement("Game/BatchControl/DistributionSettingsRowItem"), goodsStationDistributionSetting.GetComponentFast<EntityComponent>(), CreateSettingGroups(goodsStationDistributionSetting));
    }

    private ReadOnlyList<GoodGroupSpecification> GoodGroupSpecifications => _goodsGroupSpecificationService.GoodGroupSpecifications;

    private IBatchControlRowItem[] CreateSettingGroups(GoodsStationDistributionSetting goodsStationDistributionSetting)
    {
      IBatchControlRowItem[] settingGroups = new IBatchControlRowItem[GoodGroupSpecifications.Count];
      for (int index = 0; index < GoodGroupSpecifications.Count; ++index)
      {
        GoodGroupSpecification groupSpecification = GoodGroupSpecifications[index];
        settingGroups[index] = _distributionSettingGroupFactory.Create(groupSpecification, goodsStationDistributionSetting);
      }
      return settingGroups;
    }
  }
}
