using Bindito.Core;
using System;
using System.Linq;
using Timberborn.BehaviorSystem;
using Timberborn.BlockSystem;
using Timberborn.Goods;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class TrainDistributorBehavior : RootBehavior, IPersistentEntity
  {
    private static readonly ComponentKey FlyingRootBehaviorKey = new(nameof(TrainDistributorBehavior));
    private static readonly PropertyKey<bool> ExectureActionsKey = new("ExecuteActions");
    private GoodsStationsRepository _goodsStationsRepository;
    private TrainDestinationService _trainDestinationService;
    private BlockService _blockService;
    private TrainDistributeGoodsExecutor _trainDistributeGoodsExecutor;
    private Vector3 _globalMarketPosition;
    private bool _executeActions;
    private int currentStationIndex = -1;
    private int currentTrainActionIndex;

    [Inject]
    public void InjectDependencies(
      GoodsStationsRepository goodsStationsRepository,
      TrainDestinationService trainDestinationService,
      BlockService blockService)
    {
      _goodsStationsRepository = goodsStationsRepository;
      _trainDestinationService = trainDestinationService;
      _blockService = blockService;
    }

    public void Awake()
    {
      _trainDistributeGoodsExecutor = GetComponent<TrainDistributeGoodsExecutor>();
    }

    public override Decision Decide(GameObject agent)
    {
      var start = transform.position;
      var startTrackPiece =
        _blockService.GetFloorObjectComponentAt<TrackPiece>(
          Vector3Int.FloorToInt(new Vector3(start.x, start.z, start.y)));

      var reachableGoodStation = _goodsStationsRepository.GoodsStations.FirstOrDefault(station => _trainDestinationService.DestinationReachable(startTrackPiece, station.TrainDestinationComponent));

      // foreach (var VARIABLE in _goodsStationsRepository.GoodsStations)
      // {
      //   Plugin.Log.LogWarning(VARIABLE.TrainDestinationComponent.name + "   " + _trainDestinationService.DestinationReachable(startTrackPiece, VARIABLE.TrainDestinationComponent));
      // }
      // Plugin.Log.LogWarning(_goodsStationsRepository.GoodsStations.Count + "");
      
      if (reachableGoodStation == null)
        return Decision.ReleaseNow();
      
      var reachableGoodStations = _goodsStationsRepository.GoodsStations.Where(station =>
        _trainDestinationService.TrainDestinationsConnected(reachableGoodStation.TrainDestinationComponent,
          station.TrainDestinationComponent));

      foreach (var startGoodsStation in reachableGoodStations)
      {
        foreach (var endGoodStation in reachableGoodStations)
        {

          foreach (var startTransferableGood in startGoodsStation.TransferableGoods.Where(good =>
                     good.Enabled && good.CanReceiveGoods))
          {
            foreach (var endTransferableGood in endGoodStation.TransferableGoods.Where(good =>
                       good.Enabled && !good.CanReceiveGoods))
            {
              if (startTransferableGood.GoodId != endTransferableGood.GoodId)
                continue;

              var startInStock = startGoodsStation.Inventory.AmountInStock(startTransferableGood.GoodId);

              if (startInStock <= 20)
                continue;

              var endInStock = endGoodStation.Inventory.AmountInStock(endTransferableGood.GoodId);

              var desired = endGoodStation.MaxCapacity - endInStock;

              if (desired <= 20)
                continue;

              Plugin.Log.LogError("Distribute: " + startInStock + " Desired: " + desired);
              switch (_trainDistributeGoodsExecutor.Launch(startGoodsStation, endGoodStation, new GoodAmount(startTransferableGood.GoodId, Math.Min(startInStock, desired))))
              {
                case ExecutorStatus.Success:
                  return Decision.ReleaseNow();
                case ExecutorStatus.Failure:
                  return Decision.ReleaseNow();
                case ExecutorStatus.Running:
                  return Decision.ReturnWhenFinished(_trainDistributeGoodsExecutor);
                default:
                  throw new ArgumentOutOfRangeException();
              }
            }
          }
        }
      }

      return Decision.ReleaseNow();
    }

    public void Save(IEntitySaver entitySaver)
    {
    }

    public void Load(IEntityLoader entityLoader)
    {
    }
  }
}
