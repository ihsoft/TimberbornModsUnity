using System.Reflection;
using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using Timberborn.SingletonSystem;

namespace ChooChoo
{
    [HarmonyPatch]
    public class GoodStorageChangedEventListener
    {
        private static EventBus _eventBus;

        private static EventBus EventBus => _eventBus ??= DependencyContainer.GetInstance<EventBus>();

        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("DistrictDistributableGoodProvider"), "ClearCache", new[]
            {
                typeof(string)
            });
        }
        
        static void Prefix(string goodId)
        {
            EventBus.Post(new GoodStorageChangedEvent(goodId));
        }
    }
}