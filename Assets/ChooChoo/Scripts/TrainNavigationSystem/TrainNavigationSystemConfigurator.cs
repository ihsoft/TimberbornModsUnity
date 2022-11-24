using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace ChooChoo
{
    [Configurator(SceneEntrypoint.InGame)]
    public class TrainNavigationSystemConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<TrainDestinationsRepository>().AsSingleton();
        }
    }
}