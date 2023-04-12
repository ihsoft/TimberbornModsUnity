using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.Characters;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using Timberborn.TimeSystem;
using UnityEngine;

namespace ChooChoo
{
    public class WagonInitializer : ILoadableSingleton
    {
        private readonly IResourceAssetLoader _resourceAssetLoader;
        
        private readonly TrainYardService _trainYardService;

        private readonly IDayNightCycle _dayNightCycle;
        
        private readonly EntityService _entityService;

        private readonly ILoc _loc;

        private GameObject _trainWagonPrefab;
        
        WagonInitializer(IResourceAssetLoader resourceAssetLoader, TrainYardService trainYardService, IDayNightCycle dayNightCycle, EntityService entityService, ILoc loc)
        {
            _resourceAssetLoader = resourceAssetLoader;
            _trainYardService = trainYardService;
            _dayNightCycle = dayNightCycle;
            _entityService = entityService;
            _loc = loc;
        }

        public void Load()
        {
            _trainWagonPrefab = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/Wagon");
        }
        
        public TrainWagon InitializeWagon(GameObject train, int cartNumber)
        {
            var wagon = _entityService.Instantiate(_trainWagonPrefab.gameObject);
            var trainWagon = wagon.GetComponent<TrainWagon>();
            trainWagon.Train = train;

            SetInitialWagonPosition(train, wagon, cartNumber);
            SimpleLabeledPrefab simpleLabeledPrefab = wagon.GetComponent<SimpleLabeledPrefab>();
            Character character = wagon.GetComponent<Character>();
            character.FirstName = _loc.T(simpleLabeledPrefab.PrefabNameLocKey);
            character.DayOfBirth = _dayNightCycle.DayNumber;

            return wagon.GetComponent<TrainWagon>();
        }

        private void SetInitialWagonPosition(GameObject train, GameObject wagon, int cartNumber)
        {
            wagon.transform.rotation = _trainYardService.CurrentTrainYard.GetComponent<BlockObject>().Orientation.ToWorldSpaceRotation();
            var transform1 = train.transform;
            var offset = transform1.rotation * new Vector3(0, 0f, -0.6f * cartNumber - 1);
            var spawnLocation = transform1.position + offset;
            // Plugin.Log.LogInfo("Spawning wagon " + cartNumber + " at: " + spawnLocation);
            wagon.transform.position = spawnLocation;
        }
    }
}