using Bindito.Core;
using Timberborn.Beavers;
using Timberborn.TemplateSystem;

namespace BeaverHats
{
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