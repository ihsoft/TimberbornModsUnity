using Bindito.Core;
using System.Linq;
using Timberborn.BaseComponentSystem;
using Timberborn.Carrying;
using Timberborn.InventorySystem;
using Timberborn.Persistence;

namespace ChooChoo
{
  internal class DistributableGoodBringerTrain : BaseComponent
  {
    // private static readonly ComponentKey DistributableGoodBringerTrainKey = new(nameof(GoodsStation));
    // private static readonly PropertyKey<int> MinimumOfItemsToMoveKey = new("MinimumOfItemsToMove");
    private TrainDestinationService _trainDestinationService;
    private GoodsStationsRepository _goodsStationsRepository;
    private WagonGoodsManager _wagonGoodsManager;
    
    // public int MinimumOfItemsToMove { get; set; } = 19;

    [Inject]
    public void InjectDependencies(TrainDestinationService trainDestinationService, GoodsStationsRepository goodsStationsRepository, ChooChooCarryAmountCalculator chooChooCarryAmountCalculator)
    {
      _trainDestinationService = trainDestinationService;
      _goodsStationsRepository = goodsStationsRepository;
    }

    public void Awake()
    {
      _wagonGoodsManager = GetComponentFast<WagonGoodsManager>();
    }
    
    // public void Save(IEntitySaver entitySaver)
    // {
    //   entitySaver.GetComponent(DistributableGoodBringerTrainKey).Set(MinimumOfItemsToMoveKey, MinimumOfItemsToMove);
    // }
    //
    // public void Load(IEntityLoader entityLoader)
    // {
    //   if (!entityLoader.HasComponent(DistributableGoodBringerTrainKey))
    //     return;
    //   MinimumOfItemsToMove = entityLoader.GetComponent(DistributableGoodBringerTrainKey).Get(MinimumOfItemsToMoveKey);
    // }

    public bool BringDistributableGoods()
    {
      Plugin.Log.LogInfo("Looking to move goods");
      var reachableGoodStation = _goodsStationsRepository.GoodsStations.FirstOrDefault(station => _trainDestinationService.DestinationReachableOneWay(TransformFast.position, station.TrainDestinationComponent));
      if (reachableGoodStation == null)
        return false;
      
      var reachableGoodStations = _goodsStationsRepository.GoodsStations.Where(station => _trainDestinationService.TrainDestinationsConnectedBothWays(reachableGoodStation.TrainDestinationComponent, station.TrainDestinationComponent)).ToArray();

      foreach (var goodsStation in reachableGoodStations)
      {
        var goods = goodsStation.SendingQueue;
        Plugin.Log.LogInfo("Any: " + goods.Any());
        foreach (var trainDistributableGood in goods)
        {
          if (_wagonGoodsManager.IsFullOrReserved)
            break;
          // Plugin.Log.LogInfo(trainDistributableGood.ResolvingTrainWagons.Count + " ResolvingTrainWagon");
          _wagonGoodsManager.TryReservingGood(trainDistributableGood, goodsStation.SendingInventory);
        }
      }

      if (_wagonGoodsManager.IsCarryingOrReserved)
      {
        Plugin.Log.LogInfo("Found goods to move");
        return true;
      }

      Plugin.Log.LogInfo("Didnt find goods to move");
      return false;
    }
  }
}
