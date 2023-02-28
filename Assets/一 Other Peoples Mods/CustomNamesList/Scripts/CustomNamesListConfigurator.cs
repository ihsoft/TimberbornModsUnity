using Bindito.Core;
using CustomNameList;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class CustomNamesListConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<CustomNameService>().AsSingleton();
    }
  }
}
