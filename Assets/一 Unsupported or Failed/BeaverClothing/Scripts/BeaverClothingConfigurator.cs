using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.Beavers;
using Timberborn.TemplateSystem;
using Timberborn.WorkSystem;

namespace BeaverHats
{
    [Configurator(SceneEntrypoint.InGame)]
    public class BeaverClothingConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<ClothingSpecificationDeserializer>().AsSingleton();
            containerDefinition.Bind<BeaverClothingService>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<Beaver, ClothingComponent>();
            return builder.Build();
        }
    }
}