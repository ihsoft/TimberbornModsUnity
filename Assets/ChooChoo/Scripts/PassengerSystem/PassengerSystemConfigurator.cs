using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.TemplateSystem;
using Timberborn.WalkingSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class PassengerSystemConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<PassengerStationLinkObjectSerializer>().AsSingleton();
      containerDefinition.Bind<PassengerStationLinkRepository>().AsSingleton();
      containerDefinition.Bind<PathCornerBlockObjectRepository>().AsSingleton();
      containerDefinition.Bind<PathCorrector>().AsSingleton();
      containerDefinition.MultiBind<TemplateModule>().ToProvider<TemplateModuleProvider>().AsSingleton();
    }

    private class TemplateModuleProvider : IProvider<TemplateModule>
    {
      public TemplateModule Get()
      {
        TemplateModule.Builder builder = new TemplateModule.Builder();
        builder.AddDecorator<Walker, Passenger>();
        builder.AddDecorator<Walker, PathFollowerUtilities>();
        builder.AddDecorator<PassengerStation, TrainDestination>();
        return builder.Build();
      }
    }
  }
}
