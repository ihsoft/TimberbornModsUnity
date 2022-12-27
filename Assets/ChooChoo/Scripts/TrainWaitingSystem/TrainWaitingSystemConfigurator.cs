using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.PrefabSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainWaitingSystemConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<TrainWaitingLocationsRepository>().AsSingleton();
      containerDefinition.Bind<RandomTrainWaitingLocationPicker>().AsSingleton();
    }
  }
}
