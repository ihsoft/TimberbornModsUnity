using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;
using Timberborn.BlockObjectTools;

namespace VerticalPowerShaft
{
    public class Plugin : IModEntrypoint
    {
        public const string PluginGuid = "tobbert.verticalpowershaft";
        
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            new Harmony(PluginGuid).PatchAll();
        }
    }
    
    [HarmonyPatch(typeof(BlockObjectTool), "RotateClockwise")]
    public class RotateClockwisePatch
    {
        static bool Prefix(BlockObjectTool __instance)
        {
            return !__instance.Prefab.TryGetComponentFast(out VerticalPowerShaftComponent verticalPowerShaftComponent);
        }
    }
    
    [HarmonyPatch(typeof(BlockObjectTool), "RotateCounterclockwise")]
    public class RotateCounterclockwisePatch
    {
        static bool Prefix(BlockObjectTool __instance)
        {
            return !__instance.Prefab.TryGetComponentFast(out VerticalPowerShaftComponent verticalPowerShaftComponent);
        }
    }
}