using Bindito.Core;

namespace BeaverHats
{
    public class BeaverHatsConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<BeaverHatsService>().AsSingleton();
        }
    }
}