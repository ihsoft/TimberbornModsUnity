using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using HarmonyLib;
using Timberborn.AssetSystem;
using Timberborn.Characters;
using Timberborn.EntitySystem;
using Timberborn.FactionSystemGame;
using Timberborn.Localization;
using Timberborn.Persistence;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChooChoo
{
    public class TrainWagonManager : MonoBehaviour, IPersistentEntity
    {
        private static readonly ComponentKey TrainManagerKey = new(nameof(TrainManager));

        private static readonly ListKey<TrainWagon> TrainKey = new(nameof(_trainWagons));

        private const string TrainNameLocKey = "Tobbert.Train.Name";

        private ILoc _loc;

        private EntityService _entityService;

        private IResourceAssetLoader _resourceAssetLoader;

        private FactionService _factionService;
        
        private WagonsObjectSerializer _wagonsObjectSerializer;

        private List<TrainWagon> _trainWagons;

        private readonly List<List<Vector3>> _previousPathCorners = new();

        private int NumberOfCarts = 4;

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

        private void Start()
        {
            if (_trainWagons != null) 
                return;
            InitializeCarts();
            SetObjectToFollow();
        }

        private void OnDestroy()
        {
            foreach (var cart in _trainWagons)
            {
                _entityService.Delete(cart.gameObject);
            }
        }

        public void Save(IEntitySaver entitySaver)
        {
            if (_trainWagons != null)
                entitySaver.GetComponent(TrainManagerKey).Set(TrainKey, _trainWagons, _wagonsObjectSerializer);
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(TrainManagerKey))
                return;
            
            var component = entityLoader.GetComponent(TrainManagerKey);

            if (!component.Has(TrainKey)) 
                return;
            
            _trainWagons = component.Get(TrainKey, _wagonsObjectSerializer);
            SetObjectToFollow();
        }

        private void SetObjectToFollow()
        {
            for (int i = _trainWagons.Count - 1; i > 0; i--)
            {
                _trainWagons[i].SetObjectToFollow(_trainWagons[i - 1].transform, _trainWagons[i - 1].wagonLength);
            }
            
            _trainWagons[0].SetObjectToFollow(transform, minDistanceFromTrain);
            // for (int i = 0; i < _trainWagonsReversed.Count - 1; i++)
            // {
            //     _trainWagonsReversed[i].SetObjectToFollow(_trainWagonsReversed[i + 1].transform);
            // }
            //
            // _trainWagonsReversed.Last().SetObjectToFollow(transform);
        }

        public void SetTrackFollower(TrackFollower trackFollower)
        {
            foreach (var trainWagon in _trainWagons)
            {
                trainWagon.SetPathCorners(trackFollower);
            }
        }

        public void MoveWagons(List<Vector3> pathCorners, float time, float speed)
        {
            foreach (var trainWagon in _trainWagons)
            {
                trainWagon.Move(time, speed);
            }
            //
            // // _previousPathCorners.Add(pathCorners);
            //
            // for (int i = 0; i < _trainWagons.Count - 1; i++)
            // {
            //     _trainWagons[i].Move(time, speed);
            // }
            //
            // if (pathCorners != null)
            // // if (_previousPathCorners.Count >= 2)
            // {
            //     // _trainWagonsReversed.Last().Move(_previousPathCorners.SelectMany(list => list.Select(vector => vector)).ToList(), time, speed);
            //     _trainWagons.Last().Move(time, speed);
            // }
        }
        
        public void StopWagons()
        {
            foreach (var trainWagon in _trainWagons)
            {
                trainWagon.GetComponent<TrainWagon>().Stop();
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

                Destroy(wagon.GetComponent(AccessTools.TypeByName("StrandedStatus")));

                SetInitialCartPosition(wagon, cartNumber);
                
                Character component = wagon.GetComponent<Character>();
                component.FirstName = _loc.T(TrainNameLocKey);
            }

            // trainWagons.Reverse();
            _trainWagons = trainWagons;
        }

        private void SetInitialCartPosition(GameObject cart,int cartNumber)
        {
            var spawnLocation = transform.position + new Vector3(0, 0f, -0.6f * cartNumber - 1);
            Plugin.Log.LogInfo("Spawning wagon " + cartNumber + " at: " + spawnLocation);
            cart.transform.position = spawnLocation;
        }
    }
}