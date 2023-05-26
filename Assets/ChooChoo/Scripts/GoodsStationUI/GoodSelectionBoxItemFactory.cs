using System;
using Timberborn.CoreUI;
using Timberborn.ResourceCountingSystemUI;
using Timberborn.StockpilesUI;
using Timberborn.TooltipSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
  public class GoodSelectionBoxItemFactory
  {
    private readonly VisualElementLoader _visualElementLoader;
    private readonly StockpileOptionsService _stockpileOptionsService;
    private readonly ContextualResourceCountingService _contextualResourceCountingService;
    private readonly ITooltipRegistrar _tooltipRegistrar;

    public GoodSelectionBoxItemFactory(
      VisualElementLoader visualElementLoader,
      StockpileOptionsService stockpileOptionsService,
      ContextualResourceCountingService contextualResourceCountingService,
      ITooltipRegistrar tooltipRegistrar)
    {
      _visualElementLoader = visualElementLoader;
      _stockpileOptionsService = stockpileOptionsService;
      _contextualResourceCountingService = contextualResourceCountingService;
      _tooltipRegistrar = tooltipRegistrar;
    }

    public GoodSelectionBoxItem Create(string option, Action<string> itemAction)
    {
      VisualElement visualElement = _visualElementLoader.LoadVisualElement("Game/EntityPanel/StockpileInventoryFragmentItem");
      visualElement.Q<Button>("StockpileInventoryFragmentItem").clicked += () => itemAction(option);
      Sprite optionIcon = _stockpileOptionsService.GetItemIcon(option);
      visualElement.Q<Image>("Icon").sprite = optionIcon;
      _tooltipRegistrar.Register(visualElement, _stockpileOptionsService.GetItemDisplayText(option));
      return new GoodSelectionBoxItem(_contextualResourceCountingService, option, visualElement, visualElement.Q<VisualElement>("Fill"));
    }
  }
}
