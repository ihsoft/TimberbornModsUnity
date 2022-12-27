using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.PrefabSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainDestroySystemConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<TrainDestroyer>().AsSingleton();
    }
  }
}
