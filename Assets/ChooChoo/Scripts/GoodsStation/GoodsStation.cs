using System;
using System.Collections.Generic;
using System.Linq;
using Bindito.Core;
using ChooChoo.Scripts.GoodsStation;
using Timberborn.BaseComponentSystem;
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
    private static readonly ListKey<TrainDistributableGood> SendingQueueKey = new("SendingQueue");
    
    public static readonly int Capacity = 200;
    private TrainDistributableGoodObjectSerializer _trainDistributableGoodObjectSerializer;
    private GoodsStationsRepository _goodsStationsRepository;

    private GoodsStationReceivingInventory _goodsStationReceivingInventory;
    private GoodsStationSendingInventory _goodsStationSendingInventory;
    private DistrictBuilding _districtBuilding;

    private DistrictDistributableGoodProvider _districtDistributableGoodProvider;
    
    public TrainDestination TrainDestinationComponent { get; private set; }

    public Inventory SendingInventory => _goodsStationSendingInventory.Inventory;
    public Inventory ReceivingInventory => _goodsStationReceivingInventory.Inventory;
    public DistrictDistributableGoodProvider DistrictDistributableGoodProvider => _districtDistributableGoodProvider;
    
    public int MaxCapacity => Capacity;

    public readonly List<TrainDistributableGood> SendingQueue = new();

    [Inject]
    public void InjectDependencies(TrainDistributableGoodObjectSerializer trainDistributableGoodObjectSerializer, GoodsStationsRepository goodsStationsRepository)
    {
      _trainDistributableGoodObjectSerializer = trainDistributableGoodObjectSerializer;
      _goodsStationsRepository = goodsStationsRepository;
    }

    public bool CanDistribute => (bool) (UnityEngine.Object) DistrictDistributableGoodProvider;
    
    public void Awake() 
    {
      TrainDestinationComponent = GetComponentFast<TrainDestination>();
      _goodsStationReceivingInventory = GetComponentFast<GoodsStationReceivingInventory>();
      _goodsStationSendingInventory = GetComponentFast<GoodsStationSendingInventory>();
      _districtBuilding = GetComponentFast<DistrictBuilding>();
      enabled = false;
    }

    public void OnEnterFinishedState()
    {
      enabled = true;
      SendingInventory.Enable();
      ReceivingInventory.Enable();
      _districtBuilding.ReassignedDistrict += OnReassignedDistrict;
      _goodsStationsRepository.Register(this);
    }

    public void OnExitFinishedState()
    {
      SendingInventory.Disable();
      ReceivingInventory.Disable();
      enabled = false;
      _districtBuilding.ReassignedDistrict -= OnReassignedDistrict;
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
    
    private void OnReassignedDistrict(object sender, EventArgs e)
    {
      _districtDistributableGoodProvider = _districtBuilding.District ? _districtBuilding.District.GetComponentFast<DistrictDistributableGoodProvider>() : null;
    }

    public void AddToQueue(TrainDistributableGood newDistributableGood)
    {
      SendingQueue.Add(newDistributableGood);
    }

    public void ResolveCorrespondingQueueItems(TrainWagon trainWagon)
    {
      foreach (var trainDistributableGood in SendingQueue.ToList())
      {
        if (trainDistributableGood.ResolvingTrainWagons.Contains(trainWagon))
        {
          SendingQueue.Remove(trainDistributableGood);
        }
      }
    }
    
    public DistributableGood GetMyDistributableGood(string goodId) => DistrictDistributableGoodProvider.GetDistributableGoodForExport(goodId);
    
    public bool CanExport(
      DistributableGood myDistributableGood,
      DistributableGood linkedDistributableGood)
    {
      return myDistributableGood.CanExport && (double) myDistributableGood.FillRate > (double) linkedDistributableGood.FillRate;
    }
    
    public int GetAmountToExport(
      DistributableGood myDistributableGood,
      DistributableGood linkedDistributableGood)
    {
      return myDistributableGood.Capacity <= 0 ? Math.Max(0, linkedDistributableGood.FreeCapacity) : GetAmountToEqualizeFillRates(myDistributableGood, linkedDistributableGood);
    }
    private static int GetAmountToEqualizeFillRates(
      DistributableGood myDistributableGood,
      DistributableGood linkedDistributableGood)
    {
      float num = myDistributableGood.FillRate - linkedDistributableGood.FillRate;
      int capacity1 = myDistributableGood.Capacity;
      int capacity2 = linkedDistributableGood.Capacity;
      return Mathf.FloorToInt(Mathf.Min((float) (capacity1 * capacity2) * num / (float) (capacity1 + capacity2), myDistributableGood.MaxExportAmount));
    }
  }
}
