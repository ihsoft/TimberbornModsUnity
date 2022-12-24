using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.Emptying;
using Timberborn.Hauling;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class GoodsStationConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<GoodsStationInventoryInitializer>().AsSingleton();
      containerDefinition.Bind<GoodsStationsRepository>().AsSingleton();
      containerDefinition.Bind<TransferableGoodObjectSerializer>().AsSingleton();
      containerDefinition.MultiBind<TemplateModule>().ToProvider<TemplateModuleProvider>().AsSingleton();
    }

    private class TemplateModuleProvider : IProvider<TemplateModule>
    {
      private readonly GoodsStationInventoryInitializer _goodsStationErrorInventoryInitializer;

      public TemplateModuleProvider(
        GoodsStationInventoryInitializer goodsStationErrorInventoryInitializer)
      {
        _goodsStationErrorInventoryInitializer = goodsStationErrorInventoryInitializer;
      }

      public TemplateModule Get()
      {
        TemplateModule.Builder builder = new TemplateModule.Builder();
        builder.AddDecorator<GoodsStation, GoodsStationDescriber>();
        builder.AddDedicatedDecorator(_goodsStationErrorInventoryInitializer);
        builder.AddDecorator<GoodsStation, Emptiable>();
        builder.AddDecorator<GoodsStation, HaulCandidate>();
        InitializeBehaviors(builder);
        return builder.Build();
      }

      private static void InitializeBehaviors(TemplateModule.Builder builder)
      {
        builder.AddDecorator<GoodsStation, EmptyInventoriesWorkplaceBehavior>();
        builder.AddDecorator<GoodsStation, RemoveUnwantedStockWorkplaceBehavior>();
      }
    }
  }
}
