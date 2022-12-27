using System.Linq;
using Bindito.Core;
using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.Characters;
using Timberborn.ConstructibleSystem;
using Timberborn.Coordinates;
using Timberborn.EntitySystem;
using Timberborn.FactionSystemGame;
using Timberborn.Localization;
using UnityEngine;

namespace ChooChoo
{
    public class TrainYard : MonoBehaviour, IFinishedStateListener
    {
        private const string TrainNameLocKey = "Tobbert.Train.Name";

        private ILoc _loc;

        private EntityService _entityService;

        private IResourceAssetLoader _resourceAssetLoader;

        private FactionService _factionService;

        private TrainYardService _trainYardService;
        
        private GameObject _train;

        [Inject]
        public void InjectDependencies(ILoc loc, EntityService entityService, IResourceAssetLoader resourceAssetLoader, FactionService factionService, TrainYardService trainYardService)
        {
            _loc = loc;
            _entityService = entityService;
            _resourceAssetLoader = resourceAssetLoader;
            _factionService = factionService;
            _trainYardService = trainYardService;
        }

        public void OnEnterFinishedState()
        {
            _trainYardService.CurrentTrainYard = GetComponent<TrainDestination>();
        }

        public void OnExitFinishedState()
        {
            _trainYardService.CurrentTrainYard = null;
        }

        public void InitializeTrain()
        {
            // var train = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/Train." + _factionService.Current.Id);
            var train = _resourceAssetLoader.Load<GameObject>("tobbert.choochoo/tobbert_choochoo/SmallLogTrain.Folktails");

            _train = _entityService.Instantiate(train.gameObject);

            _train.GetComponent<TrainYardSubject>().HomeTrainYard = GetComponent<TrainDestination>();

            SetInitialTrainLocation();

            SetTrainName();

            _train.GetComponent<Machinist>().LastTrackConnection = GetComponent<TrackPiece>().TrackConnections.First(connection => connection.Direction == Direction2D.Down);
        }

        private void SetInitialTrainLocation()
        {
            _train.transform.rotation = GetComponent<BlockObject>().Orientation.ToWorldSpaceRotation();
            var position = _train.transform.position;
            position += GetSpawnOffset();
            position += transform.position;
            _train.transform.position = position;
        }

        private Vector3 GetSpawnOffset()
        {
            return GetComponent<BlockObject>().Orientation.TransformInWorldSpace(new Vector3(0.5f, 0f, 2.8f));
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

        private void SetTrainName()
        {
            Character component = _train.GetComponent<Character>();
            component.FirstName = _loc.T(TrainNameLocKey);
        }
    }
}