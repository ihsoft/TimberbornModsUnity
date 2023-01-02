using Bindito.Unity;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  internal class TrainYardInventoryInitializer : IDedicatedDecoratorInitializer<TrainYard, Inventory>
  {
    private static readonly string InventoryComponentName = "TrainYard";
    private readonly IGoodService _goodService;
    private readonly IInstantiator _instantiator;
    
    public TrainYardInventoryInitializer(IGoodService goodService, IInstantiator instantiator)
    {
      _goodService = goodService;
      _instantiator = instantiator;
    }

    public void Initialize(TrainYard subject, Inventory decorator)
    {
      InventoryInitializer inventoryInitializer = new InventoryInitializer(_goodService, decorator, CalculateTotalCapacity(subject), InventoryComponentName);
      inventoryInitializer.HasPublicInput();
      inventoryInitializer.HasPublicOutput();
      AllowEveryGoodAsGiveAndTabeable(inventoryInitializer, subject.TrainCost);
      // LimitableGoodDisallower limitableGoodDisallower = _instantiator.AddComponent<LimitableGoodDisallower>(subject.gameObject);
      // inventoryInitializer.AddGoodDisallower(limitableGoodDisallower);
      inventoryInitializer.Initialize();
      subject.InitializeInventory(decorator);
    }

    private int CalculateTotalCapacity(TrainYard trainYard) => trainYard.MaxCapacity * _goodService.Goods.Count;

    private void AllowEveryGoodAsGiveAndTabeable(InventoryInitializer inventoryInitializer, GoodAmountSpecification[] inventoryCapacity)
    {
      foreach (var goodAmountSpecification in inventoryCapacity)
      {
        StorableGoodAmount storableGoodAmount = new StorableGoodAmount(StorableGood.CreateGiveableAndTakeable(goodAmountSpecification.GoodId), goodAmountSpecification.Amount * 2);
        inventoryInitializer.AddAllowedGood(storableGoodAmount);
      }
    }
  }
}
