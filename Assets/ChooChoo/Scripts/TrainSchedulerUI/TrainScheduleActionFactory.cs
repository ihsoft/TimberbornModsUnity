using System;
using TimberApi.UiBuilderSystem;
using Timberborn.Localization;
using UnityEngine.UIElements;

namespace ChooChoo
{
    public class TrainScheduleActionFactory
    {
        private readonly UIBuilder _builder;

        private readonly ILoc _loc;
        
        private readonly int ButtonHeight = 21;
        
        TrainScheduleActionFactory(UIBuilder builder, ILoc loc)
        {
            _builder = builder;
            _loc = loc;
        }
        
        public void CreateTrainScheduleActions(TrainScheduleController trainScheduleController, StationActions stationActions, VisualElement parent)
        {
            foreach (var trainAction in stationActions.Actions)
                CreateTrainScheduleAction(trainScheduleController, trainAction, parent, stationActions);
            
            CreateNewActionButton(trainScheduleController, stationActions, parent);
        }

        private void CreateTrainScheduleAction(TrainScheduleController trainScheduleController, ITrainAction trainAction, VisualElement parent, StationActions stationActions)
        {
            VisualElement visualElement = _builder.CreateComponentBuilder().CreateVisualElement()
                .AddComponent(builder => builder
                    .SetFlexDirection(FlexDirection.Row)
                    .SetMargin(new Margin(4))
                    
                    .AddPreset(builder =>
                    {
                        var button = builder.Buttons().ButtonGame();
                        button.name = "TrainAction";
                        button.style.height = new Length(ButtonHeight, LengthUnit.Pixel);
                        return button;
                    })
                    
                    .AddPreset(builder =>
                    {
                        var button = builder.Buttons().Close();
                        button.name = "RemoveActionButton";
                        button.style.height = new Length(ButtonHeight, LengthUnit.Pixel);
                        button.style.width = new Length(ButtonHeight, LengthUnit.Pixel);
                        return button;
                    }))
                
                .AddComponent(builder => builder.SetName("ActionContent").SetMargin(new Margin(2)))
                
                .BuildAndInitialize();
            
            parent.Add(visualElement);

            var button = visualElement.Q<Button>("TrainAction");
            button.text = _loc.T(trainAction.ActionNameLocKey);
            button.clicked += () => trainScheduleController.CycleAction(stationActions, trainAction);

            var removeSectionButton = visualElement.Q<Button>("RemoveActionButton");
            removeSectionButton.clicked += () => trainScheduleController.RemoveAction(stationActions, trainAction);

            var actionContent = visualElement.Q<VisualElement>("ActionContent");
            actionContent.Add(trainAction.GetElement());
        }

        private void CreateNewActionButton(TrainScheduleController trainScheduleController, StationActions stationActions, VisualElement parent)
        {
            VisualElement visualElement = _builder.CreateComponentBuilder().CreateVisualElement()
                .SetMargin(new Margin(2))
                .AddPreset(builder =>
                {
                    var button = builder.Buttons().ButtonGame();
                    button.name = "AddNewTrainAction";
                    button.text = _loc.T("Tobbert.TrainSchedule.AddNewTrainAction");
                    button.style.height = new Length(ButtonHeight, LengthUnit.Pixel);
                    return button;
                })
                .BuildAndInitialize();
            
            parent.Add(visualElement);

            var button = visualElement.Q<Button>("AddNewTrainAction");
            button.clicked += () => trainScheduleController.AddNewAction(stationActions);
        }
    }
}