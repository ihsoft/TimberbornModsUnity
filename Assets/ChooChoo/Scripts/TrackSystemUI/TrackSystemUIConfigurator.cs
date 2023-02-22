using Bindito.Core;
using System;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrackSystemUIConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<TrackSectionDividerFragment>().AsSingleton();
      containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }

    private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
    {
      private readonly TrackSectionDividerFragment _trackSectionDividerFragment;

      public EntityPanelModuleProvider(TrackSectionDividerFragment trackSectionDividerFragment)
      {
        _trackSectionDividerFragment = trackSectionDividerFragment;
      }

      public EntityPanelModule Get()
      {
        EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
        builder.AddMiddleFragment(_trackSectionDividerFragment);
        return builder.Build();
      }
    }
  }
}
