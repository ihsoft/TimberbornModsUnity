using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using Timberborn.BottomBarSystem;

namespace PipetteTool
{
    [Configurator(SceneEntrypoint.InGame)]
    public class PipetteToolInGameConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            // containerDefinition.Bind<PipetteToolGroup>().AsSingleton();
            containerDefinition.Bind<IPipetteTool>().To<PipetteToolInGame>().AsSingleton();
            
            containerDefinition.Bind<PipetteToolButton>().AsSingleton();

            containerDefinition.MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider>().AsSingleton();
        }
    }
    
    [Configurator(SceneEntrypoint.MapEditor)]
    public class PipetteToolMapEditorConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            // containerDefinition.Bind<PipetteToolGroup>().AsSingleton();
            containerDefinition.Bind<IPipetteTool>().To<PipetteTool>().AsSingleton();
            
            containerDefinition.Bind<PipetteToolButton>().AsSingleton();

            containerDefinition.MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider>().AsSingleton();
        }
    }

    public class BottomBarModuleProvider : IProvider<BottomBarModule>
    {
        private readonly PipetteToolButton _pipetteToolButton;
        
        public BottomBarModuleProvider(PipetteToolButton pipetteToolButton) => _pipetteToolButton = pipetteToolButton;
        
        public BottomBarModule Get()
        {
            BottomBarModule.Builder builder = new BottomBarModule.Builder();
            builder.AddLeftSectionElement(_pipetteToolButton, 9);
            return builder.Build();
        }
    }
}