using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.Beavers;
using Timberborn.TemplateSystem;

namespace BeaverHats
{
    [Configurator(SceneEntrypoint.InGame)]
    public class BeaverHatsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<BeaverHatsService>().AsSingleton();
            containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        private TemplateModule ProvideTemplateModule()
        {
            TemplateModule.Builder builder = new TemplateModule.Builder();
            builder.AddDecorator<Beaver, ProfessionClothingComponent>();
            return builder.Build();
        }
    }
}