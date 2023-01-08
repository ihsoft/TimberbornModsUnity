using Bindito.Core;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BlockSystem;
using Timberborn.Carrying;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  internal class DistributableGoodBringerTrain : MonoBehaviour, IPersistentEntity
  {
    private static readonly ComponentKey DistributableGoodBringerTrainKey = new(nameof(GoodsStation));
    private static readonly PropertyKey<int> MinimumOfItemsToMoveKey = new("MinimumOfItemsToMove");
    private TrainDestinationService _trainDestinationService;
    private GoodsStationsRepository _goodsStationsRepository;
    private GoodsStationService _goodsStationService;
    private BlockService _blockService;
    private ChooChooCore _chooChooCore;
    private TrainCarryAmountCalculator _trainCarryAmountCalculator;
    private GoodCarrier _goodCarrier;
    private GoodReserver _goodReserver;
    private readonly List<GoodAmount> _sortedLackingGoods = new();
    
    public int MinimumOfItemsToMove { get; set; } = 19;

    [Inject]
    public void InjectDependencies(TrainDestinationService trainDestinationService, GoodsStationsRepository goodsStationsRepository, GoodsStationService goodsStationService, BlockService blockService, ChooChooCore chooChooCore, TrainCarryAmountCalculator trainCarryAmountCalculator)
    {
      _trainDestinationService = trainDestinationService;
      _goodsStationsRepository = goodsStationsRepository;
      _goodsStationService = goodsStationService;
      _blockService = blockService;
      _chooChooCore = chooChooCore;
      _trainCarryAmountCalculator = trainCarryAmountCalculator;
    }

    public void Awake()
    {
      _goodCarrier = GetComponent<GoodCarrier>();
      _goodReserver = GetComponent<GoodReserver>();
    }
    
    public void Save(IEntitySaver entitySaver)
    {
      entitySaver.GetComponent(DistributableGoodBringerTrainKey).Set(MinimumOfItemsToMoveKey, MinimumOfItemsToMove);
    }

    public void Load(IEntityLoader entityLoader)
    {
      if (!entityLoader.HasComponent(DistributableGoodBringerTrainKey))
        return;
      MinimumOfItemsToMove = entityLoader.GetComponent(DistributableGoodBringerTrainKey).Get(MinimumOfItemsToMoveKey);
    }

    public bool BringDistributableGoods()
    {
      // Plugin.Log.LogInfo("Looking to move goods");

      var reachableGoodStation = _goodsStationsRepository.GoodsStations.FirstOrDefault(station => _trainDestinationService.DestinationReachable(transform.position, station.TrainDestinationComponent));

      if (reachableGoodStation == null)
        return false;
      
      var reachableGoodStations = _goodsStationsRepository.GoodsStations.Where(station => _trainDestinationService.TrainDestinationsConnected(reachableGoodStation.TrainDestinationComponent, station.TrainDestinationComponent)).ToArray();
      
      foreach (var deliverableGoodsStation in reachableGoodStations)
      {
        // Plugin.Log.LogError(deliverableGoodsStation.transform.position + "");
        UpdateLackingGoods(deliverableGoodsStation, false);
        
        // Plugin.Log.LogError(_sortedLackingGoods.Count + "");
        foreach (GoodAmount sortedLackingGood in _sortedLackingGoods)
        {
          Inventory inventory = GoodsStationWithStock(deliverableGoodsStation, reachableGoodStations, sortedLackingGood.GoodId);
          
          // Plugin.Log.LogError((bool)(UnityEngine.Object)inventory + "");
          if ((bool)(Object)inventory)
          {
            GoodAmount carry = _trainCarryAmountCalculator.AmountToCarry(_goodCarrier.LiftingCapacity, MaxTakeableAmount(inventory, sortedLackingGood));
            // GoodAmount carry = _trainCarryAmountCalculator.AmountToCarry(_goodCarrier.LiftingCapacity, sortedLackingGood, deliverableGoodsStation.Inventory);
            // Plugin.Log.LogError(carry.Amount + "");

            if (carry.Amount > 0)
            {
              Reserve(deliverableGoodsStation, carry, inventory);
              
              // Plugin.Log.LogInfo("Found goods to move");
              return true;
            }
          }
        }
      }
      
      // Plugin.Log.LogInfo("Didnt find goods to move");
      
      return false;
    }

    private void UpdateLackingGoods(GoodsStation distributionPost, bool isSendingGood)
    {
      _sortedLackingGoods.Clear();
      // goodsStation.LackingGoods(_sortedLackingGoods);
      var transferableGoods = isSendingGood ? distributionPost.SendingGoods : distributionPost.ReceivingGoods;
      
      foreach (TransferableGood transferableGood in transferableGoods)
      {
        string goodId = transferableGood.GoodId;
        // Plugin.Log.LogInfo(goodId);
        // Plugin.Log.LogWarning("MaxAllowedAmount: " + distributionPost.MaxAllowedAmount(goodId) + " UnreservedAmountInStockAndIncoming: " + distributionPost.Inventory.UnreservedAmountInStockAndIncoming(goodId) + " UnreservedCapacity: " + distributionPost.Inventory.UnreservedCapacity(goodId));
        // int amount = Mathf.Min(Mathf.Max(goodsStation.MaxAllowedAmount(goodId) - goodsStation.Inventory.UnreservedAmountInStockAndIncoming(goodId), 0), goodsStation.Inventory.UnreservedCapacity(goodId));
        int amount = Mathf.Max(distributionPost.MaxAllowedAmount(goodId) - distributionPost.Inventory.UnreservedAmountInStockAndIncoming(goodId), 0);
        
        // Plugin.Log.LogInfo("Found amount: " + amount);
        if (amount > MinimumOfItemsToMove)
        {
          _sortedLackingGoods.Add(new GoodAmount(goodId, amount));
        }
      }
      _sortedLackingGoods.Sort((x, y) => CompareLackingGoods(distributionPost, x, y));
    }

    private int CompareLackingGoods(
      GoodsStation goodsStation,
      GoodAmount x,
      GoodAmount y)
    {
      float num = LackingGoodPriority(goodsStation, x);
      return LackingGoodPriority(goodsStation, y).CompareTo(num);
    }

    private float LackingGoodPriority(
      GoodsStation goodsStation,
      GoodAmount goodAmount)
    {
      return (float) goodAmount.Amount / (float) goodsStation.MaxAllowedAmount(goodAmount.GoodId);
    }

    private Inventory GoodsStationWithStock(GoodsStation deliverableGoodsStation, GoodsStation[] reachableGoodsStations, string goodId)
    {
      foreach (var goodsStation in reachableGoodsStations)
      {
        if (goodsStation == deliverableGoodsStation)
          continue;
        if (goodsStation.IsSending(goodId) && goodsStation.Inventory.AmountInStock(goodId) > MinimumOfItemsToMove)
          return goodsStation.Inventory;
      }

      return null;
    }
    
    private GoodAmount MaxTakeableAmount(
      Inventory inventory,
      GoodAmount lackingGood)
    {
      int amount = Mathf.Min(inventory.UnreservedAmountInStock(lackingGood.GoodId), lackingGood.Amount);
      return new GoodAmount(lackingGood.GoodId, amount);
    }

    private void Reserve(
      GoodsStation goodsStation,
      GoodAmount carriableGood,
      Inventory closestInventory)
    {
      _goodReserver.ReserveExactStockAmount(closestInventory, carriableGood);
      // _goodReserver.ReserveCapacity(goodsStation.Inventory, carriableGood);
      _goodReserver.UnreserveCapacity();
      _chooChooCore.SetPrivateProperty(_goodReserver, "CapacityReservation", new GoodReservation(goodsStation.Inventory, carriableGood, true));
    }
  }
}
