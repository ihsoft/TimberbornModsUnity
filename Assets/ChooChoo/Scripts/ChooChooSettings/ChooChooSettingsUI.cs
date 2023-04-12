using System.Linq;
using TimberApi.UiBuilderSystem;
using Timberborn.CoreUI;
using Timberborn.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
    public class ChooChooSettingsUI
    {
        private readonly DropdownOptionsSetter _dropdownOptionsSetter;
        
        private readonly VisualElementLoader _visualElementLoader;

        private readonly DropdownListDrawer _dropdownListDrawer;
        
        private readonly ChooChooSettings _chooChooSettings;
        
        private readonly UIBuilder _builder;
        
        private readonly ILoc _loc;

        ChooChooSettingsUI(
            DropdownOptionsSetter dropdownOptionsSetter,
            VisualElementLoader visualElementLoader, 
            DropdownListDrawer dropdownListDrawer,
            ChooChooSettings chooChooSettings, 
            UIBuilder uiBuilder,
            ILoc loc)
        {
            _dropdownOptionsSetter = dropdownOptionsSetter;
            _visualElementLoader = visualElementLoader;
            _dropdownListDrawer = dropdownListDrawer;
            _chooChooSettings = chooChooSettings;
            _builder = uiBuilder;
            _loc = loc;
        }

        public void InitializeSelectorSettings(ref VisualElement root)
        {
            var container = _builder.CreateComponentBuilder().CreateVisualElement()
                .SetWidth(new Length(100, LengthUnit.Percent))
                .SetJustifyContent(Justify.Center)
                .SetAlignContent(Align.Center)
                .SetAlignItems(Align.Center)
                .BuildAndInitialize();
            
            var customPathsHeader = _builder.CreateComponentBuilder()
                .CreateVisualElement()
                .AddPreset(factory =>
                {
                    var test = factory.Labels().DefaultHeader();
                    test.TextLocKey = "Tobbert.ChooChooSettings.Header";
                    test.style.fontSize = new Length(16, LengthUnit.Pixel);
                    test.style.unityFontStyleAndWeight = FontStyle.Bold;
                    return test;
                })
                .BuildAndInitialize();

            var trainModelSelector = _builder.CreateComponentBuilder()
                .CreateVisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetWidth(new Length(100, LengthUnit.Percent))
                .SetHeight(new Length(60, LengthUnit.Pixel))
                .SetJustifyContent(Justify.Center)
                .SetAlignItems(Align.Center)
                .AddPreset(builder =>
                {
                    var label = builder.Labels().DefaultText();
                    label.TextLocKey = "Tobbert.ChooChooSettings.DefaultTrainModel";
                    return label;
                })
                .AddPreset(_ =>
                {
                    var fragment = _visualElementLoader.LoadVisualElement("Options/SettingsBox");
                    var dropDown = fragment.Q<Dropdown>("ScreenResolution");
                    _dropdownOptionsSetter.SetLocalizableOptions(
                        dropDown,
                        new[] { "Tobbert.TrainModel.BigWooden", "Tobbert.TrainModel.SmallLog" },
                        () => _chooChooSettings.DefaultModelSettings.DefaultTrainModel,
                        OnTrainSettingChanged);
                    dropDown.Initialize(_dropdownListDrawer);
                    dropDown.Q<Label>("Label").ToggleDisplayStyle(false);
                    return dropDown;
                })
                .BuildAndInitialize();
            
            var wagonModelSelector = _builder.CreateComponentBuilder()
                .CreateVisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetWidth(new Length(100, LengthUnit.Percent))
                .SetHeight(new Length(60, LengthUnit.Pixel))
                .SetJustifyContent(Justify.Center)
                .SetAlignItems(Align.Center)
                .AddPreset(builder =>
                {
                    var label = builder.Labels().DefaultText();
                    label.TextLocKey = "Tobbert.ChooChooSettings.DefaultWagonModel";
                    return label;
                })
                .AddPreset(_ =>
                {
                    var fragment = _visualElementLoader.LoadVisualElement("Options/SettingsBox");
                    var dropDown = fragment.Q<Dropdown>("ScreenResolution");
                    _dropdownOptionsSetter.SetLocalizableOptions(
                        dropDown,
                        new[] { "Tobbert.WagonModel.BoxWagon", "Tobbert.WagonModel.TankWagon", "Tobbert.WagonModel.FlatWagon", "Tobbert.WagonModel.FlipperWagon", "Tobbert.WagonModel.MetalCart" },
                        () => _chooChooSettings.DefaultModelSettings.DefaultWagonModel,
                        OnWagonSettingChanged);
                    dropDown.Initialize(_dropdownListDrawer);
                    dropDown.Q<Label>("Label").ToggleDisplayStyle(false);
                    return dropDown;
                })
                .BuildAndInitialize();

            container.Add(customPathsHeader);
            container.Add(trainModelSelector);
            container.Add(wagonModelSelector);

            var toggle = root.Q<Toggle>("AutoSavingOn");
            toggle.parent.Add(container);
        }

        private void OnTrainSettingChanged(string value)
        {
            _chooChooSettings.ChangeTrainModelSetting(value);
        }
        
        private void OnWagonSettingChanged(string value)
        {
            _chooChooSettings.ChangeWagonModelSetting(value);
        }
    }
}