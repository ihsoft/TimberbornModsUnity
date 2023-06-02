using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class GoodsStationUIConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<GoodsStationFragment>().AsSingleton();
      
      containerDefinition.Bind<GoodsStationSendingInventoryFragment>().AsSingleton();
      containerDefinition.Bind<GoodsStationReceivingInventoryFragment>().AsSingleton();
      
      containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
      containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule()
    {
      TemplateModule.Builder builder = new TemplateModule.Builder();
      builder.AddDecorator<GoodsStation, GoodsStationOptionsProvider>();
      return builder.Build();
    }

    private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
    {
      private readonly GoodsStationFragment _goodsStationFragment;
      private readonly GoodsStationSendingInventoryFragment _goodsStationSendingInventoryFragment;
      private readonly GoodsStationReceivingInventoryFragment _goodsStationReceivingInventoryFragment;

      public EntityPanelModuleProvider(
        GoodsStationFragment goodsStationFragment,
        GoodsStationSendingInventoryFragment goodsStationSendingInventoryFragment,
        GoodsStationReceivingInventoryFragment goodsStationReceivingInventoryFragment
        )
      {
        _goodsStationFragment = goodsStationFragment;
        _goodsStationSendingInventoryFragment = goodsStationSendingInventoryFragment;
        _goodsStationReceivingInventoryFragment = goodsStationReceivingInventoryFragment;
      }

      public EntityPanelModule Get()
      {
        EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
        builder.AddSideFragment(_goodsStationFragment);
        builder.AddBottomFragment(_goodsStationSendingInventoryFragment);
        builder.AddBottomFragment(_goodsStationReceivingInventoryFragment);
        return builder.Build();
      }
      
      
    }
  }
}
