using TimberApi.UiBuilderSystem;
using Timberborn.BehaviorSystem;
using Timberborn.CoreUI;
using Timberborn.Persistence;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
    public class TrainActionWait : ITrainAction
    {
        private static readonly PropertyKey<int> WaitingTimeInMinutesKey = new("WaitingInMinutes");
        
        private UIBuilder _builder;

        private WaitExecutor _waitExecutor;

        private int _waitingTimeInMinutes;
            
        // public TrainDestination TrainDestination { get; set; }
        public string ActionNameLocKey { get; }
        public GameObject Train { get; private set; }
        public GameObject TrainStation { get; private set; }

        public TrainActionWait(
            UIBuilder uiBuilder
            // , TrainDestination trainDestination
            )
        {
            _builder = uiBuilder;
            // TrainDestination = trainDestination;
            ActionNameLocKey = "Tobbert.TrainAction.Wait";
        }


        public void SetTrain(GameObject train)
        {
            Train = train;
            
            _waitExecutor = train.GetComponent<WaitExecutor>();
        }

        public void SetTrainStation(GameObject trainStation)
        {
            TrainStation = trainStation;
        }

        public VisualElement GetElement()
        {
            VisualElement visualElement = _builder.CreateComponentBuilder().CreateVisualElement()

                .AddComponent(builder => builder
                    .SetFlexDirection(FlexDirection.Row)
                    .SetHeight(new Length(20, LengthUnit.Pixel))

                    .AddPreset(builder => builder.Labels().GameText("Tobbert.TrainAction.Wait.WaitingInMinutes"))

                    .AddPreset(builder => builder.TextFields().InGameTextField(50, name: "WaitingTimeField")))

                .BuildAndInitialize();

            var waitingTimeField = visualElement.Q<TextField>("WaitingTimeField");
            
            TextFields.InitializeIntTextField(waitingTimeField, _waitingTimeInMinutes, midEditingCallback: value => _waitingTimeInMinutes = value);
            
            return visualElement;
        }

        public Decision ExecuteAction()
        {
            _waitExecutor.LaunchForSpecifiedTime(_waitingTimeInMinutes / 60);
            
            return Decision.ReleaseWhenFinished(_waitExecutor);
        }

        public void Save(IObjectSaver objectSaver)
        {
            objectSaver.Set(WaitingTimeInMinutesKey, _waitingTimeInMinutes);
        }

        public void Load(IObjectLoader objectLoader)
        {
            _waitingTimeInMinutes = objectLoader.Get(WaitingTimeInMinutesKey);
        }
    }
}