using System.Reflection;
using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using Timberborn.Carrying;
using Timberborn.SingletonSystem;

namespace ChooChoo
{
    [HarmonyPatch]
    public class EmptyingHandsEventListner
    {
        private static EventBus _eventBus;

        private static EventBus EventBus => _eventBus ??= DependencyContainer.GetInstance<EventBus>();

        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("GoodCarrier"), "EmptyHands");
        }
        
        static void Prefix(GoodCarrier __instance)
        {
            EventBus.Post(new EmptyingHandsEvent(__instance.GetComponentFast<CarryRootBehavior>()));
        }
    }
}