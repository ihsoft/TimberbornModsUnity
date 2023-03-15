using System;
using System.Collections.Generic;
using Bindito.Core;
using Timberborn.EntitySystem;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
    public class TrainWagonManager : MonoBehaviour, IDeletableEntity, IPersistentEntity
    {
        private static readonly ComponentKey TrainWagonManagerKey = new(nameof(TrainYard));

        private static readonly ListKey<TrainWagon> WagonsKey = new(nameof(TrainWagons));

        private EntityService _entityService;

        private WagonsObjectSerializer _wagonsObjectSerializer;

        private WagonInitializer _wagonInitializer;

        private WagonModelSwitcher _wagonModelSwitcher;

        public List<TrainWagon> TrainWagons { get; private set; }

        public int MinimumNumberOfWagons = 2;
        
        public int MaximumNumberOfWagons = 4;

        public float minDistanceFromTrain;

        [NonSerialized] 
        public readonly List<string> WagonTypes = new() { "Tobbert.WagonType.Box", "Tobbert.WagonType.Liquid", "Tobbert.WagonType.Flat", "Tobbert.WagonType.Gondola" };

        public event EventHandler WagonTypesChanged;

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
            if (TrainWagons == null)
            {
                InitializeWagons();
            }
            SetObjectToFollow();
            UpdateWagonModels();
        }

        public void DeleteEntity()
        {
            foreach (var wagon in TrainWagons)
                _entityService.Delete(wagon.gameObject);
        }

        public void Save(IEntitySaver entitySaver)
        {
            if (TrainWagons != null)
                entitySaver.GetComponent(TrainWagonManagerKey).Set(WagonsKey, TrainWagons, _wagonsObjectSerializer);
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(TrainWagonManagerKey))
                return;
            var component = entityLoader.GetComponent(TrainWagonManagerKey);
            if (component.Has(WagonsKey))
                TrainWagons = component.Get(WagonsKey, _wagonsObjectSerializer);
        }

        private void SetObjectToFollow()
        {
            for (int i = TrainWagons.Count - 1; i > 0; i--)
                TrainWagons[i].InitializeObjectFollower(TrainWagons[i - 1].transform, TrainWagons[i - 1].wagonLength);

            TrainWagons[0].InitializeObjectFollower(transform, minDistanceFromTrain);
        }

        public void UpdateWagonType(string type, int index)
        {
            TrainWagons[index].ActiveWagonType = type;
            UpdateWagonModels();
        }
        
        private void InitializeWagons()
        {
            var trainWagons = new List<TrainWagon>();
            for (int i = 0; i < MaximumNumberOfWagons; i++)
                trainWagons.Add(_wagonInitializer.InitializeWagon(gameObject, i));
            TrainWagons = trainWagons;
        }

        private void UpdateWagonModels()
        {
            foreach (var trainWagon in TrainWagons)
            {
                trainWagon.GetComponent<WagonModelSwitcher>().RefreshModel();
            }
        }
    }
}