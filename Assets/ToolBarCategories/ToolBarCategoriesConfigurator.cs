using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.TemplateSystem;
using TimberbornAPI.AssetLoaderSystem.AssetSystem;

namespace ToolBarCategories
{
    public class ToolBarCategoriesConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ToolBarCategoriesService>().AsSingleton();
            containerDefinition.Bind<AssetLoader>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<PlaceableBlockObject, ToolBarCategory>();
            return builder.Build();
        }
    }
}