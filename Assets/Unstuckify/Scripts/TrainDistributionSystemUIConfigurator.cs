using Bindito.Core;
using System;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;
using Timberborn.TemplateSystem;
using Unstuckify.Scripts;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class UnstuckifyConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<UnstuckifyService>().AsSingleton();
      containerDefinition.Bind<UnstuckifyFragment>().AsSingleton();
      containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }

    private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
    {
      private readonly UnstuckifyFragment _unstuckifyFragment;

      public EntityPanelModuleProvider(UnstuckifyFragment unstuckifyFragment)
      {
        _unstuckifyFragment = unstuckifyFragment;
      }

      public EntityPanelModule Get()
      {
        EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
        builder.AddMiddleFragment(_unstuckifyFragment);
        return builder.Build();
      }
    }
  }
}
