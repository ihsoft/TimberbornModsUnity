﻿using ChooChoo.Scripts.GoodsStation;
using Timberborn.BaseComponentSystem;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  internal class GoodsStationReceivingInventoryInitializer : IDedicatedDecoratorInitializer<GoodsStationReceivingInventory, Inventory>
  {
    private static readonly string InventoryComponentName = nameof(GoodsStationReceivingInventory);
    private readonly IGoodService _goodService;
    private readonly InventoryNeedBehaviorAdder _inventoryNeedBehaviorAdder;
    private readonly InventoryInitializerFactory _inventoryInitializerFactory;
    private readonly BaseInstantiator _baseInstantiator;

    public GoodsStationReceivingInventoryInitializer(
      IGoodService goodService,
      InventoryNeedBehaviorAdder inventoryNeedBehaviorAdder, 
      InventoryInitializerFactory inventoryInitializerFactory, 
      BaseInstantiator baseInstantiator)
    {
      _goodService = goodService;
      _inventoryNeedBehaviorAdder = inventoryNeedBehaviorAdder;
      _inventoryInitializerFactory = inventoryInitializerFactory;
      _baseInstantiator = baseInstantiator;
    }

    public void Initialize(GoodsStationReceivingInventory subject, Inventory decorator)
    {
      InventoryInitializer unlimitedCapacity = _inventoryInitializerFactory.CreateWithUnlimitedCapacity(decorator, InventoryComponentName);
      AllowEveryGoodAsTakeable(unlimitedCapacity);
      unlimitedCapacity.HasPublicOutput();
      unlimitedCapacity.SetIgnorableCapacity();
      LimitableGoodDisallower singleGoodAllower = _baseInstantiator.AddComponent<LimitableGoodDisallower>(subject.GameObjectFast);
      unlimitedCapacity.AddGoodDisallower(singleGoodAllower);
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
