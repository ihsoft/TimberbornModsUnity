using System.Collections.Generic;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.Goods;
using UnityEngine.UIElements;

namespace ChooChoo
{
  internal class DistributionSettingGroupFactory
  {
    private readonly GoodDistributionSettingItemFactory _goodDistributionSettingItemFactory;
    private readonly VisualElementLoader _visualElementLoader;

    public DistributionSettingGroupFactory(
      GoodDistributionSettingItemFactory goodDistributionSettingItemFactory,
      VisualElementLoader visualElementLoader)
    {
      _goodDistributionSettingItemFactory = goodDistributionSettingItemFactory;
      _visualElementLoader = visualElementLoader;
    }

    public DistributionSettingGroup Create(GoodGroupSpecification groupSpecification, GoodsStationDistributionSetting goodsStationDistributionSetting)
    {
      VisualElement visualElement = _visualElementLoader.LoadVisualElement("Game/BatchControl/DistributionSettingsGroup");
      visualElement.Q<Image>("Icon").sprite = groupSpecification.Icon;
      List<GoodDistributionSettingItem> items = CreateItems(goodsStationDistributionSetting, groupSpecification.Id, visualElement.Q<VisualElement>("Items"));
      return new DistributionSettingGroup(visualElement, items);
    }

    private List<GoodDistributionSettingItem> CreateItems(GoodsStationDistributionSetting goodsStationDistributionSetting, string groupId, VisualElement parent)
    {
      List<GoodDistributionSettingItem> items = new List<GoodDistributionSettingItem>();
      GoodsStationDistributableGoodProvider componentFast = goodsStationDistributionSetting.GetComponentFast<GoodsStation>().GoodsStationDistributableGoodProvider;
      foreach (GoodsStationGoodDistributionSetting goodDistributionSetting in goodsStationDistributionSetting.GetGoodDistributionSettingsForGroup(groupId))
      {
        GoodDistributionSettingItem distributionSettingItem = _goodDistributionSettingItemFactory.Create(componentFast, goodDistributionSetting);
        items.Add(distributionSettingItem);
        parent.Add(distributionSettingItem.Root);
      }
      return items;
    }
  }
}
