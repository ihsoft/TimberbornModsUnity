using Bindito.Core;
using HarmonyLib;
using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.Characters;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.FactionSystemGame;
using Timberborn.Localization;
using Timberborn.Persistence;
using UnityEngine;

namespace GlobalMarket
{
    public class TrainsManager : MonoBehaviour, IFinishedStateListener, IPersistentEntity
    {
        private static readonly ComponentKey AirBalloonManagerKey = new(nameof (TrainsManager));
        
        private static readonly PropertyKey<GameObject> AirBalloonKey = new(nameof (_train));
        
        private const string AirBalloonNameLocKey = "Tobbert.AirBalloon.Name";

        private ILoc _loc;
        
        private EntityService _entityService;

        private IResourceAssetLoader _resourceAssetLoader;

        private FactionService _factionService;

        private GameObject _train;

        // private GlobalMarketServant _globalMarketServant;

        public bool AirBalloonEnabled { get; private set; } = true;

        [Inject]
        public void InjectDependencies(ILoc loc, EntityService entityService, IResourceAssetLoader resourceAssetLoader, FactionService factionService)
        {
            _loc = loc;
            _entityService = entityService;
            _resourceAssetLoader = resourceAssetLoader;
            _factionService = factionService;
        }
        
        public void Save(IEntitySaver entitySaver)
        {
            if (_train != null)
                entitySaver.GetComponent(AirBalloonManagerKey).Set(AirBalloonKey, _train);
        }
        
        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(AirBalloonManagerKey))
                return;
            
            var component = entityLoader.GetComponent(AirBalloonManagerKey);
            
            if (component.Has(AirBalloonKey))
                _train = component.Get(AirBalloonKey);
        }
        
        public void OnEnterFinishedState()
        {
            if (_train == null)
            {
                InitializeAirBalloon();
            }
        }

        public void OnExitFinishedState()
        {
            _entityService.Delete(_train);
        }
        
        public void EnableAirBalloon()
        {
            AirBalloonEnabled = true;
            _train.SetActive(true);
        }
        
        public void DisableAirBalloon()
        {
            AirBalloonEnabled = false;
            _train.SetActive(false);
        }

        private void InitializeAirBalloon()
        {
            var trainPrefab = _resourceAssetLoader.Load<GameObject>("tobbert.globalmarket/tobbert_globalmarket/Train." + _factionService.Current.Id);
            
            _train = _entityService.Instantiate(trainPrefab.gameObject);
            
            Destroy(_train.GetComponent(AccessTools.TypeByName("StrandedStatus")));

            SetTrainPosition();
            
            SetAirBalloonName();

            // var globMarketServant = _airBalloon.GetComponent<GlobalMarketServant>();
            // var globalMarket = gameObject;
            // globMarketServant.LinkedGlobalMarket = globalMarket;
            // globMarketServant.LinkedGlobalMarketPosition = globalMarket.transform.position + GetSpawnOffset();
        }
        
        private void SetTrainPosition()
        {
            var position = _train.transform.position;
            // position += GetSpawnOffset();
            position += new Vector3(1, 0, 1);
            position += transform.position;
            _train.transform.position = position;
        }

        private Vector3 GetSpawnOffset()
        {
            var orientation = GetComponent<BlockObject>().Orientation;
            switch (orientation)
            {
                default:
                    return new Vector3(1.9f, 0.45f, 1.8f);
                case Orientation.Cw90:
                    return new Vector3(1.9f, 0.45f, -1.8f);
                case Orientation.Cw180:
                    return new Vector3(-1.9f, 0.45f, -1.8f);
                case Orientation.Cw270:
                    return new Vector3(-1.9f, 0.45f, 1.8f);
            }
        }

        private void SetAirBalloonName()
        {
            Character component = _train.GetComponent<Character>();
            component.FirstName = _loc.T(AirBalloonNameLocKey);
        }
    }
}
