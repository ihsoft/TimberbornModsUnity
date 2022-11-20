using Bindito.Core;
using GlobalMarket;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
    [Configurator(SceneEntrypoint.InGame)]
    public class TrackSystemConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<TrackMap>().AsSingleton();
            containerDefinition.Bind<TrackConnectionsArrayProvider>().AsSingleton();
            containerDefinition.Bind<TracksService>().AsSingleton();
            containerDefinition.Bind<TrackConnectionService>().AsSingleton();
            containerDefinition.Bind<TrackSectionService>().AsSingleton();

            // containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        // private static TemplateModule ProvideTemplateModule()
        // {
        //     TemplateModule.Builder builder = new TemplateModule.Builder();
        //     builder.AddDecorator<TrainStation, TrainsManager>();
        //     return builder.Build();
        // }
    }
}