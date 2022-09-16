using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TimberbornAPI;
using TimberbornAPI.Common;
using UnityEngine.UIElements;

namespace CustomCursors
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.customcursors";
        public const string PluginName = "Custom Cursors";
        public const string PluginVersion = "1.0.0";
        
        public static ManualLogSource Log;

        void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
                        
            TimberAPI.DependencyRegistry.AddConfigurator(new CustomCursorsConfigurator(), SceneEntryPoint.Global);
            new Harmony(PluginGuid).PatchAll();
        }
    }

    [HarmonyPatch]
    public class StartGrabbingPatch
    {
        static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("GrabbingCameraTargetPicker"), "StartGrabbing");
        }
        
        static void Postfix()
        {
            TimberAPI.DependencyContainer.GetInstance<CustomCursorsService>().StartGrabbing();
        }
    }
    
    [HarmonyPatch]
    public class StopGrabbingPatch
    {
        static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("GrabbingCameraTargetPicker"), "StopGrabbing");
        }
        
        static void Postfix()
        {
            TimberAPI.DependencyContainer.GetInstance<CustomCursorsService>().StopGrabbing();
        }
    }
    
    [HarmonyPatch]
    public class SettingsPatch
    {
        static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("GameSavingSettingsController"), "InitializeAutoSavingOnToggle", new []
            {
                typeof(VisualElement)
            });
        }
        
        static void Postfix(ref VisualElement root)
        {
            TimberAPI.DependencyContainer.GetInstance<CustomCursorsService>().InitializeSelectorSettings(ref root);
        }
    }
}