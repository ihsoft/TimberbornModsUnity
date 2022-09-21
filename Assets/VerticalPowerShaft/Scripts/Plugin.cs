using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;
using Timberborn.BlockObjectTools;

namespace VerticalPowerShaft
{
    // [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    // [BepInDependency("com.timberapi.timberapi")]
    // public class Plugin : BaseUnityPlugin
    // {
    //     public const string PluginGuid = "tobbert.verticalpowershaft";
    //     public const string PluginName = "Vertical Power Shaft";
    //     public const string PluginVersion = "1.0.2";
    //     
    //     public static ManualLogSource Log;
    //     
    //     void Awake()
    //     {
    //         Log = Logger;
    //         
    //         Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
    //         
    //         TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.InGame);
    //         new Harmony(PluginGuid).PatchAll();
    //     }
    // }
    
    public class Plugin : IModEntrypoint
    {
        public const string PluginGuid = "tobbert.verticalpowershaft";
        public const string PluginName = "Vertical Power Shaft";
        public const string PluginVersion = "1.0.2";
        
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
            return !__instance.Prefab.TryGetComponent(out VerticalPowerShaftComponent verticalPowerShaftComponent);
        }
    }
    
    [HarmonyPatch(typeof(BlockObjectTool), "RotateCounterclockwise")]
    public class RotateCounterclockwisePatch
    {
        static bool Prefix(BlockObjectTool __instance)
        {
            return !__instance.Prefab.TryGetComponent(out VerticalPowerShaftComponent verticalPowerShaftComponent);
        }
    }
}