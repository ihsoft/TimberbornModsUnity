using Bindito.Core;

namespace CategoryButton
{
    public class ToolBarCategoriesConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<CategoryButtonService>().AsSingleton();
        }
    }
}