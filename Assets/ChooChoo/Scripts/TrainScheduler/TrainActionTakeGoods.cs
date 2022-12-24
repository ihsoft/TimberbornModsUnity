using TimberApi.UiBuilderSystem;
using Timberborn.BehaviorSystem;
using Timberborn.CoreUI;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Persistence;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
    public class TrainActionTakeGoods : ITrainAction
    {
        private UIBuilder _builder;
        public string ActionNameLocKey { get; }
        public GameObject Train { get; private set; }
        public GameObject TrainStation { get; private set; }

        private Inventory _stationInventory;
        
        private GoodAmount _goodAmount = new("", 0);

        public TrainActionTakeGoods(UIBuilder uiBuilder)
        {
            _builder = uiBuilder;
            ActionNameLocKey = "Tobbert.TrainAction.TakeGoods";
        }

        public void SetTrain(GameObject train)
        {
            Train = train;
        }

        public void SetTrainStation(GameObject trainStation)
        {
            TrainStation = trainStation;

            _stationInventory = trainStation.GetComponent<Inventory>();
        }

        public VisualElement GetElement()
        {
            VisualElement visualElement = _builder.CreateComponentBuilder().CreateVisualElement()
                
                .AddComponent(builder => builder
                    .SetFlexDirection(FlexDirection.Row)
                    .SetHeight(new Length(20, LengthUnit.Pixel))
                    
                    .AddPreset(builder => builder.Labels().GameText("Tobbert.TrainAction.TakeGoods.TakeGoodAmount"))
                
                    .AddPreset(builder => builder.TextFields().InGameTextField(50, name: "GoodsAmount")))


                .BuildAndInitialize();

            var waitingTimeField = visualElement.Q<TextField>("GoodsAmount");

            TextFields.InitializeIntTextField(waitingTimeField, _goodAmount.Amount, midEditingCallback: value =>
            {
                var goodId = _goodAmount.GoodId;
                _goodAmount = new GoodAmount(goodId, value);
            });
            
            return visualElement;
        }

        public Decision ExecuteAction()
        {
            return Decision.ReleaseNow();
        }

        public void Save(IObjectSaver objectSaver)
        {
            
        }

        public void Load(IObjectLoader objectLoader)
        {
            
        }
    }
}