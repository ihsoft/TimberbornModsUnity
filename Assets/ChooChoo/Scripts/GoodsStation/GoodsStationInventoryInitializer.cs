using Bindito.Unity;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  internal class GoodsStationInventoryInitializer : IDedicatedDecoratorInitializer<GoodsStation, Inventory>
  {
    private static readonly string InventoryComponentName = "GoodsStationInventory";
    private readonly IGoodService _goodService;
    private readonly IInstantiator _instantiator;

    public GoodsStationInventoryInitializer(
      IGoodService goodService,
      IInstantiator instantiator)
    {
      _goodService = goodService;
      _instantiator = instantiator;
    }

    public void Initialize(GoodsStation subject, Inventory decorator)
    {
      int maxValue = int.MaxValue;
      InventoryInitializer inventoryInitializer = new InventoryInitializer(_goodService, decorator, maxValue, InventoryComponentName);
      inventoryInitializer.HasPublicInput();
      AllowEveryGoodAsGivableAndTakeable(inventoryInitializer, maxValue);
      LimitableGoodDisallower limitableGoodDisallower = _instantiator.AddComponent<LimitableGoodDisallower>(subject.gameObject);
      inventoryInitializer.AddGoodDisallower(limitableGoodDisallower);
      inventoryInitializer.Initialize();
      subject.InitializeInventory(decorator);
    }

    private void AllowEveryGoodAsGivableAndTakeable(
      InventoryInitializer inventoryInitializer,
      int inventoryCapacity)
    {
      foreach (string good1 in _goodService.Goods)
      {
        StorableGoodAmount good2 = new StorableGoodAmount(StorableGood.CreateGiveableAndTakeable(good1), inventoryCapacity);
        inventoryInitializer.AddAllowedGood(good2);
      }
    }
  }
}
