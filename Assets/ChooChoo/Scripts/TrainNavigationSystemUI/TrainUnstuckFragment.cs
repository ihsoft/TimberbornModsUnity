using TimberApi.UiBuilderSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ChooChoo
{
  internal class TrainUnstuckFragment : IEntityPanelFragment
  {
    private readonly UIBuilder _uiBuilder;
    private readonly ILoc _loc;
    private Machinist _machinist;
    private VisualElement _root;
    private Button _button;

    public TrainUnstuckFragment(
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
          .SetWidth(new Length(100, LengthUnit.Percent))
          .SetJustifyContent(Justify.Center)

          .AddPreset(builder =>
          {
            var button = builder.Buttons().ButtonGame("Tobbert.TrainNavigation.Unstuck");
            button.name = "button";
            return button;
          }))
        .BuildAndInitialize();

      _button = _root.Q<Button>("button");

      _button.clicked += OnClick;

      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      _machinist = entity.GetComponent<Machinist>();
      if (!(bool)(Object)_machinist) 
        return;
      _root.ToggleDisplayStyle(true);
    }

    public void ClearFragment()
    {
      _root.ToggleDisplayStyle(false);
    }

    public void UpdateFragment()
    {
      
    }

    private void OnClick()
    {
      if (!(bool)(Object)_machinist) 
        return;
      _machinist.IsStuck = true;
    }
  }
}
