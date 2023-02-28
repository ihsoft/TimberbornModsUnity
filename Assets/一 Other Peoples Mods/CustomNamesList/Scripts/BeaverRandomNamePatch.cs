using System.Reflection;
using HarmonyLib;

namespace CustomNameList
{
    [HarmonyPatch]
    public class BeaverRandomNamePatch
    {
        private static CustomNameService _customNameService;
        
        private static CustomNameService CustomNameService
        {
            get
            {
                return _customNameService ??= TimberApi.DependencyContainerSystem.DependencyContainer.GetInstance<CustomNameService>();
            }
        }
    
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("BeaverNameService"), "RandomName");
        }
        
        static void Postfix(ref string __result)
        {
            __result = CustomNameService.NextName();
        }
    }
}