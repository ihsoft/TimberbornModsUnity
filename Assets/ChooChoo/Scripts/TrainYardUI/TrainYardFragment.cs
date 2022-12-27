using TimberApi.UiBuilderSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
  internal class TrainYardFragment : IEntityPanelFragment
  {
    private readonly VisualElementLoader _visualElementLoader;
    private readonly UIBuilder _uiBuilder;
    private TrainYard _trainYard;
    private VisualElement _root;

    public TrainYardFragment(VisualElementLoader visualElementLoader, UIBuilder uiBuilder)
    {
      _visualElementLoader = visualElementLoader;
      _uiBuilder = uiBuilder;
    }

    public VisualElement InitializeFragment()
    {
      _root = _uiBuilder.CreateFragmentBuilder()
        .AddPreset(builder =>
        {
          var button = builder.Buttons().Button();
          button.name = "Button";
          button.text = "Create train";
          
          return button;
        })
        
        
        .BuildAndInitialize();
      
      
      _root.Q<Button>("Button").clicked += () => _trainYard.InitializeTrain();
      
      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      _trainYard = entity.GetComponent<TrainYard>();
      if ((bool)(Object)_trainYard)
      {
        
      }
      else
        _trainYard = null;
    }

    public void ClearFragment()
    {
      _trainYard = null;
      _root.ToggleDisplayStyle(false);
    }

    public void UpdateFragment()
    {
      if ((bool) (Object) _trainYard)
      {
        _root.ToggleDisplayStyle(true);
      }
      else
        _root.ToggleDisplayStyle(false);
    }
  }
}
