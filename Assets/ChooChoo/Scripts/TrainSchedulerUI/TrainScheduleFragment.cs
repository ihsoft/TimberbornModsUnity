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
  internal class TrainScheduleFragment : IEntityPanelFragment
  {
    private readonly EventBus _eventBus;
    private readonly UIBuilder _builder;
    private readonly TrainScheduleSectionFactory _trainScheduleSectionFactory;
    private readonly ILoc _loc;
    private readonly NewStationButton _newStationButton;
    private VisualElement _root;
    private VisualElement _content;
    private TrainScheduleController _trainScheduleController;

    public TrainScheduleFragment(
      EventBus eventBus,
      UIBuilder builder,
      TrainScheduleSectionFactory trainScheduleSectionFactory,
      ILoc loc,
      NewStationButton newStationButton)
    {
      _eventBus = eventBus;
      _builder = builder;
      _trainScheduleSectionFactory = trainScheduleSectionFactory;
      _loc = loc;
      _newStationButton = newStationButton;
    }

    public VisualElement InitializeFragment()
    {
      _eventBus.Register(this);
      
      _root = _builder.CreateFragmentBuilder()
        .AddComponent(builder => builder
          .AddPreset(builder =>
          {
            var scrollView = builder.ScrollViews().MainScrollView();
            scrollView.name = "Content";
            scrollView.style.height = new Length(Screen.height * 0.4f, LengthUnit.Pixel);
            scrollView.style.flexDirection = FlexDirection.Column;
            return scrollView;
          }))
        
        .AddComponent(builder => builder
          .SetWidth(new Length(100, LengthUnit.Percent))
          .SetJustifyContent(Justify.Center)
          
          .AddPreset(builder =>
            {
              var button = builder.Buttons().Button();
              button.name = "NewStationButton";
              button.style.width = new Length(60, LengthUnit.Percent);
              return button;
            }))
        
        .BuildAndInitialize();
      
      var button = _root.Q<Button>("NewStationButton");
      _newStationButton.Initialize(button, () => _trainScheduleController, RefreshFragment);

      _content = _root.Q<VisualElement>("Content");
      
      _root.ToggleDisplayStyle(false);
      return _root;
    }

    public void ShowFragment(GameObject entity)
    {
      var component = entity.GetComponent<TrainScheduleController>();
      if (component == null)
        return;
      _trainScheduleController = component;
      _trainScheduleSectionFactory.CreateTrainScheduleSections(_trainScheduleController, _content);
    }

    public void ClearFragment()
    {
      _root.ToggleDisplayStyle(false);
      _content.Clear();
      _newStationButton.StopRouteAddition();
      _trainScheduleController = null;
    }

    public void UpdateFragment()
    {
      if ((bool) (Object) _trainScheduleController && _trainScheduleController.enabled)
      {
        _root.ToggleDisplayStyle(true);
      }
      else
        _root.ToggleDisplayStyle(false);
    }

    [OnEvent]
    public void OnScheduleUpdate(OnScheduleUpdatedEvent onScheduleUpdatedEvent)
    {
      RefreshFragment();
    }
    
    private void RefreshFragment()
    {
      _content.Clear();
      _trainScheduleSectionFactory.CreateTrainScheduleSections(_trainScheduleController, _content);
    }
  }
}
