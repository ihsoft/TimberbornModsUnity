using Bindito.Core;
using System;
using System.Linq;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.Common;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class TrainDistributorBehavior : RootBehavior, IPersistentEntity
  {
    private static readonly ComponentKey FlyingRootBehaviorKey = new(nameof (TrainDistributorBehavior));
    private static readonly PropertyKey<bool> ExectureActionsKey = new("ExecuteActions");
    private RandomTrainDestinationPicker _randomTrainDestinationPicker;
    private IRandomNumberGenerator _randomNumberGenerator;
    private GoodsStationsRepository _goodsStationsRepository;
    private TrainDestinationService _trainDestinationService;
    private BlockService _blockService;
    private TrainScheduleController _trainScheduleController;
    private MoveToStationExecutor _moveToStationExecutor;
    private TrainDistributeGoodsExecutor _trainDistributeGoodsExecutor;
    private Vector3 _globalMarketPosition;
    private bool _executeActions;
    private int currentStationIndex = -1;
    private int currentTrainActionIndex;
    
    [Inject]
    public void InjectDependencies(
      RandomTrainDestinationPicker randomTrainDestinationPicker, 
      IRandomNumberGenerator randomNumberGenerator,
      GoodsStationsRepository goodsStationsRepository,
      TrainDestinationService trainDestinationService,
      BlockService blockService)
    {
      _randomTrainDestinationPicker = randomTrainDestinationPicker;
      _randomNumberGenerator = randomNumberGenerator;
      _goodsStationsRepository = goodsStationsRepository;
      _trainDestinationService = trainDestinationService;
      _blockService = blockService;
    }

    public void Awake()
    {
      _trainScheduleController = GetComponent<TrainScheduleController>();
      _moveToStationExecutor = GetComponent<MoveToStationExecutor>();
      _trainDistributeGoodsExecutor = GetComponent<TrainDistributeGoodsExecutor>();
    }

    public override Decision Decide(GameObject agent)
    {
      var start = transform.position;
      var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(Vector3Int.FloorToInt(new Vector3(start.x, start.z, start.y)));
      
      var reachableGoodStation = _goodsStationsRepository.GoodsStations.FirstOrDefault(station => _trainDestinationService.DestinationReachable(startTrackPiece, station.TrainDestinationComponent));

      // foreach (var VARIABLE in _goodsStationsRepository.GoodsStations)
      // {
      //   Plugin.Log.LogWarning(VARIABLE.TrainDestinationComponent.name + "   " + _trainDestinationService.DestinationReachable(startTrackPiece, VARIABLE.TrainDestinationComponent));
      // }
      // Plugin.Log.LogWarning(_goodsStationsRepository.GoodsStations.Count + "");
      if (reachableGoodStation == null)
        return Decision.ReleaseNow();
      var reachableGoodStations = _goodsStationsRepository.GoodsStations.Where(station => _trainDestinationService.TrainDestinationsConnected(reachableGoodStation.TrainDestinationComponent, station.TrainDestinationComponent));
      
      foreach (var startGoodsStation in reachableGoodStations)
      {
        foreach (var endGoodStation in reachableGoodStations)
        {

          foreach (var startTransferableGood in startGoodsStation.TransferableGoods.Where(good => good.Enabled && good.CanReceiveGoods))
          {
            foreach (var endTransferableGood in endGoodStation.TransferableGoods.Where(good => good.Enabled && !good.CanReceiveGoods))
            {
              if (startTransferableGood.GoodId == endTransferableGood.GoodId)
              {
                var startInStock = startGoodsStation.Inventory.AmountInStock(startTransferableGood.GoodId);

                if (startInStock > 20)
                {
                  var endInStock = endGoodStation.Inventory.AmountInStock(endTransferableGood.GoodId);

                  var desired = endGoodStation.MaxCapacity - endInStock;

                  if (desired > 20)
                  {
                    Plugin.Log.LogError("Distribute: " + startInStock + " Desired: " + desired);
                    if (_trainDistributeGoodsExecutor.Launch(startGoodsStation, startTransferableGood, endGoodStation, Math.Min(startInStock, desired)))
                    {
                      return Decision.ReleaseWhenFinished(_trainDistributeGoodsExecutor);
                    }
                  }
                }
              }
            }
          }
        }
      }
      
      return Decision.ReleaseNow();
    }

    public void Save(IEntitySaver entitySaver)
    {
      entitySaver.GetComponent(FlyingRootBehaviorKey).Set(ExectureActionsKey, _executeActions);
    }

    public void Load(IEntityLoader entityLoader)
    {
      _executeActions = entityLoader.GetComponent(FlyingRootBehaviorKey).Get(ExectureActionsKey);
    }

    private Decision GoToNextStation()
    {
      _executeActions = true;
      
      // var randomLocation = _randomTrainDestinationPicker.RandomTrainDestination();
      
      currentStationIndex += 1;
      
      if (currentStationIndex >= _trainScheduleController.TrainSchedule.Count)
        currentStationIndex = 0;
      
      var nextStation = _trainScheduleController.TrainSchedule[currentStationIndex].Station;

      Plugin.Log.LogWarning( "Go to next station");
      switch (_moveToStationExecutor.Launch(nextStation))
      {
        case ExecutorStatus.Success:
          return Decision.ReleaseNow();
        case ExecutorStatus.Failure:
          return Decision.ReleaseNow();
        case ExecutorStatus.Running:
          return Decision.ReturnWhenFinished(_moveToStationExecutor);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private Decision ExecuteAction()
    {
      if (LastActionExecuted())
      {
        currentTrainActionIndex = 0;
        _executeActions = false;
        return Decision.ReleaseNow();
      }

      var trainAction = _trainScheduleController.TrainSchedule[currentStationIndex].Actions[currentTrainActionIndex];
      
      currentTrainActionIndex += 1;

      return trainAction.ExecuteAction();
    }
    
    private bool LastActionExecuted() => currentTrainActionIndex >= _trainScheduleController.TrainSchedule[currentStationIndex].Actions.Count;
  }
}
