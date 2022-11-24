using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.EntitySystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.MultiBind<IObjectCollection>().To<TrainObjectCollector>().AsSingleton();
      containerDefinition.Bind<MachinistCharacterFactory>().AsSingleton();
    }
  }
}
