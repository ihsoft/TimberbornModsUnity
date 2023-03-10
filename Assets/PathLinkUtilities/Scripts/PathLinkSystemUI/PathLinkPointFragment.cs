using System;
using TimberApi;
using TimberApi.UiBuilderSystem;
using TimberApi.UiBuilderSystem.ElementSystem;
using TimberApi.UiBuilderSystem.PresetSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace PathLinkUtilities
{
  internal class PathLinkPointFragment : IEntityPanelFragment
  {
    private readonly EventBus _eventBus;
    private readonly UIBuilder _builder;
    private readonly ILoc _loc;
    private readonly ConnectPathLinkPointButton _newStationButton;
    private readonly PathLinkRepository _pathLinkRepository;
    private VisualElement _root;
    private VisualElement _content;
    private PathLinkPoint _pathLinkPoint;

    public PathLinkPointFragment(
      EventBus eventBus,
      UIBuilder builder,
      ILoc loc,
      ConnectPathLinkPointButton newStationButton,
      PathLinkRepository pathLinkRepository)
    {
      _eventBus = eventBus;
      _builder = builder;
      _loc = loc;
      _newStationButton = newStationButton;
      _pathLinkRepository = pathLinkRepository;
    }

    public VisualElement InitializeFragment()
    {
      _eventBus.Register(this);
      _root = _builder.CreateFragmentBuilder()
        .AddComponent(builder => builder
          .SetWidth(new Length(100f, LengthUnit.Percent))
          .SetJustifyContent(Justify.Center)
          .AddPreset(builder =>
          {
            var localizableButton = builder.Buttons().ButtonGame();
            localizableButton.name = "PathLinkPointButton";
            localizableButton.style.width = new Length(60f, LengthUnit.Percent);
            return localizableButton;
          }))
        .AddComponent((Action<VisualElementBuilder>) (builder => builder.SetWidth(new Length(100f, LengthUnit.Percent)).SetJustifyContent(Justify.Center).AddPreset((Func<UiPresetFactory, VisualElement>) (builder =>
      {
        var localizableButton = builder.Buttons().ButtonGame();
        localizableButton.name = "RemovePathLinksButton";
        localizableButton.TextLocKey = "Tobbert.PathLinkPoint.RemoveLinks";
        localizableButton.style.width = new Length(60f, LengthUnit.Percent);
        localizableButton.style.marginTop = new Length(5f, LengthUnit.Pixel);
        return localizableButton;
      })))).BuildAndInitialize();
      _newStationButton.Initialize(_root.Q<Button>("PathLinkPointButton"), () => _pathLinkPoint, RefreshFragment);
      _root.Q<Button>("RemovePathLinksButton").clicked += RemoveLinks;
      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      PathLinkPoint component = entity.GetComponent<PathLinkPoint>();
      if (!(component != null) || !component.UIEnabledEnabled)
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
      if ((bool) (UnityEngine.Object) _pathLinkPoint)
        _root.ToggleDisplayStyle(true);
      else
        _root.ToggleDisplayStyle(false);
    }

    private void RefreshFragment()
    {
    }

    private void RemoveLinks()
    {
      _pathLinkRepository.RemoveLinks(_pathLinkPoint);
      _eventBus.Post(new OnPathLinksUpdated());
    }
  }
}
