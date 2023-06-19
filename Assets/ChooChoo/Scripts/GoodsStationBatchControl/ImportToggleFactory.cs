using Timberborn.CoreUI;
using Timberborn.DistributionSystem;
using Timberborn.Localization;
using Timberborn.SliderToggleSystem;
using UnityEngine.UIElements;

namespace ChooChoo
{
  internal class ImportToggleFactory
  {
    private static readonly string ImportDisabledIconClass = "import-icon--disabled";
    private static readonly string ImportForcedIconClass = "import-icon--forced";
    private static readonly string ImportDisabledBackgroundClass = "import-background--disabled";
    private static readonly string ImportForcedBackgroundClass = "import-background--forced";
    private static readonly string ImportDisabledLocKey = "Tobbert.BatchControl.ImportDisabled";
    private static readonly string ImportDisabledDescriptionLocKey = "Tobbert.BatchControl.ImportDisabled.Description";
    private static readonly string ImportForcedLocKey = "Tobbert.BatchControl.ImportForced";
    private static readonly string ImportForcedDescriptionLocKey = "Tobbert.BatchControl.ImportForced.Description";
    private readonly ILoc _loc;
    private readonly SliderToggleFactory _sliderToggleFactory;
    private readonly VisualElementLoader _visualElementLoader;

    public ImportToggleFactory(
      ILoc loc,
      SliderToggleFactory sliderToggleFactory,
      VisualElementLoader visualElementLoader)
    {
      _loc = loc;
      _sliderToggleFactory = sliderToggleFactory;
      _visualElementLoader = visualElementLoader;
    }

    public SliderToggle Create(VisualElement parent, GoodsStationGoodDistributionSetting setting)
    {
      SliderToggleItem sliderToggleItem1 = SliderToggleItem.Create(
        GetImportDisabledTooltip, 
        ImportDisabledIconClass, 
        ImportDisabledBackgroundClass, 
        () => {
          setting.SetImportOption(ImportOption.Disabled);
          setting.SetMaxCapacity(0);
        }, 
        () => setting.ImportOption == ImportOption.Disabled);
      SliderToggleItem sliderToggleItem2 = SliderToggleItem.Create(GetImportForcedTooltip, ImportForcedIconClass, ImportForcedBackgroundClass, () => setting.SetImportOption(ImportOption.Forced), () => setting.ImportOption == ImportOption.Forced);
      return _sliderToggleFactory.Create(parent, sliderToggleItem1, sliderToggleItem2);
    }

    private VisualElement GetImportDisabledTooltip() => GetTooltip(ImportDisabledLocKey, ImportDisabledDescriptionLocKey);

    private VisualElement GetImportForcedTooltip() => GetTooltip(ImportForcedLocKey, ImportForcedDescriptionLocKey);

    private VisualElement GetTooltip(string title, string description)
    {
      VisualElement e = _visualElementLoader.LoadVisualElement("Game/ImportToggleTooltip");
      e.Q<Label>("Title").text = _loc.T(title);
      e.Q<Label>("Description").text = _loc.T(description);
      return e;
    }
  }
}
