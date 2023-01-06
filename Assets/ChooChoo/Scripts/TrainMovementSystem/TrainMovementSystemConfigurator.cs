using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.PrefabSystem;
using Timberborn.TemplateSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainMovementSystemConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<TrackFollowerFactory>().AsSingleton();
    }
  }
}
