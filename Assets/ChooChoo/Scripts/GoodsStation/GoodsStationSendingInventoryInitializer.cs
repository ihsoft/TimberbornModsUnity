using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  internal class GoodsStationSendingInventoryInitializer : IDedicatedDecoratorInitializer<GoodsStation, Inventory>
  {
    private static readonly string InventoryComponentName = "GoodsStation";
    private readonly IGoodService _goodService;
    private readonly InventoryNeedBehaviorAdder _inventoryNeedBehaviorAdder;
    private readonly InventoryInitializerFactory _inventoryInitializerFactory;

    public GoodsStationSendingInventoryInitializer(IGoodService goodService,InventoryNeedBehaviorAdder inventoryNeedBehaviorAdder, InventoryInitializerFactory inventoryInitializerFactory)
    {
      _goodService = goodService;
      _inventoryNeedBehaviorAdder = inventoryNeedBehaviorAdder;
      _inventoryInitializerFactory = inventoryInitializerFactory;
    }

    public void Initialize(GoodsStation subject, Inventory decorator)
    {
      InventoryInitializer unlimitedCapacity = _inventoryInitializerFactory.CreateWithUnlimitedCapacity(decorator, InventoryComponentName);
      AllowEveryGoodAsGiveAble(unlimitedCapacity);
      unlimitedCapacity.HasPublicOutput();
      unlimitedCapacity.SetIgnorableCapacity();
      unlimitedCapacity.Initialize();
      subject.InitializeSendingInventory(decorator);
      _inventoryNeedBehaviorAdder.AddNeedBehavior(decorator);
    }
    
    private void AllowEveryGoodAsGiveAble(InventoryInitializer inventoryInitializer)
    {
      foreach (string good1 in _goodService.Goods)
      {
        StorableGoodAmount good2 = new StorableGoodAmount(StorableGood.CreateAsGivable(good1), GoodsStation.Capacity);
        inventoryInitializer.AddAllowedGood(good2);
      }
    }
  }
}
