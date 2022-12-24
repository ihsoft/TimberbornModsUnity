using Bindito.Unity;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  internal class GoodsStationInventoryInitializer : IDedicatedDecoratorInitializer<GoodsStation, Inventory>
  {
    private static readonly string InventoryComponentName = "GoodsStation";
    private readonly IGoodService _goodService;
    private readonly IInstantiator _instantiator;

    public GoodsStationInventoryInitializer(IGoodService goodService, IInstantiator instantiator)
    {
      _goodService = goodService;
      _instantiator = instantiator;
    }

    public void Initialize(GoodsStation subject, Inventory decorator)
    {
      InventoryInitializer inventoryInitializer = new InventoryInitializer(_goodService, decorator, CalculateTotalCapacity(subject), InventoryComponentName);
      inventoryInitializer.HasPublicInput();
      inventoryInitializer.HasPublicOutput();
      AllowEveryGoodAsGiveAndTabeable(inventoryInitializer, subject.MaxCapacity);
      LimitableGoodDisallower limitableGoodDisallower = _instantiator.AddComponent<LimitableGoodDisallower>(subject.gameObject);
      inventoryInitializer.AddGoodDisallower(limitableGoodDisallower);
      inventoryInitializer.Initialize();
      subject.InitializeInventory(decorator);
    }

    private int CalculateTotalCapacity(GoodsStation simpleOutputInventory) => simpleOutputInventory.MaxCapacity * _goodService.Goods.Count;

    private void AllowEveryGoodAsGiveAndTabeable(InventoryInitializer inventoryInitializer, int inventoryCapacity)
    {
      foreach (string goodId in _goodService.Goods)
      {
        StorableGoodAmount storableGoodAmount = new StorableGoodAmount(StorableGood.CreateGiveableAndTakeable(goodId), inventoryCapacity);
        inventoryInitializer.AddAllowedGood(storableGoodAmount);
      }
    }
  }
}
