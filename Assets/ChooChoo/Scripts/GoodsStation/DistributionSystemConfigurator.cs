using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.Emptying;
using Timberborn.Hauling;
using Timberborn.TemplateSystem;
using Timberborn.DistributionSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class DistributionSystemConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<GoodsStationInventoryInitializer>().AsSingleton();
      // containerDefinition.Bind<DistributionRouteCalculator>().AsSingleton();
      // containerDefinition.Bind<DistributionRouteObjectSerializer>().AsSingleton();
      containerDefinition.MultiBind<TemplateModule>().ToProvider<DistributionSystemConfigurator.TemplateModuleProvider>().AsSingleton();
    }

    private class TemplateModuleProvider : IProvider<TemplateModule>
    {
      private readonly GoodsStationInventoryInitializer _distributionPostInventoryInitializer;

      public TemplateModuleProvider(GoodsStationInventoryInitializer distributionPostInventoryInitializer)
      {
        _distributionPostInventoryInitializer = distributionPostInventoryInitializer;
      }

      public TemplateModule Get()
      {
        TemplateModule.Builder builder = new TemplateModule.Builder();
        // builder.AddDecorator<Worker, DistributableGoodBringer>();
        // builder.AddDecorator<DistrictCenter, DistrictDistributionLimits>();
        builder.AddDedicatedDecorator(_distributionPostInventoryInitializer);
        builder.AddDecorator<GoodsStation, Emptiable>();
        builder.AddDecorator<GoodsStation, HaulCandidate>();
        builder.AddDecorator<GoodsStation, AutoEmptiable>();
        builder.AddDecorator<GoodsStation, BringDistributableGoodHaulBehaviorProvider>();
        // builder.AddDecorator<DistributionPost, WorkplaceWithBackpacks>();
        // builder.AddDecorator<DistributionPost, DistributeGoodWorkplaceBehavior>();
        // builder.AddDecorator<DistributionPost, EmptyInventoriesWorkplaceBehavior>();
        // builder.AddDecorator<DistributionPost, RemoveUnwantedStockWorkplaceBehavior>();
        builder.AddDecorator<GoodsStation, BringDistributableGoodWorkplaceBehavior>();
        // builder.AddDecorator<DistributionPost, WaitInsideIdlyWorkplaceBehavior>();
        // builder.AddDecorator<DropOffPoint, Emptiable>();
        // builder.AddDecorator<DropOffPoint, HaulCandidate>();
        // builder.AddDecorator<DropOffPoint, AutoEmptiable>();
        // builder.AddDecorator<DropOffPoint, BlockableBuilding>();
        // builder.AddDecorator<DropOffPoint, SimpleOutputInventoryHaulBehaviorProvider>();
        // builder.AddDecorator<DropOffPoint, EmptyOutputWorkplaceBehavior>();
        // builder.AddDecorator<DropOffPoint, EmptyInventoriesWorkplaceBehavior>();
        // builder.AddDecorator<DropOffPoint, RemoveUnwantedStockWorkplaceBehavior>();
        return builder.Build();
      }
    }
  }
}
