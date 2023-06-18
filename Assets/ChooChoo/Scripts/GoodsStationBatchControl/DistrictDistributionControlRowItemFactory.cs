using Timberborn.BatchControl;
using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.TooltipSystem;
using UnityEngine.UIElements;

namespace ChooChoo
{
  internal class DistrictDistributionControlRowItemFactory
  {
    private static readonly string ResetLocKey = "Distribution.Reset";
    private static readonly string ImportDisabledAllLocKey = "Distribution.ImportDisabledAll";
    private static readonly string ImportForcedAllLocKey = "Distribution.ImportForcedAll";
    private readonly ITooltipRegistrar _tooltipRegistrar;
    private readonly VisualElementLoader _visualElementLoader;

    public DistrictDistributionControlRowItemFactory(
      ITooltipRegistrar tooltipRegistrar,
      VisualElementLoader visualElementLoader)
    {
      _tooltipRegistrar = tooltipRegistrar;
      _visualElementLoader = visualElementLoader;
    }

    public IBatchControlRowItem Create(GoodsStationDistributionSetting goodsStationDistributionSetting)
    {
      VisualElement visualElement = _visualElementLoader.LoadVisualElement("Game/BatchControl/DistrictDistributionControlRowItem");
      Button button1 = visualElement.Q<Button>("Reset");
      button1.RegisterCallback((EventCallback<ClickEvent>) (_ => goodsStationDistributionSetting.ResetToDefault()));
      _tooltipRegistrar.RegisterLocalizable(button1, ResetLocKey);
      Button button2 = visualElement.Q<Button>("ExportAll");
      button2.ToggleDisplayStyle(false);
      Button button3 = visualElement.Q<Button>("ExportNone");
      button3.ToggleDisplayStyle(false);
      Button button4 = visualElement.Q<Button>("ImportDisabledAll");
      button4.RegisterCallback((EventCallback<ClickEvent>) (_ => goodsStationDistributionSetting.SetDistrictImportOption(ImportOption.Disabled)));
      _tooltipRegistrar.RegisterLocalizable(button4, ImportDisabledAllLocKey);
      Button button5 = visualElement.Q<Button>("ImportAutoAll");
      button5.ToggleDisplayStyle(false);
      Button button6 = visualElement.Q<Button>("ImportForcedAll");
      button6.RegisterCallback((EventCallback<ClickEvent>) (_ => goodsStationDistributionSetting.SetDistrictImportOption(ImportOption.Forced)));
      _tooltipRegistrar.RegisterLocalizable(button6, ImportForcedAllLocKey);
      return new EmptyBatchControlRowItem(visualElement);
    }
  }
}
