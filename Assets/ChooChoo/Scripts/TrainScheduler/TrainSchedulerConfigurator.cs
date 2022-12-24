using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainSchedulerConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      // containerDefinition.MultiBind<ITrainAction>().To<TrainActionWait>().AsSingleton();
      // containerDefinition.MultiBind<ITrainAction>().To<TrainActionTakeGoods>().AsSingleton();
      
      containerDefinition.Bind<TrainScheduleObjectSerializer>().AsSingleton();
      containerDefinition.Bind<TrainActionObjectSerializer>().AsSingleton();
    }
  }
}
