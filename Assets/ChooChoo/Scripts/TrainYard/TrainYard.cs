using System.Linq;
using Bindito.Core;
using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.Characters;
using Timberborn.Common;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.FactionSystemGame;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Localization;
using UnityEngine;

namespace ChooChoo
{
    public class TrainYard : MonoBehaviour, IRegisteredComponent, IFinishedStateListener, IDeletableEntity
    {
        private const string TrainNameLocKey = "Tobbert.Train.Name";

        [SerializeField]
        private int _maxCapacity;
        
        private ILoc _loc;

        private EntityService _entityService;

        private IResourceAssetLoader _resourceAssetLoader;

        private FactionService _factionService;

        private TrainYardService _trainYardService;
        public Inventory Inventory { get; private set; }
        public int MaxCapacity => _maxCapacity;

        [Inject]
        public void InjectDependencies(ILoc loc, EntityService entityService, IResourceAssetLoader resourceAssetLoader, FactionService factionService, TrainYardService trainYardService)
        {
            _loc = loc;
            _entityService = entityService;
            _resourceAssetLoader = resourceAssetLoader;
            _factionService = factionService;
            _trainYardService = trainYardService;
        }

        public void Awake()
        {
            enabled = false;
            if (!name.ToLower().Contains("preview"))
                _trainYardService.CurrentTrainYard = GetComponent<TrainDestination>();
        }

        public void OnEnterFinishedState()
        {
            enabled = true;
            Inventory.Enable();
        }

        public void OnExitFinishedState()
        {
            Inventory.Disable();
            enabled = false;
        }
        
        public void InitializeInventory(Inventory inventory)
        {
            Asserts.FieldIsNull(this, Inventory, "Inventory");
            Inventory = inventory;
        }

        public void InitializeTrain()
        {
            // var train = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/Train." + _factionService.Current.Id);
            var trainPrefab = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/SmallLogTrain.Folktails");

            var train = _entityService.Instantiate(trainPrefab.gameObject);
            
            foreach (var goodAmountSpecification in train.GetComponent<Train>().TrainCost)
                Inventory.Take(goodAmountSpecification.ToGoodAmount());

            train.GetComponent<TrainYardSubject>().HomeTrainYard = GetComponent<TrainDestination>();

            SetInitialTrainLocation(train);

            SetTrainName(train);

            train.GetComponent<Machinist>().LastTrackConnection = GetComponent<TrackPiece>().TrackRoutes.First(connection => connection.Entrance.Direction == Direction2D.Up);
        }

        public void DeleteEntity()
        {
            // Plugin.Log.LogInfo("Removing");
            // _trainYardService.CurrentTrainYard = null;
        }

        private void SetInitialTrainLocation(GameObject train)
        {
            train.transform.rotation = GetComponent<BlockObject>().Orientation.ToWorldSpaceRotation();
            var position = train.transform.position;
            position += GetSpawnOffset();
            position += transform.position;
            train.transform.position = position;
        }

        private Vector3 GetSpawnOffset() => GetComponent<BlockObject>().Orientation.TransformInWorldSpace(new Vector3(0.5f, 0f, 2.8f));

        private void SetTrainName(GameObject train)
        {
            Character component = train.GetComponent<Character>();
            component.FirstName = _loc.T(TrainNameLocKey);
        }
    }
}