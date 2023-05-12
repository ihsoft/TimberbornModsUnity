using Bindito.Core;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;
using TimberApi.ToolSystem;
using Timberborn.BottomBarSystem;

namespace PipetteTool
{
    [Configurator(SceneEntrypoint.InGame)]
    public class PipetteToolInGameConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<IPipetteTool>().To<PipetteToolInGame>().AsSingleton();
            containerDefinition.MultiBind<IToolFactory>().To<PipetteToolFactory>().AsSingleton();
            // containerDefinition.Bind<PipetteToolButtonProvider>().AsSingleton();

            // containerDefinition.MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider>().AsSingleton();
        }
    }
    
    // [Configurator(SceneEntrypoint.MapEditor)]
    // public class PipetteToolMapEditorConfigurator : IConfigurator
    // {
    //     public void Configure(IContainerDefinition containerDefinition)
    //     {
    //         containerDefinition.Bind<IPipetteTool>().To<PipetteTool>().AsSingleton();
    //         
    //         containerDefinition.Bind<PipetteToolButtonProvider>().AsSingleton();
    //
    //         containerDefinition.MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider>().AsSingleton();
    //     }
    // }
    //
    // public class BottomBarModuleProvider : IProvider<BottomBarModule>
    // {
    //     private readonly PipetteToolButtonProvider _pipetteToolButtonProvider;
    //     
    //     public BottomBarModuleProvider(PipetteToolButtonProvider pipetteToolButtonProvider) => _pipetteToolButtonProvider = pipetteToolButtonProvider;
    //     
    //     public BottomBarModule Get()
    //     {
    //         BottomBarModule.Builder builder = new BottomBarModule.Builder();
    //         builder.AddLeftSectionElement(_pipetteToolButtonProvider, 91);
    //         return builder.Build();
    //     }
    // }
}