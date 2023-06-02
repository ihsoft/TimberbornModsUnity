using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class PassengerSystemUIConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<PassengerDistrictObstacleFragment>().AsSingleton();
      containerDefinition.Bind<PathLinkPointFragment>().AsSingleton();
      containerDefinition.Bind<ConnectPathLinkPointButton>().AsSingleton();
      containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }

    private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
    {
      private readonly PassengerDistrictObstacleFragment _passengerDistrictObstacleFragment;
      private readonly PathLinkPointFragment _pathLinkPointFragment;

      public EntityPanelModuleProvider(PassengerDistrictObstacleFragment passengerDistrictObstacleFragment, PathLinkPointFragment pathLinkPointFragment)
      {
        _passengerDistrictObstacleFragment = passengerDistrictObstacleFragment;
        _pathLinkPointFragment = pathLinkPointFragment;
      }

      public EntityPanelModule Get()
      {
        EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
        // builder.AddMiddleFragment(_pathLinkPointFragment);
        builder.AddMiddleFragment(_passengerDistrictObstacleFragment);
        return builder.Build();
      }
    }
  }
}
