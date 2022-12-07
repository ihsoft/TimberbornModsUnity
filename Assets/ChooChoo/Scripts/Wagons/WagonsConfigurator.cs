using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class WagonsConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<ObjectFollowerFactory>().AsSingleton();
    }
  }
}
