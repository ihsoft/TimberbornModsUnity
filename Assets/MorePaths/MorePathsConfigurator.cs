using Bindito.Core;
using Timberborn.Beavers;
using Timberborn.PathSystem;
using Timberborn.TemplateSystem;

namespace MorePaths
{
    public class MorePathsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<MorePathsService>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<DrivewayModel, CustomDrivewayModel>();
            // builder.AddDecorator<Beaver, PathListener>();
            return builder.Build();
        }
    }
}