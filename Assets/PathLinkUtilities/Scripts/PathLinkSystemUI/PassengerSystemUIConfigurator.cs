using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;

namespace PathLinkUtilities
{
  [Configurator(SceneEntrypoint.InGame)]
  public class PassengerSystemUIConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<PathLinkPointFragment>().AsSingleton();
      containerDefinition.Bind<ConnectPathLinkPointButton>().AsSingleton();
      containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }

    private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
    {
      private readonly PathLinkPointFragment _pathLinkPointFragment;

      public EntityPanelModuleProvider(PathLinkPointFragment pathLinkPointFragment) => _pathLinkPointFragment = pathLinkPointFragment;

      public EntityPanelModule Get()
      {
        EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
        builder.AddMiddleFragment(_pathLinkPointFragment);
        return builder.Build();
      }
    }
  }
}
