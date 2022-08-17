using Bindito.Core;

namespace PlaceableCropFields
{
    public class PlaceableCropFieldsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<PlaceableCropFieldsService>().AsSingleton();
        }
    }
}