using Bindito.Core;
using ChooChoo.Scripts.GoodsStation;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.Emptying;
using Timberborn.Hauling;
using Timberborn.StockpilePrioritySystem;
using Timberborn.TemplateSystem;
using Timberborn.Workshops;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class GoodsStationConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<GoodsStationSendingInventoryInitializer>().AsSingleton();
      containerDefinition.Bind<GoodsStationReceivingInventoryInitializer>().AsSingleton();
      containerDefinition.Bind<GoodsStationRegistry>().AsSingleton();
      containerDefinition.Bind<TrainDistributableGoodValidator>().AsSingleton();
      containerDefinition.Bind<GoodsStationsRepository>().AsSingleton();
      containerDefinition.Bind<GoodsStationService>().AsSingleton();
      containerDefinition.Bind<TransferableGoodObjectSerializer>().AsSingleton();
      containerDefinition.Bind<GoodsStationDistributionSettingSerializer>().AsSingleton();
      containerDefinition.Bind<TrainDistributableGoodObjectSerializer>().AsSingleton();
      containerDefinition.MultiBind<TemplateModule>().ToProvider<TemplateModuleProvider>().AsSingleton();
    }

    private class TemplateModuleProvider : IProvider<TemplateModule>
    {
      private readonly GoodsStationSendingInventoryInitializer _goodsStationSendingInventoryInitializer;
      private readonly GoodsStationReceivingInventoryInitializer _goodsStationReceivingInventoryInitializer;

      public TemplateModuleProvider(
        GoodsStationSendingInventoryInitializer goodsStationSendingInventoryInitializer,
        GoodsStationReceivingInventoryInitializer goodsStationReceivingInventoryInitializer)
      {
        _goodsStationSendingInventoryInitializer = goodsStationSendingInventoryInitializer;
        _goodsStationReceivingInventoryInitializer = goodsStationReceivingInventoryInitializer;
      }

      public TemplateModule Get()
      {
        TemplateModule.Builder builder = new TemplateModule.Builder();
        builder.AddDecorator<GoodsStation, TrainDestination>();
        builder.AddDecorator<GoodsStation, GoodsStationDistributionSetting>();
        builder.AddDecorator<GoodsStation, GoodsStationDistributableGoodProvider>();
        builder.AddDecorator<GoodsStation, GoodsStationDescriber>();
        builder.AddDecorator<GoodsStation, GoodsStationSendingInventory>();
        builder.AddDecorator<GoodsStation, GoodsStationReceivingInventory>();
        builder.AddDedicatedDecorator(_goodsStationSendingInventoryInitializer);
        builder.AddDedicatedDecorator(_goodsStationReceivingInventoryInitializer);
        builder.AddDecorator<GoodsStation, Emptiable>();
        builder.AddDecorator<GoodsStation, HaulCandidate>();
        InitializeBehaviors(builder);
        return builder.Build();
      }

      private static void InitializeBehaviors(TemplateModule.Builder builder)
      {
        builder.AddDecorator<GoodsStation, GoodsStationWorkplaceBehavior>();
      }
    }
  }
}
