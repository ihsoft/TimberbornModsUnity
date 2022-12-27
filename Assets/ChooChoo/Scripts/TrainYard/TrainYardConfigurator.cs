using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.GameDistrictsUI;
using Timberborn.PrefabSystem;
using Timberborn.PreviewSystem;

namespace ChooChoo
{
  [Configurator(SceneEntrypoint.InGame)]
  public class TrainYardConfigurator : IConfigurator
  {
    public void Configure(IContainerDefinition containerDefinition)
    {
      containerDefinition.Bind<TrainYardService>().AsSingleton();
      containerDefinition.MultiBind<IPreviewsValidator>().To<TrainYardPreviewsValidator>().AsSingleton();
    }
  }
}
