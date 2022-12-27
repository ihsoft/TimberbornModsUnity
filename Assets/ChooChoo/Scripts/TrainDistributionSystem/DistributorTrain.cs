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
      
      if (!_reachedPickupLocation && _machinist.Stopped())
      {
        _reachedPickupLocation = true;
        _goodReserver.UnreserveStock();
        _startGoodStation.Inventory.Take(_goodAmount);
        _machinist.GoTo(_trainPositionDestinationFactory.Create(_endGoodStation.TrainDestinationComponent));
      }
      
      if (_reachedPickupLocation && _machinist.Stopped())
      {
        var storage = (GoodRegistry)_chooChooCore.GetPrivateField(_endGoodStation.Inventory, "_storage");
        storage.Add(_goodAmount);
        _chooChooCore.InvokePrivateMethod(_endGoodStation.Inventory, "InvokeInventoryChangedEvent", new object[]{ _goodAmount.GoodId });
        _deliveredGoods = true;
      }
    }

    public bool Delivered() => _deliveredGoods;

    public void Save(IEntitySaver entitySaver)
    {
    }

    public void Load(IEntityLoader entityLoader)
    {
    }
  }
}
