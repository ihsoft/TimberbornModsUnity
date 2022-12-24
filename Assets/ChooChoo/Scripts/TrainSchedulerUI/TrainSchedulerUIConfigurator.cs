using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntityPanelSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainSchedulerUIConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<TrainScheduleFragment>().AsSingleton();
      containerDefinition.Bind<TrainScheduleActionFactory>().AsSingleton();
      containerDefinition.Bind<TrainScheduleSectionFactory>().AsSingleton();
      containerDefinition.Bind<NewStationButton>().AsSingleton();
      containerDefinition.MultiBind<EntityPanelModule>().ToProvider<EntityPanelModuleProvider>().AsSingleton();
    }
    
    private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
    {
      private readonly TrainScheduleFragment _trainScheduleFragment;

      public EntityPanelModuleProvider(TrainScheduleFragment trainScheduleFragment)
      {
        _trainScheduleFragment = trainScheduleFragment;
      }

      public EntityPanelModule Get()
      {
        EntityPanelModule.Builder builder = new EntityPanelModule.Builder();
        builder.AddMiddleFragment(_trainScheduleFragment);
        return builder.Build();
      }
    }
  }
}
