using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.Navigation;
using TimberbornAPI;
using TimberbornAPI.Common;
using UnityEngine;

namespace Ladder
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.ladder";
        public const string PluginName = "Ladder";
        public const string PluginVersion = "1.2.1";

        public static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");

            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.Global);
            TimberAPI.DependencyRegistry.AddConfigurator(new LadderConfigurator());

            new Harmony(PluginGuid).PatchAll();
        }
    }
    
    [HarmonyPatch(typeof(Debug), "LogWarning", typeof(object))]
    public class LogWarningPatch
    {
        static bool Prefix(object message, bool __runOriginal)
        {
            if (__runOriginal)
            {
                string mess = message as string;
                if (mess != null && mess.Contains("path marker mesh at"))
                {
                    return false;
                }
            }
            return __runOriginal;
        }
    }

    [HarmonyPatch(typeof(PathReconstructor), "TiltVerticalEdge", typeof(List<Vector3>), typeof(int), typeof(int))]
    public class Patch3
    {
        static bool Prefix(PathReconstructor __instance, ref List<Vector3> pathCorners, int startIndex, int endIndex)
        { 
            return TimberAPI.DependencyContainer.GetInstance<LadderService>().ChangeVertical(__instance, ref pathCorners, startIndex, endIndex);
        }
    }
}