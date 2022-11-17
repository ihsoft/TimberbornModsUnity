using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.PathSystem;
using Timberborn.TemplateSystem;

namespace MorePaths
{
    [Configurator(SceneEntrypoint.MainMenu | SceneEntrypoint.InGame)]
    public class MorePathsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<PathSpecificationObjectDeserializer>().AsSingleton();
            containerDefinition.Bind<MorePathsCore>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<DrivewayModel, CustomDrivewayModel>();
            // builder.AddDecorator<DynamicPathModel, DynamicPathCorner>();
            // builder.AddDecorator<Beaver, PathListener>();
            return builder.Build();
        }
    }
}