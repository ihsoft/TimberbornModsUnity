using System.Collections.Generic;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.Goods;
using Timberborn.GoodsUI;
using Timberborn.TooltipSystem;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class ImportGoodIconFactory
  {
    private readonly GoodDescriber _goodDescriber;
    private readonly IGoodService _goodService;
    private readonly GoodsGroupSpecificationService _goodsGroupSpecificationService;
    private readonly ITooltipRegistrar _tooltipRegistrar;
    private readonly VisualElementLoader _visualElementLoader;

    public ImportGoodIconFactory(
      GoodDescriber goodDescriber,
      IGoodService goodService,
      GoodsGroupSpecificationService goodsGroupSpecificationService,
      ITooltipRegistrar tooltipRegistrar,
      VisualElementLoader visualElementLoader)
    {
      _goodDescriber = goodDescriber;
      _goodService = goodService;
      _goodsGroupSpecificationService = goodsGroupSpecificationService;
      _tooltipRegistrar = tooltipRegistrar;
      _visualElementLoader = visualElementLoader;
    }

    public IEnumerable<ImportGoodIcon> CreateImportGoods(VisualElement parent)
    {
      List<ImportGoodIcon> importGoods = new List<ImportGoodIcon>();
      foreach (GoodGroupSpecification groupSpecification in _goodsGroupSpecificationService.GoodGroupSpecifications)
        importGoods.AddRange(CreateImportGoodsGroup(parent, groupSpecification));
      return importGoods;
    }

    public ImportGoodIcon CreateImportGoodIcon(VisualElement parent, string goodId)
    {
      VisualElement visualElement = _visualElementLoader.LoadVisualElement("Game/ImportGoodIcon");
      parent.Add(visualElement);
      Image image = visualElement.Q<Image>("Icon");
      DescribedGood describedGood = _goodDescriber.GetDescribedGood(goodId);
      image.sprite = describedGood.Icon;
      VisualElement importableIcon = visualElement.Q<VisualElement>("ImportableIcon");
      VisualElement nonImportableIcon = visualElement.Q<VisualElement>("NonImportableIcon");
      ImportGoodIcon importGoodIcon = new ImportGoodIcon(goodId, importableIcon, nonImportableIcon);
      _tooltipRegistrar.Register(image, () => GetTooltip(importGoodIcon, goodId, describedGood.DisplayName));
      return importGoodIcon;
    }

    private IEnumerable<ImportGoodIcon> CreateImportGoodsGroup(VisualElement parent, GoodGroupSpecification groupSpecification)
    {
      VisualElement visualElement = _visualElementLoader.LoadVisualElement("Game/EntityPanel/ImportGoodsGroup");
      visualElement.Q<Image>("Icon").sprite = groupSpecification.Icon;
      parent.Add(visualElement);
      VisualElement iconsParent = visualElement.Q<VisualElement>("Items");
      foreach (string goodId in _goodService.GetGoodsForGroup(groupSpecification.Id))
        yield return CreateImportGoodIcon(iconsParent, goodId);
    }

    private VisualElement GetTooltip(ImportGoodIcon importGoodIcon, string goodId, string goodDisplayName)
    {
      VisualElement e = _visualElementLoader.LoadVisualElement("Game/ImportGoodIconTooltip");
      e.Q<Label>("GoodLabel").text = goodDisplayName;
      GoodsStationDistributableGoodProvider distributableGoodProvider = importGoodIcon.GoodsStationDistributableGoodProvider;
      bool flag = distributableGoodProvider.IsImportEnabled(goodId);
      ImportOption goodImportOption = distributableGoodProvider.GetGoodImportOption(goodId);
      e.Q<VisualElement>("DisabledInfo").ToggleDisplayStyle(goodImportOption == ImportOption.Disabled);
      e.Q<VisualElement>("ForcedInfo").ToggleDisplayStyle(goodImportOption == ImportOption.Forced);
      e.Q<VisualElement>("ImportableInfo").ToggleDisplayStyle(goodImportOption == ImportOption.Auto & flag);
      e.Q<VisualElement>("NonImportableInfo").ToggleDisplayStyle(goodImportOption == ImportOption.Auto && !flag);
      return e;
    }
  }
}
