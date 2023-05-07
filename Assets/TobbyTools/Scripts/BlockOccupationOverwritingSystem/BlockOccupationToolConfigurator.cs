using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace TobbyTools.BlockOccupationTool
{
    [Configurator(SceneEntrypoint.All)]
    public class TextureReplacementToolConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<BlockSpecificationDeserializer>().AsSingleton();
            containerDefinition.Bind<BlockOccupationSpecificationDeserializer>().AsSingleton();
            containerDefinition.Bind<BlockOccupationService>().AsSingleton();
        }
    }
}