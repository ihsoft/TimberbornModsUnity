using System;
using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using ChooChoo.Scripts.GoodsStation;
using Timberborn.BaseComponentSystem;
using Timberborn.Carrying;
using Timberborn.Common;
using Timberborn.ConstructibleSystem;
using Timberborn.DistributionSystem;
using Timberborn.EntitySystem;
using Timberborn.GameDistricts;
using Timberborn.InventorySystem;
using Timberborn.Persistence;
using UnityEngine;

namespace ChooChoo
{
  public class GoodsStation : BaseComponent, IRegisteredComponent, IFinishedStateListener, IPersistentEntity
  {
    private static readonly ComponentKey GoodsStationKey = new(nameof(GoodsStation));
    private static readonly ListKey<TrainDistributableGoodAmount> SendingQueueKey = new("SendingQueue");
    
    public static readonly int Capacity = 200;
    private TrainDistributableGoodObjectSerializer _trainDistributableGoodObjectSerializer;
    private GoodsStationsRepository _goodsStationsRepository;

    private GoodsStationReceivingInventory _goodsStationReceivingInventory;
    private GoodsStationSendingInventory _goodsStationSendingInventory;

    private GoodsStationDistributableGoodProvider _goodsStationDistributableGoodProvider;
    
    public TrainDestination TrainDestinationComponent { get; private set; }

    public Inventory SendingInventory => _goodsStationSendingInventory.Inventory;
    public Inventory ReceivingInventory => _goodsStationReceivingInventory.Inventory;
    public GoodsStationDistributableGoodProvider GoodsStationDistributableGoodProvider => _goodsStationDistributableGoodProvider;
    
    public int MaxCapacity => Capacity;

    public readonly List<TrainDistributableGoodAmount> SendingQueue = new();

    [Inject]
    public void InjectDependencies(TrainDistributableGoodObjectSerializer trainDistributableGoodObjectSerializer, GoodsStationsRepository goodsStationsRepository)
    {
      _trainDistributableGoodObjectSerializer = trainDistributableGoodObjectSerializer;
      _goodsStationsRepository = goodsStationsRepository;
    }

    public bool CanDistribute => (bool) (UnityEngine.Object) GoodsStationDistributableGoodProvider;
    
    public void Awake()
    {
      _goodsStationDistributableGoodProvider = GetComponentFast<GoodsStationDistributableGoodProvider>();
      TrainDestinationComponent = GetComponentFast<TrainDestination>();
      _goodsStationReceivingInventory = GetComponentFast<GoodsStationReceivingInventory>();
      _goodsStationSendingInventory = GetComponentFast<GoodsStationSendingInventory>();
      enabled = false;
    }

    public void OnEnterFinishedState()
    {
      enabled = true;
      SendingInventory.Enable();
      ReceivingInventory.Enable();
      _goodsStationsRepository.Register(this);
    }

    public void OnExitFinishedState()
    {
      SendingInventory.Disable();
      ReceivingInventory.Disable();
      enabled = false;
      _goodsStationsRepository.UnRegister(this);
    }
    
    public void Save(IEntitySaver entitySaver)
    {
      entitySaver.GetComponent(GoodsStationKey).Set(SendingQueueKey, SendingQueue, _trainDistributableGoodObjectSerializer);
    }

    public void Load(IEntityLoader entityLoader)
    {
      if (!entityLoader.HasComponent(GoodsStationKey))
        return;
      if (!entityLoader.GetComponent(GoodsStationKey).Has(SendingQueueKey))
        return;
      foreach (var trainDistributableGood in entityLoader.GetComponent(GoodsStationKey).Get(SendingQueueKey, _trainDistributableGoodObjectSerializer))
      {
        AddToQueue(trainDistributableGood);
      }
    }

    public void AddToQueue(TrainDistributableGoodAmount newDistributableGoodAmount)
    {
      SendingQueue.Add(newDistributableGoodAmount);
      // Plugin.Log.LogError("Agent: " + (newDistributableGoodAmount.Agent == null) + " "  + (newDistributableGoodAmount.Agent != null ? newDistributableGoodAmount.Agent.name : ""));
      Plugin.Log.LogError("Good id: " + newDistributableGoodAmount.GoodAmount.GoodId + " Amount: " + newDistributableGoodAmount.GoodAmount.Amount);
      Plugin.Log.LogError("Please report if number keeps increasing: " + SendingQueue.Count);
    }

    public void OnDeliveryCompletedEvent(CarryRootBehavior agent)
    {
      foreach (var trainDistributableGoodAmount in SendingQueue.ToList())
      {
        if (trainDistributableGoodAmount.TryDelivering(agent))
          break;
      }
    }
    
    public void OnEmptyingHandsEvent(CarryRootBehavior agent)
    {
      foreach (var trainDistributableGoodAmount in SendingQueue.ToList())
      {
        if (trainDistributableGoodAmount.TryDeliveryFailed(agent)) 
          SendingQueue.Remove(trainDistributableGoodAmount);
      }
    }

    public void ResolveRetrieval(TrainDistributableGoodAmount trainDistributableGoodAmount)
    {
      SendingQueue.Remove(trainDistributableGoodAmount);
    }
    
    public TrainDistributableGood GetMyDistributableGood(string goodId) => GoodsStationDistributableGoodProvider.GetDistributableGoodForExport(goodId);
    
    public bool CanExport(TrainDistributableGood myTrainDistributableGood)
    {
      return myTrainDistributableGood.CanExport;
    }
    
    public int GetAmountToExport(TrainDistributableGood myTrainDistributableGood, TrainDistributableGood linkedTrainDistributableGood)
    {
      return myTrainDistributableGood.Capacity <= 0 ? Math.Max(0, linkedTrainDistributableGood.FreeCapacity) : GetAmountToEqualizeFillRates(myTrainDistributableGood, linkedTrainDistributableGood);
    }
    private static int GetAmountToEqualizeFillRates(
      TrainDistributableGood myTrainDistributableGood,
      TrainDistributableGood linkedTrainDistributableGood)
    {
      float num = myTrainDistributableGood.FillRate - linkedTrainDistributableGood.FillRate;
      int capacity1 = myTrainDistributableGood.Capacity;
      int capacity2 = linkedTrainDistributableGood.Capacity;
      return Mathf.FloorToInt(Mathf.Min((float) (capacity1 * capacity2) * num / (float) (capacity1 + capacity2), myTrainDistributableGood.MaxExportAmount));
    }
  }
}
