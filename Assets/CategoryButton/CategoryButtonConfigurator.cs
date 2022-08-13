using Bindito.Core;
using TimberbornAPI.AssetLoaderSystem.AssetSystem;

namespace CategoryButton
{
    public class ToolBarCategoriesConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ToolBarCategoriesService>().AsSingleton();
            containerDefinition.Bind<AssetLoader>().AsSingleton();
        }
    }
}