using Bindito.Core;
using System.Linq;
using Timberborn.BaseComponentSystem;

namespace ChooChoo
{
  internal class DistributableGoodBringerTrain : BaseComponent
  {
    private TrainDestinationService _trainDestinationService;
    private GoodsStationsRepository _goodsStationsRepository;
    private TrainWagonsGoodsManager _trainWagonsGoodsManager;

    [Inject]
    public void InjectDependencies(TrainDestinationService trainDestinationService, GoodsStationsRepository goodsStationsRepository, ChooChooCarryAmountCalculator chooChooCarryAmountCalculator)
    {
      _trainDestinationService = trainDestinationService;
      _goodsStationsRepository = goodsStationsRepository;
    }

    public void Awake()
    {
      _trainWagonsGoodsManager = GetComponentFast<TrainWagonsGoodsManager>();
    }

    public bool BringDistributableGoods()
    {
      // Plugin.Log.LogInfo("Looking to move goods");
      var reachableGoodStation = _goodsStationsRepository.GoodsStations.FirstOrDefault(station => _trainDestinationService.DestinationReachableOneWay(TransformFast.position, station.TrainDestinationComponent));
      if (reachableGoodStation == null)
        return false;

      return BringFromSpecificStation(reachableGoodStation);
    }
      
    public bool BringFromSpecificStation(GoodsStation reachableGoodStation)
    {
      var reachableGoodStations = _goodsStationsRepository.GoodsStations.Where(station => _trainDestinationService.TrainDestinationsConnectedBothWays(reachableGoodStation.TrainDestinationComponent, station.TrainDestinationComponent)).ToArray();

      foreach (var goodsStation in reachableGoodStations)
      {
        var goods = goodsStation.SendingQueue.Where(distributable => distributable.BeaverDeliveryCompleted).ToList();
        // Plugin.Log.LogInfo("Any items to send: " + goods.Any());
        foreach (var trainDistributableGood in goods)
        {
          if (_trainWagonsGoodsManager.IsFullOrReserved)
            break;
          _trainWagonsGoodsManager.TryReservingGood(trainDistributableGood, goodsStation);
        }
      }

      if (_trainWagonsGoodsManager.IsCarryingOrReserved)
      {
        // Plugin.Log.LogInfo("Found goods to move");
        return true;
      }

      // Plugin.Log.LogInfo("Didnt find goods to move");
      return false;
    }
  }
}
