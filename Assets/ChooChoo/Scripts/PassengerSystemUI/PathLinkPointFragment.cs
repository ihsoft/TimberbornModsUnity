using TimberApi.UiBuilderSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ChooChoo
{
  internal class PathLinkPointFragment : IEntityPanelFragment
  {
    private readonly EventBus _eventBus;
    private readonly UIBuilder _builder;
    private readonly ILoc _loc;
    private readonly ConnectPathLinkPointButton _newStationButton;
    private VisualElement _root;
    private VisualElement _content;
    private PathLinkPoint _pathLinkPoint;

    public PathLinkPointFragment(
      EventBus eventBus,
      UIBuilder builder,
      ILoc loc,
      ConnectPathLinkPointButton newStationButton)
    {
      _eventBus = eventBus;
      _builder = builder;
      _loc = loc;
      _newStationButton = newStationButton;
    }

    public VisualElement InitializeFragment()
    {
      _root = _builder.CreateFragmentBuilder()
        .AddComponent(builder => builder
          .SetWidth(new Length(100, LengthUnit.Percent))
          .SetJustifyContent(Justify.Center)
          
          .AddPreset(builder =>
            {
              var button = builder.Buttons().Button();
              button.name = "PathLinkPointButton";
              button.style.width = new Length(60, LengthUnit.Percent);
              return button;
            }))
        
        .BuildAndInitialize();
      
      var button = _root.Q<Button>("PathLinkPointButton");
      _newStationButton.Initialize(button, () => _pathLinkPoint, RefreshFragment);

      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      var component = entity.GetComponent<PathLinkPoint>();
      if (component == null)
        return;
      _pathLinkPoint = component;
    }

    public void ClearFragment()
    {
      _root.ToggleDisplayStyle(false);
      _newStationButton.StopRouteAddition();
      _pathLinkPoint = null;
    }

    public void UpdateFragment()
    {
      if ((bool) (Object) _pathLinkPoint)
      {
        _root.ToggleDisplayStyle(true);
      }
      else
        _root.ToggleDisplayStyle(false);
    }

    private void RefreshFragment()
    {
    }
  }
}
