using Bindito.Core;
using System;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainDistributionSystemUIConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<TrainDistributionAmountFragment>().AsSingleton();
      containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }

    private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
    {
      private readonly TrainDistributionAmountFragment _trainDistributionAmountFragment;

      public EntityPanelModuleProvider(TrainDistributionAmountFragment trainDistributionAmountFragment)
      {
        _trainDistributionAmountFragment = trainDistributionAmountFragment;
      }

      public EntityPanelModule Get()
      {
        EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
        builder.AddMiddleFragment(_trainDistributionAmountFragment);
        return builder.Build();
      }
    }
  }
}
