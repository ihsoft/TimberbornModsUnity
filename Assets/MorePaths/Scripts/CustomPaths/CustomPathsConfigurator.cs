using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace MorePaths
{
    [Configurator(SceneEntrypoint.InGame)]
    public class CustomPathsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<DrivewayFactory>().AsSingleton();
            containerDefinition.Bind<DrivewayService>().AsSingleton();
            
            containerDefinition.Bind<CustomPathFactory>().AsSingleton();
            
            containerDefinition.Bind<PathCornerService>().AsSingleton();
        }
    }
}
