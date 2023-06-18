using System.Linq;
using Timberborn.CoreUI;
using Timberborn.InputSystem;
using Timberborn.SliderToggleSystem;
using Timberborn.TooltipSystem;
using Timberborn.UIFormatters;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using ProgressBar = Timberborn.CoreUI.ProgressBar;
using Slider = UnityEngine.UIElements.Slider;
using Toggle = UnityEngine.UIElements.Toggle;

namespace ChooChoo
{
  internal class GoodDistributionSettingItemFactory
  {
    private readonly ImportGoodIconFactory _importGoodIconFactory;
    private readonly ImportToggleFactory _importToggleFactory;
    private readonly ITooltipRegistrar _tooltipRegistrar;
    private readonly VisualElementLoader _visualElementLoader;
    private readonly InputService _inputService;

    public GoodDistributionSettingItemFactory(
      ImportGoodIconFactory importGoodIconFactory,
      ImportToggleFactory importToggleFactory,
      ITooltipRegistrar tooltipRegistrar,
      VisualElementLoader visualElementLoader,
      InputService inputService)
    {
      _importGoodIconFactory = importGoodIconFactory;
      _importToggleFactory = importToggleFactory;
      _tooltipRegistrar = tooltipRegistrar;
      _visualElementLoader = visualElementLoader;
      _inputService = inputService;
    }

    public GoodDistributionSettingItem Create(GoodsStationDistributableGoodProvider goodsStationDistributableGoodProvider, GoodsStationGoodDistributionSetting goodsStationGoodDistributionSetting)
    {
      VisualElement visualElement = _visualElementLoader.LoadVisualElement("Game/BatchControl/GoodDistributionSettingItem");
      visualElement.style.justifyContent = Justify.SpaceBetween;
      ImportGoodIcon importGoodIcon = _importGoodIconFactory.CreateImportGoodIcon(visualElement.Q<VisualElement>("ImportGoodIconWrapper"), goodsStationGoodDistributionSetting.GoodId);
      importGoodIcon.SetDistrictDistributableGoodProvider(goodsStationDistributableGoodProvider);
      var slider = visualElement.Q<Slider>("ExportThresholdSlider");
      slider.parent.ToggleDisplayStyle(false);
      
      
      VisualElement visualElement1 = _visualElementLoader.LoadVisualElement("Game/BatchControl/PopulationDistributorBatchControlRowItem");
      TextField textField = visualElement1.Q<TextField>("MinimumValue");
      var plusButton = visualElement1.Q<Button>("PlusButton");
      var minusButton = visualElement1.Q<Button>("MinusButton");
      InitializeMinimumControls(textField, plusButton, minusButton, visualElement1, goodsStationGoodDistributionSetting);
      var buttonsParent = textField.parent;
      var minimum = buttonsParent.Children().First();
      minimum.ToggleDisplayStyle(false);
      var warning = buttonsParent.Children().Last();
      warning.ToggleDisplayStyle(false);
      slider.parent.parent.Add(buttonsParent);
      
      
      SliderToggle importToggle = _importToggleFactory.Create(visualElement.Q<VisualElement>("ImportToggleWrapper"), goodsStationGoodDistributionSetting);
      visualElement.Q<ProgressBar>("FillRateProgressBar").ToggleDisplayStyle(false);
      var goodDistributionSettingItem = new GoodDistributionSettingItem(_inputService, visualElement, goodsStationGoodDistributionSetting, importGoodIcon, importToggle, textField, plusButton, minusButton);
      goodDistributionSettingItem .Initialize();
      return goodDistributionSettingItem ;
    }

    private void InitializeMinimumControls(TextField minimumValue, Button plusButton, Button minusButton, VisualElement root, GoodsStationGoodDistributionSetting goodsStationGoodDistributionSetting)
    {
      TextFields.InitializeIntTextField(minimumValue, goodsStationGoodDistributionSetting.MaxCapacity, afterEditingCallback: newValue => OnIntFieldChange(newValue, goodsStationGoodDistributionSetting));
      minusButton.RegisterCallback<ClickEvent>(_ => OnButtonClicked(-1, goodsStationGoodDistributionSetting));
      plusButton.RegisterCallback<ClickEvent>(_ => OnButtonClicked(1, goodsStationGoodDistributionSetting));
      Toggle immigrationToggle = root.Q<Toggle>("ImmigrationToggle");
      immigrationToggle.ToggleDisplayStyle(false);
      Toggle emigrationToggle = root.Q<Toggle>("EmigrationToggle");
      emigrationToggle.ToggleDisplayStyle(false);
    }

    private void OnIntFieldChange(int newValue, GoodsStationGoodDistributionSetting goodsStationGoodDistributionSetting)
    {
      goodsStationGoodDistributionSetting.SetMaxCapacity(newValue);
    }

    private void OnButtonClicked(int change, GoodsStationGoodDistributionSetting goodsStationGoodDistributionSetting)
    {
      if (_inputService.IsShiftHeld)
        change *= 10;
      int minimum = goodsStationGoodDistributionSetting.MaxCapacity + change;
      goodsStationGoodDistributionSetting.SetMaxCapacity(minimum);
    }
  }
}
