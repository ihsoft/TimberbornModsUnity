using Bindito.Core;
using Timberborn.CameraSystem;
using Timberborn.TemplateSystem;

namespace CustomCursors
{
    public class CustomCursorsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<CustomCursorsService>().AsSingleton();
            // containerDefinition.MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
        }

        // private TemplateModule ProvideTemplateModule()
        // {
        //     TemplateModule.Builder builder = new TemplateModule.Builder();
        //     builder.AddDecorator<GrabbingCameraTargetPicker, CustomCursorsService>();
        //     return builder.Build();
        // }
    }
}