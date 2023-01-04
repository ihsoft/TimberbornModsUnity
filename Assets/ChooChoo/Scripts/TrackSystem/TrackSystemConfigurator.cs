using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.Coordinates;
using Timberborn.Persistence;

namespace ChooChoo
{
    [Configurator(SceneEntrypoint.InGame)]
    public class TrackSystemConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<TrackArrayProvider>().AsSingleton();
            containerDefinition.Bind<TracksService>().AsSingleton();
            containerDefinition.Bind<TrackConnectionService>().AsSingleton();
            containerDefinition.Bind<TrackRouteWeightCache>().AsSingleton();
            // containerDefinition.Bind<TrackSectionService>().AsSingleton();
            containerDefinition.Bind<EnumObjectSerializer<Direction2D>>().AsSingleton();
            containerDefinition.Bind<TrackRouteObjectSerializer>().AsSingleton();
            containerDefinition.Bind<TrackConnectionObjectSerializer>().AsSingleton();

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