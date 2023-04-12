using System.Collections.Generic;
using Bindito.Core;
using Timberborn.EntitySystem;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
    public class WagonManager : MonoBehaviour, IDeletableEntity, IPersistentEntity
    {
        private static readonly ComponentKey TrainWagonManagerKey = new(nameof(TrainYard));

        private static readonly ListKey<TrainWagon> WagonsKey = new("TrainWagons");

        private EntityService _entityService;

        private WagonsObjectSerializer _wagonsObjectSerializer;

        private WagonInitializer _wagonInitializer;

        private WagonModelManager _wagonModelManager;

        public List<TrainWagon> Wagons { get; private set; }

        public int MinimumNumberOfWagons = 2;
        
        public int MaximumNumberOfWagons = 4;

        // public float minDistanceFromTrain;

        // public event EventHandler WagonTypesChanged;

        [Inject]
        public void InjectDependencies(
            EntityService entityService, 
            WagonsObjectSerializer wagonsObjectSerializer,
            WagonInitializer wagonInitializer)
        {
            _entityService = entityService;
            _wagonsObjectSerializer = wagonsObjectSerializer;
            _wagonInitializer = wagonInitializer;
        }

        public void Start()
        {
            if (Wagons == null)
                InitializeWagons();
            SetObjectToFollow();
        }

        public void DeleteEntity()
        {
            foreach (var wagon in Wagons)
                _entityService.Delete(wagon.gameObject);
        }

        public void Save(IEntitySaver entitySaver)
        {
            if (Wagons != null)
                entitySaver.GetComponent(TrainWagonManagerKey).Set(WagonsKey, Wagons, _wagonsObjectSerializer);
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(TrainWagonManagerKey))
                return;
            var component = entityLoader.GetComponent(TrainWagonManagerKey);
            if (component.Has(WagonsKey))
                Wagons = component.Get(WagonsKey, _wagonsObjectSerializer);
        }

        public void SetObjectToFollow()
        {
            for (int i = Wagons.Count - 1; i > 0; i--)
                Wagons[i].InitializeObjectFollower(Wagons[i - 1].transform, Wagons[i - 1].GetComponent<WagonModelManager>().ActiveWagonModel.WagonModelSpecification.Length);
            Wagons[0].InitializeObjectFollower(transform, GetComponent<TrainModelManager>().ActiveTrainModel.TrainModelSpecification.Length);
        }

        public void UpdateWagonType(string type, int index)
        {
            Wagons[index].GetComponent<WagonModelManager>().UpdateTrainType(type);
            SetObjectToFollow();
        }
        
        private void InitializeWagons()
        {
            var trainWagons = new List<TrainWagon>();
            for (int i = 0; i < MaximumNumberOfWagons; i++)
                trainWagons.Add(_wagonInitializer.InitializeWagon(gameObject, i));
            Wagons = trainWagons;
        }
    }
}