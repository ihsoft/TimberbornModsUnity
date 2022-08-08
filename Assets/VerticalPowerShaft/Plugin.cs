using BepInEx;
using BepInEx.Logging;
using TimberbornAPI;
using TimberbornAPI.Common;

namespace VerticalPowerShaft
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.verticalpowershaft";
        public const string PluginName = "Vertical Power Shaft";
        public const string PluginVersion = "1.0.2";
        
        public static ManualLogSource Log;
        
        void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
            
            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.Global);
        }
    }
}