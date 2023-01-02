using Bindito.Core;
using ChooChoo;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.BeaversUI;
using Timberborn.TemplateSystem;

namespace GlobalMarket
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainsUIConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }
    
    private static TemplateModule ProvideTemplateModule()
    {
      TemplateModule.Builder builder = new TemplateModule.Builder();
      builder.AddDecorator<TrainWagonManager, TrainEntityBadge>();
      return builder.Build();
    }
  }
}
