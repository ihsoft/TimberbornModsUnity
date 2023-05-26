using Bindito.Core;
using System;
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
      // containerDefinition.Bind<GoodInputToggleFactory>().AsSingleton();
      containerDefinition.Bind<GoodsStationIconService>().AsSingleton();
      
      containerDefinition.Bind<GoodsStationFragment>().AsSingleton();
      
      containerDefinition.Bind<GoodsStationRowsFactory>().AsSingleton();
      containerDefinition.Bind<GoodsStationSendingInventoryFragment>().AsSingleton();
      containerDefinition.Bind<GoodsStationReceivingInventoryFragment>().AsSingleton();
      
      // containerDefinition.Bind<StockpileInventoryFragment>().AsSingleton();
      // containerDefinition.Bind<StockpileBatchControlRowItemFactory>().AsSingleton();
      
      containerDefinition.Bind<IGoodSelectionController>().To<GoodSelectionController>().AsSingleton();
      
      // containerDefinition.Bind<StockpileOverlay>().AsSingleton();
      // containerDefinition.Bind<StockpileOverlayShower>().AsSingleton();
      containerDefinition.Bind<StockpileGoodSelectionBox>().AsSingleton();
      // containerDefinition.Bind<StockpileOverlayTogglePanel>().AsSingleton();
      // containerDefinition.Bind<StockpileOverlayHider>().AsSingleton();
      containerDefinition.Bind<GoodSelectionBoxRowFactory>().AsSingleton();
      containerDefinition.Bind<GoodSelectionBoxItemFactory>().AsSingleton();
      // containerDefinition.Bind<StockpileOptionsService>().AsSingleton();
      containerDefinition.Bind<GoodsStationGoodSelectionBoxItemsFactory>().AsSingleton();
      // containerDefinition.Bind<GoodStockpilesTooltipFactory>().AsSingleton();
      containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
      containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule()
    {
      TemplateModule.Builder builder = new TemplateModule.Builder();
      builder.AddDecorator<GoodsStation, GoodsStationOptionsProvider>();
      // builder.AddDecorator<GoodsStation, StockpileOverlayItemAdder>();
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
