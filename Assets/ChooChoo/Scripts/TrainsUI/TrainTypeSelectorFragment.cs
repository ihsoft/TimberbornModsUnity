using System;
using System.Collections.Generic;
using System.Linq;
using TimberApi.UiBuilderSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
  internal class TrainTypeSelectorFragment : IEntityPanelFragment
  {
    private readonly UIBuilder _uiBuilder;
    private ILoc _loc;
    private readonly VisualElementLoader _visualElementLoader;
    private readonly TrainTypeDropdownOptionsSetter _trainTypeDropdownOptionsSetter;
    private TrainModelManager _trainModelManager;
    private Dropdown _dropdown;
    private VisualElement _root;
    private readonly int _numberOfWagons = 4;

    public TrainTypeSelectorFragment(
      UIBuilder uiBuilder,
      ILoc loc,
      VisualElementLoader visualElementLoader,
      TrainTypeDropdownOptionsSetter trainTypeDropdownOptionsSetter)
    {
      _uiBuilder = uiBuilder;
      _loc = loc;
      _visualElementLoader = visualElementLoader;
      _trainTypeDropdownOptionsSetter = trainTypeDropdownOptionsSetter;
    }

    public VisualElement InitializeFragment()
    {
      _root = _uiBuilder.CreateFragmentBuilder().BuildAndInitialize();
      
      var fragment = _visualElementLoader.LoadVisualElement("Master/EntityPanel/PlantablePrioritizerFragment");
      var container = new VisualElement();
      foreach (var element in fragment.Children().ToList())
      {
        container.Add(element);
        container.Q<Label>().text = _loc.T("Tobbert.TrainModel.TrainModel");
      }
      _root.Add(container);
      _dropdown = container.Q<Dropdown>("Priorities");
      

      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      _trainModelManager = entity.GetComponent<TrainModelManager>();
      if (!_trainModelManager)
        return;
      _trainTypeDropdownOptionsSetter.SetOptions(_trainModelManager, _dropdown);
    }

    public void ClearFragment()
    {
      _dropdown.ClearOptions();
      _trainModelManager = null;
      UpdateFragment();
    }

    public void UpdateFragment() => _root.ToggleDisplayStyle(_trainModelManager);
  }
}
