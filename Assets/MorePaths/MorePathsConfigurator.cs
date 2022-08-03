using Bindito.Core;
using Timberborn.PathSystem;
using Timberborn.TemplateSystem;
using TimberbornAPI.AssetLoaderSystem.AssetSystem;

namespace MorePaths
{
    public class MorePathsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<MorePathsService>().AsSingleton();
            containerDefinition.Bind<AssetLoader>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<DrivewayModel, CustomDrivewayModel>();
            return builder.Build();
        }
    }
}