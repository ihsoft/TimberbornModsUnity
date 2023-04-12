using System;
using System.Collections.Generic;
using System.Linq;
using TimberApi.UiBuilderSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
  internal class WagonTypeSelectorFragment : IEntityPanelFragment
  {
    private readonly UIBuilder _uiBuilder;
    private readonly VisualElementLoader _visualElementLoader;
    private readonly WagonTypeDropdownOptionsSetter _wagonTypeDropdownOptionsSetter;
    private WagonManager _wagonManager;
    private readonly List<Dropdown> _dropdowns = new();
    private VisualElement _root;
    private readonly List<VisualElement> _wagonSections = new();
    private readonly int _numberOfWagons = 4;

    public WagonTypeSelectorFragment(
      UIBuilder uiBuilder,
      VisualElementLoader visualElementLoader,
      WagonTypeDropdownOptionsSetter wagonTypeDropdownOptionsSetter)
    {
      _uiBuilder = uiBuilder;
      _visualElementLoader = visualElementLoader;
      _wagonTypeDropdownOptionsSetter = wagonTypeDropdownOptionsSetter;
    }

    public VisualElement InitializeFragment()
    {
      _root = _uiBuilder.CreateFragmentBuilder().BuildAndInitialize();
      for (int i = 0; i < _numberOfWagons; i++)
      {
        var fragment = _visualElementLoader.LoadVisualElement("Master/EntityPanel/PlantablePrioritizerFragment");
        var container = new VisualElement();
        foreach (var element in fragment.Children().ToList())
        {
          container.Add(element);
          container.Q<Label>().text = "Wagon Type";
        }
        _root.Add(container);
        _wagonSections.Add(fragment);
        _dropdowns.Add(container.Q<Dropdown>("Priorities"));
      }

      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      _wagonManager = entity.GetComponent<WagonManager>();
      if (!_wagonManager)
        return;
      for (int i = 0; i < _numberOfWagons; i++)
      {
        _wagonTypeDropdownOptionsSetter.SetOptions(_wagonManager, _dropdowns[i], i);
        _wagonSections[i].ToggleDisplayStyle(true);
      }

      // _trainWagonManager.WagonTypesChanged += OnWagonTypeChanged;
    }

    public void ClearFragment()
    {
      // if (_trainWagonManager)
      //   _trainWagonManager.WagonTypesChanged -= OnWagonTypeChanged;
      for (int i = 0; i < _numberOfWagons; i++)
      {
        _wagonSections[i].ToggleDisplayStyle(false);
        _dropdowns[i].ClearOptions();
      }

      _wagonManager = null;
      UpdateFragment();
    }

    public void UpdateFragment() => _root.ToggleDisplayStyle(_wagonManager);

    private void OnWagonTypeChanged(object sender, EventArgs e) =>
      _dropdowns.ForEach(dropdown => dropdown.RefreshContent());
  }
}
