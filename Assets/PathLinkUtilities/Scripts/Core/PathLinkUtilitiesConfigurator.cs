using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace PathLinkUtilities
{
  [Configurator(SceneEntrypoint.InGame)]
  public class PathLinkUtilitiesConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<PathLinkUtilitiesCore>().AsSingleton();
    }
  }
}
