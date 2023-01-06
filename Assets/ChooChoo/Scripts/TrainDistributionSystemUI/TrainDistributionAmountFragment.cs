using TimberApi.UiBuilderSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ChooChoo
{
  internal class TrainDistributionAmountFragment : IEntityPanelFragment
  {
    private readonly UIBuilder _uiBuilder;
    private readonly ILoc _loc;
    private DistributableGoodBringerTrain _distributableGoodBringerTrain;
    private VisualElement _root;
    private TextField _distributionAmountInput;

    public TrainDistributionAmountFragment(
      UIBuilder uiBuilder, 
      ILoc loc)
    {
      _uiBuilder = uiBuilder;
      _loc = loc;
    }

    public VisualElement InitializeFragment()
    {
      _root = _uiBuilder.CreateFragmentBuilder()
        .AddComponent(builder => builder
          .SetFlexDirection(FlexDirection.Row)
          .SetMargin(new Margin(new Length(5, LengthUnit.Pixel)))

          .AddPreset(builder =>
          {
            var label = builder.Labels().GameText("Tobbert.DistributionTrain.MinimumOfItemsToMove");
            label.style.marginRight = new Length(5, LengthUnit.Pixel);
            return label;
          })

          .AddPreset(builder => builder.TextFields().InGameTextField(50, name: "DistributionAmountInput")))
        .BuildAndInitialize();

      _distributionAmountInput = _root.Q<TextField>("DistributionAmountInput");
            
      TextFields.InitializeIntTextField(_distributionAmountInput, 0, midEditingCallback: value => _distributableGoodBringerTrain.MinimumOfItemsToMove = value - 1);

      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      _distributableGoodBringerTrain = entity.GetComponent<DistributableGoodBringerTrain>();
      if (!(bool)(Object)_distributableGoodBringerTrain) 
        return;
      _root.ToggleDisplayStyle(true);
      _distributionAmountInput.value = (_distributableGoodBringerTrain.MinimumOfItemsToMove + 1).ToString();
    }

    public void ClearFragment()
    {
      _root.ToggleDisplayStyle(false);
      _distributableGoodBringerTrain = null;
    }

    public void UpdateFragment()
    {
      
    }
  }
}
