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

namespace ChooChoo
{
    public class TrainManager : MonoBehaviour, IFinishedStateListener, IPersistentEntity
    {
        private static readonly ComponentKey TrainManagerKey = new(nameof(TrainManager));

        private static readonly PropertyKey<GameObject> TrainKey = new(nameof(_train));

        private const string TrainNameLocKey = "Tobbert.Train.Name";

        private ILoc _loc;

        private EntityService _entityService;

        private IResourceAssetLoader _resourceAssetLoader;

        private FactionService _factionService;

        private GameObject _train;

        // private GlobalMarketServant _globalMarketServant;

        [Inject]
        public void InjectDependencies(ILoc loc, EntityService entityService, IResourceAssetLoader resourceAssetLoader,
            FactionService factionService)
        {
            _loc = loc;
            _entityService = entityService;
            _resourceAssetLoader = resourceAssetLoader;
            _factionService = factionService;
        }

        public void Save(IEntitySaver entitySaver)
        {
            if (_train != null)
                entitySaver.GetComponent(TrainManagerKey).Set(TrainKey, _train);
        }

        public void Load(IEntityLoader entityLoader)
        {
            if (!entityLoader.HasComponent(TrainManagerKey))
                return;

            var component = entityLoader.GetComponent(TrainManagerKey);

            if (component.Has(TrainKey))
                _train = component.Get(TrainKey);
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

        private void InitializeAirBalloon()
        {
            // var airBalloonPrefab = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/Train." + _factionService.Current.Id);
            var airBalloonPrefab = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/Train.Folktails");

            _train = _entityService.Instantiate(airBalloonPrefab.gameObject);

            Destroy(_train.GetComponent(AccessTools.TypeByName("StrandedStatus")));

            SetAirBalloonPosition();

            SetAirBalloonName();

            var trainYardSubject = _train.GetComponent<TrainYardSubject>();
            var trainYard = gameObject;
            trainYardSubject.LinkedGlobalMarket = trainYard;
            trainYardSubject.LinkedGlobalMarketPosition = trainYard.transform.position + GetSpawnOffset();
        }

        private void SetAirBalloonPosition()
        {
            var position = _train.transform.position;
            position += GetSpawnOffset();
            position += transform.position;
            _train.transform.position = position;
        }

        private Vector3 GetSpawnOffset()
        {
            return new Vector3(0.5f, 0f, 0.5f);
            // var orientation = GetComponent<BlockObject>().Orientation;
            // switch (orientation)
            // {
            //     default:
            //         return new Vector3(1.9f, 0.45f, 1.8f);
            //     case Orientation.Cw90:
            //         return new Vector3(1.9f, 0.45f, -1.8f);
            //     case Orientation.Cw180:
            //         return new Vector3(-1.9f, 0.45f, -1.8f);
            //     case Orientation.Cw270:
            //         return new Vector3(-1.9f, 0.45f, 1.8f);
            // }
        }

        private void SetAirBalloonName()
        {
            Character component = _train.GetComponent<Character>();
            component.FirstName = _loc.T(TrainNameLocKey);
        }
    }
}