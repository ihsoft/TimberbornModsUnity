using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace TobbyTools.DynamicSpecificationSystem
{
    [Configurator(SceneEntrypoint.MainMenu)]
    public class DynamicSpecificationSystemConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<DynamicSpecificationDeserializer>().AsSingleton();
            containerDefinition.Bind<DynamicSpecificationFactory>().AsSingleton();
            containerDefinition.Bind<DynamicSpecificationApplier>().AsSingleton();
        }
    }
}