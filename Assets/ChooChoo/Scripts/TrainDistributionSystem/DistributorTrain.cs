using System;
using System.Linq;
using System.Reflection;
using Bindito.Core;
using HarmonyLib;
using Timberborn.BehaviorSystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Persistence;
using Timberborn.TickSystem;

namespace ChooChoo
{
  public class DistributorTrain : TickableComponent
  {
    private ChooChooCore _chooChooCore;
    private TrainPositionDestinationFactory _trainPositionDestinationFactory;
    
    private Machinist _machinist;
    private GoodReserver _goodReserver;
    private TrainWagonManager _trainWagonManager;

    private bool _reachedPickupLocation = true;
    private bool _deliveredGoods = true;

    private GoodsStation _startGoodStation;
    private GoodsStation _endGoodStation;
    private GoodAmount _goodAmount;

    [Inject]
    public void InjectDependencies(ChooChooCore chooChooCore, TrainPositionDestinationFactory trainPositionDestinationFactory)
    {
      _chooChooCore = chooChooCore;
      _trainPositionDestinationFactory = trainPositionDestinationFactory;
    }

    public void Awake()
    {
      _machinist = GetComponent<Machinist>();
      _goodReserver = GetComponent<GoodReserver>();
      _trainWagonManager = GetComponent<TrainWagonManager>();
    }

    public ExecutorStatus Distribute(GoodsStation startGoodStation, GoodsStation endGoodStation, GoodAmount goodAmount)
    {
      if (startGoodStation.Inventory.UnreservedAmountInStock(goodAmount.GoodId) < goodAmount.Amount)
        return ExecutorStatus.Failure;
      
      _reachedPickupLocation = false;
      _deliveredGoods = false; 

      _startGoodStation = startGoodStation;
      _endGoodStation = endGoodStation;
      _goodAmount = goodAmount;
        
      _goodReserver.ReserveExactStockAmount(startGoodStation.Inventory, goodAmount);
      
      _machinist.GoTo(_trainPositionDestinationFactory.Create(startGoodStation.TrainDestinationComponent));
      return ExecutorStatus.Running;
    }

    public override void Tick()
    {
      if (Delivered())
        return;

      // if (HasGoods() && _machinist.Stopped())
      //   DeliverGoods();
      
      if (!_reachedPickupLocation && _machinist.Stopped())
        TakeGoods();

      if (_reachedPickupLocation && _machinist.Stopped())
        DeliverGoods();
    }

    public bool Delivered() => _deliveredGoods;

    private bool HasGoods()
    {
      var flag = false;
      foreach (var trainWagon in _trainWagonManager.TrainWagons)
      {
        if (trainWagon.GoodCarrier.IsCarrying)
        {
          flag = true;
        }
      }

      return flag;
    }
    
    private void TakeGoods()
    {
      _reachedPickupLocation = true;
      _goodReserver.UnreserveStock();
      _startGoodStation.Inventory.Take(_goodAmount);
      int wagonCount = _trainWagonManager.TrainWagons.Count;
      int remainingGoodAmount = _goodAmount.Amount;
      for (var index = 0; index < _trainWagonManager.TrainWagons.Count; index++)
      {
        var amount = (int)Math.Ceiling((float)(remainingGoodAmount / (wagonCount - index)));
        remainingGoodAmount -= amount;
        var trainWagon = _trainWagonManager.TrainWagons[index];
        trainWagon.GoodCarrier.PutGoodsInHands(new GoodAmount(_goodAmount.GoodId, amount));
      }
      _machinist.GoTo(_trainPositionDestinationFactory.Create(_endGoodStation.TrainDestinationComponent));
    }

    private void DeliverGoods()
    {
      var storage = (GoodRegistry)_chooChooCore.GetPrivateField(_endGoodStation.Inventory, "_storage");
      storage.Add(_goodAmount);
      foreach (var trainWagon in _trainWagonManager.TrainWagons)
        trainWagon.GoodCarrier.EmptyHands();
      _chooChooCore.InvokePrivateMethod(_endGoodStation.Inventory, "InvokeInventoryChangedEvent", new object[]{ _goodAmount.GoodId });
      _deliveredGoods = true;
    }
  }
}
