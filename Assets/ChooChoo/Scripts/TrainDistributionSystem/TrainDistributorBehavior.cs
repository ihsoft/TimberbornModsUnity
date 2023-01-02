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
      var startTrackPiece = _blockService.GetFloorObjectComponentAt<TrackPiece>(Vector3Int.FloorToInt(new Vector3(start.x, start.z, start.y)));

      var reachableGoodStation = _goodsStationsRepository.GoodsStations.FirstOrDefault(station => _trainDestinationService.DestinationReachable(startTrackPiece, station.TrainDestinationComponent));

      if (reachableGoodStation == null)
        return Decision.ReleaseNow();
      
      var reachableGoodStations = _goodsStationsRepository.GoodsStations.Where(station => _trainDestinationService.TrainDestinationsConnected(reachableGoodStation.TrainDestinationComponent, station.TrainDestinationComponent)).ToArray();

      foreach (var startGoodsStation in reachableGoodStations)
      {
        foreach (var endGoodStation in reachableGoodStations)
        {

          foreach (var startTransferableGood in startGoodsStation.SendingGoods)
          {
            foreach (var endTransferableGood in endGoodStation.ReceivingGoods)
            {
              if (startTransferableGood.GoodId != endTransferableGood.GoodId)
                continue;

              var startInStock = startGoodsStation.Inventory.AmountInStock(startTransferableGood.GoodId);

              if (startInStock <= 5)
                continue;

              var endInStock = endGoodStation.Inventory.AmountInStock(endTransferableGood.GoodId);

              var desired = endGoodStation.MaxCapacity - endInStock;

              if (desired <= 5)
                continue;

              // Plugin.Log.LogError("Distribute: " + startInStock + " Desired: " + desired);
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
    
    // private void TakeGoods()
    // {
    //   _reachedPickupLocation = true;
    //   _goodReserver.UnreserveStock();
    //   _startGoodStation.Inventory.Take(_goodAmount);
    //   int wagonCount = _trainWagonManager.TrainWagons.Count;
    //   int remainingGoodAmount = _goodAmount.Amount;
    //   for (var index = 0; index < _trainWagonManager.TrainWagons.Count; index++)
    //   {
    //     var amount = (int)Math.Ceiling((float)(remainingGoodAmount / (wagonCount - index)));
    //     remainingGoodAmount -= amount;
    //     var trainWagon = _trainWagonManager.TrainWagons[index];
    //     trainWagon.GoodCarrier.PutGoodsInHands(new GoodAmount(_goodAmount.GoodId, amount));
    //   }
    //   _machinist.GoTo(_trainPositionDestinationFactory.Create(_endGoodStation.TrainDestinationComponent));
    // }
    //
    // private void DeliverGoods()
    // {
    //   var storage = (GoodRegistry)_chooChooCore.GetPrivateField(_endGoodStation.Inventory, "_storage");
    //   storage.Add(_goodAmount);
    //   foreach (var trainWagon in _trainWagonManager.TrainWagons)
    //     trainWagon.GoodCarrier.EmptyHands();
    //   _chooChooCore.InvokePrivateMethod(_endGoodStation.Inventory, "InvokeInventoryChangedEvent", new object[]{ _goodAmount.GoodId });
    //   _deliveredGoods = true;
    // }
  }
}
