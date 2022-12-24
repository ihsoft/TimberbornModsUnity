// using Bindito.Core;
// using TimberApi.ConfiguratorSystem;
// using TimberApi.SceneSystem;
// using Timberborn.EntityPanelSystem;
//
// namespace ChooChoo
// {
//   [Configurator(SceneEntrypoint.InGame)]
//   public class SimpleOutputBuildingsUIConfigurator : IConfigurator
//   {
//     public void Configure(IContainerDefinition containerDefinition)
//     {
//       containerDefinition.Bind<GoodsStationRowsFactory>().AsSingleton();
//       containerDefinition.Bind<GoodsStationInventoryFragment>().AsSingleton();
//       containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
//     }
//
//     private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
//     {
//       private readonly GoodsStationInventoryFragment _goodsStationInventoryFragment;
//
//       public EntityPanelModuleProvider(
//         GoodsStationInventoryFragment goodsStationInventoryFragment)
//       {
//         this._goodsStationInventoryFragment = goodsStationInventoryFragment;
//       }
//
//       public EntityPanelModule Get()
//       {
//         EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
//         builder.AddBottomFragment((IEntityPanelFragment) this._goodsStationInventoryFragment);
//         return builder.Build();
//       }
//     }
//   }
// }
