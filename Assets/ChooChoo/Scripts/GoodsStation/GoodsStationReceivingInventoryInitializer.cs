using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  internal class GoodsStationReceivingInventoryInitializer : IDedicatedDecoratorInitializer<GoodsStation, Inventory>
  {
    private static readonly string InventoryComponentName = "GoodsStation";
    private readonly IGoodService _goodService;
    private readonly InventoryNeedBehaviorAdder _inventoryNeedBehaviorAdder;
    private readonly InventoryInitializerFactory _inventoryInitializerFactory;

    public GoodsStationReceivingInventoryInitializer(IGoodService goodService,InventoryNeedBehaviorAdder inventoryNeedBehaviorAdder, InventoryInitializerFactory inventoryInitializerFactory)
    {
      _goodService = goodService;
      _inventoryNeedBehaviorAdder = inventoryNeedBehaviorAdder;
      _inventoryInitializerFactory = inventoryInitializerFactory;
    }

    public void Initialize(GoodsStation subject, Inventory decorator)
    {
      InventoryInitializer unlimitedCapacity = _inventoryInitializerFactory.CreateWithUnlimitedCapacity(decorator, InventoryComponentName);
      AllowEveryGoodAsTakeable(unlimitedCapacity);
      unlimitedCapacity.HasPublicOutput();
      unlimitedCapacity.SetIgnorableCapacity();
      unlimitedCapacity.Initialize();
      subject.InitializeReceivingInventory(decorator);
      _inventoryNeedBehaviorAdder.AddNeedBehavior(decorator);
    }
    
    private void AllowEveryGoodAsTakeable(InventoryInitializer inventoryInitializer)
    {
      foreach (string good1 in _goodService.Goods)
      {
        StorableGoodAmount good2 = new StorableGoodAmount(StorableGood.CreateAsTakeable(good1), GoodsStation.Capacity);
        inventoryInitializer.AddAllowedGood(good2);
      }
    }
  }
}
