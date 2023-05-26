using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ChooChoo
{
    [HarmonyPatch]
    public class BoundsNavRangeServicePatch
    {
        public static Material Material;
        
        static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("BoundsNavRangeDrawingService"), "Load");
        }
        
        static void Prefix(object ____boundsNavRangeDrawer)
        {
            Material = ChooChooCore.GetInaccessibleField(____boundsNavRangeDrawer, "_material") as Material;
        }
    }
}