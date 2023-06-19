using System.Reflection;
using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using Timberborn.Carrying;
using Timberborn.SingletonSystem;

namespace ChooChoo
{
    [HarmonyPatch]
    public class DeliveryCompletedEventListener
    {
        private static EventBus _eventBus;

        private static EventBus EventBus => _eventBus ??= DependencyContainer.GetInstance<EventBus>();

        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("CarryRootBehavior"), "CompleteDelivery");
        }
        
        static void Prefix(CarryRootBehavior __instance)
        {
            EventBus.Post(new DeliveryCompletedEvent(__instance));
        }
    }
}