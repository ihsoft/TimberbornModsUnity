using System.Reflection;
using Bindito.Core;
using HarmonyLib;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Persistence;
using Timberborn.TickSystem;

namespace ChooChoo
{
  public class DistributorTrain : TickableComponent
  {
    private TrainPositionDestinationFactory _trainPositionDestinationFactory;
    
    private Machinist _machinist;
    private GoodReserver _goodReserver;

    private bool _reachedPickupLocation;
    private bool _deliveredGoods = true;


    private GoodsStation _startGoodStation;
    private GoodsStation _endGoodStation;
    private GoodAmount _goodAmount;

    public bool CantDistributeGoods { get; set; }

    [Inject]
    public void InjectDependencies(TrainPositionDestinationFactory trainPositionDestinationFactory)
    {
      _trainPositionDestinationFactory = trainPositionDestinationFactory;
    }

    public void Awake()
    {
      _machinist = GetComponent<Machinist>();
      _goodReserver = GetComponent<GoodReserver>();
    }

    public bool Distribute(GoodsStation startGoodStation, GoodsStation endGoodStation, GoodAmount goodAmount)
    {
      _reachedPickupLocation = false;
      _deliveredGoods = false; 

      _startGoodStation = startGoodStation;
      _endGoodStation = endGoodStation;
      _goodAmount = goodAmount;

      if (startGoodStation.Inventory.UnreservedCapacity(goodAmount.GoodId) >= goodAmount.Amount)
      {
        _goodReserver.ReserveExactStockAmount(startGoodStation.Inventory, goodAmount);
      
        _machinist.GoTo(_trainPositionDestinationFactory.Create(startGoodStation.TrainDestinationComponent));
        return true;
      }

      return false;
    }

    public override void Tick()
    {
      if (Delivered())
        return;
      
      if (!_reachedPickupLocation)
      {
        if (_machinist.Stopped())
        {
          _reachedPickupLocation = true;
          _goodReserver.UnreserveStock();
          _startGoodStation.Inventory.Take(_goodAmount);
          _machinist.GoTo(_trainPositionDestinationFactory.Create(_endGoodStation.TrainDestinationComponent));
        }
      }
      
    
      if (_machinist.Stopped())
      {
        var test = (GoodRegistry)GetPrivateField(_endGoodStation.Inventory, "_storage");
        test.Add(_goodAmount);
        InvokePrivateMethod(_endGoodStation.Inventory, "InvokeInventoryChangedEvent", new object[]{ _goodAmount.GoodId });
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
    
    public object GetPrivateField(object instance, string fieldName)
    {
      return AccessTools.TypeByName(instance.GetType().Name).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
    }
    
    public object InvokePrivateMethod(object instance, string methodName, object[] args)
    {
      return AccessTools.TypeByName(instance.GetType().Name).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, args);
    }
  }
}
