using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace MorePaths
{
    [Configurator(SceneEntrypoint.MainMenu | SceneEntrypoint.InGame)]
    public class MorePathsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<PathSpecificationObjectDeserializer>().AsSingleton();
            containerDefinition.Bind<MorePathsCore>().AsSingleton();
        }
    }
}