using System;
using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using TimberApi.UiBuilderSystem;
using Timberborn.EntitySystem;
using Timberborn.Persistence;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainScheduleController : MonoBehaviour, IPersistentEntity, IDeletableEntity
    {
        private static readonly ComponentKey TrainScheduleControllerKey = new(nameof(TrainScheduleController));
        
        private static readonly ListKey<StationActions> TrainScheduleKey = new("TrainSchedule");

        private UIBuilder _builder;

        private EventBus _eventBus;

        private TrainScheduleObjectSerializer _trainScheduleObjectSerializer;
        
        private List<Type> _actions;
        
        private List<StationActions> _trainSchedule = new();

        public List<StationActions> TrainSchedule => _trainSchedule;

        [Inject]
        public void InjectDependencies(UIBuilder uiBuilder, EventBus eventBus,
            TrainScheduleObjectSerializer trainScheduleObjectSerializer
            )
        {
            _builder = uiBuilder;
            _eventBus = eventBus;
            _trainScheduleObjectSerializer = trainScheduleObjectSerializer;
        }
        
        private void Start()
        {
            _eventBus.Register(this);
            var type = typeof(ITrainAction);
            _actions = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(p => type.IsAssignableFrom(p) && !p.IsInterface).ToList();
        }

        public void Save(IEntitySaver entitySaver)
        {
            IObjectSaver component = entitySaver.GetComponent(TrainScheduleControllerKey);
            if (_trainSchedule.Count != 0)
                component.Set(TrainScheduleKey, _trainSchedule, _trainScheduleObjectSerializer);
        }
        
        public void Load(IEntityLoader entityLoader)
        {
            IObjectLoader component = entityLoader.GetComponent(TrainScheduleControllerKey);
            if (component.Has(TrainScheduleKey))
                _trainSchedule = component.Get(TrainScheduleKey, _trainScheduleObjectSerializer);
        }

        public void DeleteEntity()
        {
            _eventBus.Unregister(this);
        }
        
        public void AddStation(TrainDestination station)
        {
            _trainSchedule.Add(new StationActions(station, new List<ITrainAction>()));
            
            _eventBus.Post(new OnScheduleUpdatedEvent());
        }

        public void RemoveStation(StationActions stationActions)
        {
            _trainSchedule.Remove(stationActions);
            
            _eventBus.Post(new OnScheduleUpdatedEvent());
        }

        public void CycleAction(StationActions stationActions, ITrainAction oldAction)
        {
            var actionIndex = _actions.IndexOf(oldAction.GetType());            

            ITrainAction newAction;
            var args = new object[] { _builder };
            
            if (actionIndex + 1 < _actions.Count)
                newAction = Activator.CreateInstance(_actions[actionIndex + 1], args) as ITrainAction;
            else
                newAction = Activator.CreateInstance(_actions[0], args) as ITrainAction;

            var index = stationActions.Actions.IndexOf(oldAction);
            stationActions.Actions.Remove(oldAction);
            stationActions.Actions.Insert(index, newAction);

            newAction.SetTrain(gameObject);
            newAction.SetTrainStation(stationActions.Station.gameObject);
            
            _eventBus.Post(new OnScheduleUpdatedEvent());
        } 
        
        public void AddNewAction(StationActions stationActions)
        {
            var newAction = new TrainActionWait(_builder);
            newAction.SetTrain(gameObject);
            newAction.SetTrainStation(stationActions.Station.gameObject);
            
            stationActions.Actions.Add(newAction);
            
            _eventBus.Post(new OnScheduleUpdatedEvent());
        }
        
        public void RemoveAction(StationActions stationActions, ITrainAction trainAction)
        {
            stationActions.Actions.Remove(trainAction);
            
            _eventBus.Post(new OnScheduleUpdatedEvent());
        }
    }
}