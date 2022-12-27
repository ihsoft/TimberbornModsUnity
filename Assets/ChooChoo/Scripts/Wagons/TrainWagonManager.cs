using System.Collections.Generic;
using Bindito.Core;
using Timberborn.AssetSystem;
using Timberborn.Characters;
using Timberborn.EntitySystem;
using Timberborn.FactionSystemGame;
using Timberborn.Localization;
using Timberborn.Persistence;
using Timberborn.TickSystem;
using UnityEngine;

namespace ChooChoo
{
    public class TrainWagonManager : TickableComponent, IDeletableEntity, IPersistentEntity
    {
        private static readonly ComponentKey TrainManagerKey = new(nameof(TrainYard));

        private static readonly ListKey<TrainWagon> TrainKey = new(nameof(TrainWagons));

        private const string TrainNameLocKey = "Tobbert.Train.Name";

        private ILoc _loc;

        private EntityService _entityService;

        private IResourceAssetLoader _resourceAssetLoader;

        private FactionService _factionService;
        
        private WagonsObjectSerializer _wagonsObjectSerializer;

        public List<TrainWagon> TrainWagons { get; private set; }

        public int NumberOfCarts = 4;

        public float minDistanceFromTrain;

        [Inject]
        public void InjectDependencies(ILoc loc, EntityService entityService, IResourceAssetLoader resourceAssetLoader, FactionService factionService, WagonsObjectSerializer wagonsObjectSerializer)
        {
            _loc = loc;
            _entityService = entityService;
            _resourceAssetLoader = resourceAssetLoader;
            _factionService = factionService;
            _wagonsObjectSerializer = wagonsObjectSerializer;
        }
        
        public override void StartTickable()
        {
            if (TrainWagons != null) 
                return;
            InitializeCarts();
            SetObjectToFollow();
        }
        
        public override void Tick()
        {
            MoveWagons();
        }

        public void DeleteEntity()
        {
            foreach (var cart in TrainWagons)
            {
                _entityService.Delete(cart.gameObject);
            }
        }

        public void Save(IEntitySaver entitySaver)
        {
            if (TrainWagons != null)
                entitySaver.GetComponent(TrainManagerKey).Set(TrainKey, TrainWagons, _wagonsObjectSerializer);
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(TrainManagerKey))
                return;
            
            var component = entityLoader.GetComponent(TrainManagerKey);

            if (!component.Has(TrainKey)) 
                return;
            
            TrainWagons = component.Get(TrainKey, _wagonsObjectSerializer);
            SetObjectToFollow();
        }

        private void SetObjectToFollow()
        {
            for (int i = TrainWagons.Count - 1; i > 0; i--)
            {
                TrainWagons[i].InitializeObjectFollower(TrainWagons[i - 1].transform, TrainWagons[i - 1].wagonLength);
            }
            
            TrainWagons[0].InitializeObjectFollower(transform, minDistanceFromTrain);
        }

        public void SetNewPathConnections(ITrackFollower trackFollower, List<TrackConnection> pathConnections)
        {
            TrainWagons[0].StartMoving(trackFollower, pathConnections);
            for (var index = 1; index < TrainWagons.Count; index++)
            {
                var trainWagon = TrainWagons[index];
                trainWagon.StartMoving(TrainWagons[index - 1].ObjectFollower, pathConnections);
            }
        }

        public void StopWagons()
        {
            foreach (var trainWagon in TrainWagons)
            {
                trainWagon.GetComponent<TrainWagon>().Stop();
            }
        }

        private void MoveWagons()
        {
            foreach (var trainWagon in TrainWagons)
            {
                trainWagon.Move();
            }
        }
        
        private void InitializeCarts()
        {
            var trainWagons = new List<TrainWagon>();
            // var train = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/Train." + _factionService.Current.Id);
            var cartPrefab = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/SmallMetalCart.Folktails");
            for (int cartNumber = 0; cartNumber < NumberOfCarts; cartNumber++)
            {
                var wagon = _entityService.Instantiate(cartPrefab.gameObject);
                trainWagons.Add(wagon.GetComponent<TrainWagon>());

                SetInitialCartPosition(wagon, cartNumber);
                
                Character component = wagon.GetComponent<Character>();
                component.FirstName = _loc.T(TrainNameLocKey);
            }

            TrainWagons = trainWagons;
        }

        private void SetInitialCartPosition(GameObject cart,int cartNumber)
        {
            var spawnLocation = transform.position + new Vector3(0, 0f, -0.6f * cartNumber - 1);
            Plugin.Log.LogInfo("Spawning wagon " + cartNumber + " at: " + spawnLocation);
            cart.transform.position = spawnLocation;
        }
    }
}