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
      containerDefinition.Bind<GoodInputToggleFactory>().AsSingleton();
      containerDefinition.Bind<GoodsStationIconService>().AsSingleton();
      
      containerDefinition.Bind<GoodsStationRowsFactory>().AsSingleton();
      containerDefinition.Bind<GoodsStationInventoryFragment>().AsSingleton();
      
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
      private readonly GoodsStationInventoryFragment _goodsStationInventoryFragment;

      public EntityPanelModuleProvider(
        GoodsStationInventoryFragment goodsStationInventoryFragment)
      {
        _goodsStationInventoryFragment = goodsStationInventoryFragment;
      }

      public EntityPanelModule Get()
      {
        EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
        builder.AddBottomFragment(_goodsStationInventoryFragment);
        return builder.Build();
      }
    }
  }
}
