using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TimberbornAPI;
using TimberbornAPI.Common;

namespace Ladder
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.ladder";
        public const string PluginName = "Ladder";
        public const string PluginVersion = "1.1.0";

        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");

            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.Global);

            new Harmony(PluginGuid).PatchAll();
        }
    }
    
    // [HarmonyPatch(typeof(PathReconstructor), "TiltingOffset", new Type[] {typeof(List<Vector3>), typeof(int), typeof(int)})]
    // public class Patch
    // {
    //    
    //     static void Postfix(
    //         Vector3 __result,
    //         List<Vector3> pathCorners,
    //         int startIndex,
    //         int endIndex)
    //     {
    //         Plugin.Log.LogFatal(__result);
    //     }
    // }
    //
    // [HarmonyPatch(typeof(PathReconstructor), "ReconstructPath", new Type[] {typeof(IFlowField), typeof(Vector3), typeof(Vector3), typeof(List<Vector3>)})]
    // public class Patch2
    // {
    //    
    //     static void Postfix(
    //         Vector3 __result,
    //         IFlowField flowField,
    //         Vector3 start,
    //         Vector3 destination,
    //         List<Vector3> pathCorners)
    //     {
    //         Plugin.Log.LogFatal(__result);
    //     }
    // }
}