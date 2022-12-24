using TimberApi.UiBuilderSystem;
using Timberborn.Localization;
using Timberborn.PrefabSystem;
using Timberborn.SelectionSystem;
using UnityEngine.UIElements;

namespace ChooChoo
{
    public class TrainScheduleSectionFactory
    {
        private readonly UIBuilder _builder;

        private readonly TrainScheduleActionFactory _trainScheduleActionFactory;

        private readonly ILoc _loc;

        private readonly SelectionManager _selectionManager;

        private readonly int ButtonHeight = 24;
        
        TrainScheduleSectionFactory(UIBuilder builder,  TrainScheduleActionFactory trainScheduleActionFactory, ILoc loc, SelectionManager selectionManager)
        {
            _builder = builder;
            _trainScheduleActionFactory = trainScheduleActionFactory;
            _loc = loc;
            _selectionManager = selectionManager;
        }
        
        public void CreateTrainScheduleSections(TrainScheduleController trainScheduleController, VisualElement parent)
        {
            foreach (var stationActions in trainScheduleController.TrainSchedule)
                CreateTrainScheduleSection(trainScheduleController, stationActions, parent);
        }

        private void CreateTrainScheduleSection(TrainScheduleController trainScheduleController, StationActions stationActions, VisualElement parent)
        {
            VisualElement visualElement = _builder.CreateComponentBuilder()
                .CreateVisualElement()
                
                    .AddComponent(builder => builder
                        .SetFlexDirection(FlexDirection.Row)
                        .SetMargin(new Margin(8))
                        .AddPreset(builder =>
                        {
                            var button = builder.Buttons().ButtonGame();
                            button.name = "SectionButton";
                            button.style.height = new Length(ButtonHeight, LengthUnit.Pixel);
                            button.style.width = new Length(60, LengthUnit.Percent);
                            return button;
                        })

                        .AddPreset(builder =>
                        {
                            var button = builder.Buttons().Close();
                            button.name = "RemoveSectionButton";
                            button.style.height = new Length(ButtonHeight, LengthUnit.Pixel);
                            button.style.width = new Length(ButtonHeight, LengthUnit.Pixel);
                            return button;
                        }))
                    

                    .AddComponent(builder =>
                    {
                        builder.SetName("SectionActions");
                        builder.SetPadding(new Padding(0, 0, 0, new Length(40, LengthUnit.Pixel)));
                    })

                .BuildAndInitialize();
            
            parent.Add(visualElement);
            
            var sectionButton = visualElement.Q<Button>("SectionButton");
            sectionButton.text = _loc.T(stationActions.Station.GetComponent<LabeledPrefab>().DisplayNameLocKey);
            sectionButton.clicked += () => _selectionManager.FocusOn(stationActions.Station.gameObject);
            
            var removeSectionButton = visualElement.Q<Button>("RemoveSectionButton");
            removeSectionButton.clicked += () => trainScheduleController.RemoveStation(stationActions);;
            
            var sectionActions = visualElement.Q<VisualElement>("SectionActions");
            _trainScheduleActionFactory.CreateTrainScheduleActions(trainScheduleController, stationActions, sectionActions);
        }
    }
}