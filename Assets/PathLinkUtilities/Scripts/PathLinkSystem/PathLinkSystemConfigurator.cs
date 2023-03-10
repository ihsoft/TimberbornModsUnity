using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.TemplateSystem;
using Timberborn.WalkingSystem;

namespace PathLinkUtilities
{
  [Configurator(SceneEntrypoint.InGame)]
  public class PathLinkSystemConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<PathLinkObjectSerializer>().AsSingleton();
      containerDefinition.Bind<PathLinkRepository>().AsSingleton();
      containerDefinition.MultiBind<TemplateModule>().ToProvider<TemplateModuleProvider>().AsSingleton();
    }

    private class TemplateModuleProvider : IProvider<TemplateModule>
    {
      public TemplateModule Get()
      {
        TemplateModule.Builder builder = new TemplateModule.Builder();
        builder.AddDecorator<Walker, PathLinkWaiter>();
        return builder.Build();
      }
    }
  }
}
