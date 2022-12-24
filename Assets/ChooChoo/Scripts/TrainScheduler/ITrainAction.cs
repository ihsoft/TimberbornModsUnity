using Timberborn.BehaviorSystem;
using Timberborn.Persistence;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChooChoo
{
    public interface ITrainAction
    {
        string ActionNameLocKey { get; }
        GameObject Train { get; }
        GameObject TrainStation { get; }
        public void SetTrain(GameObject train);
        public void SetTrainStation(GameObject trainStation);
        public VisualElement GetElement();
        public Decision ExecuteAction();
        public void Save(IObjectSaver objectSaver);
        public void Load(IObjectLoader objectLoader);
    }
}