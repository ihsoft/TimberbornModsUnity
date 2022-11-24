﻿using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class ChooChooConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      // containerDefinition.Bind<TrainObjectCollector>().AsSingleton();
      containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule()
    {
      TemplateModule.Builder builder = new TemplateModule.Builder();
      builder.AddDecorator<TrainStation, TrainDestination>();
      return builder.Build();
    }
  }
}
