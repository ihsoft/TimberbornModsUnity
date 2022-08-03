using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.PathSystem;
using TimberbornAPI;
using TimberbornAPI.AssetLoaderSystem.ResourceAssetPatch;
using TimberbornAPI.Common;
using UnityEngine;

namespace MorePaths
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.morepaths";
        public const string PluginName = "More Paths";
        public const string PluginVersion = "1.0.0";
        
        public static ManualLogSource Log;

        void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
            
            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.InGame);
            TimberAPI.DependencyRegistry.AddConfigurator(new MorePathsConfigurator());
            new Harmony(PluginGuid).PatchAll();
        }
    }

    [HarmonyPatch(typeof(TimberApiResourceAssetLoader), "FixMaterialShader", new Type[] {typeof(GameObject), typeof(Shader)})]
    public class ChangeShaderPatch
    {
        static void Prefix(
            ref GameObject obj,
            ref Shader shader)
        {
            if (obj.GetComponent<MeshRenderer>())
            {
                if (obj.GetComponent<MeshRenderer>().materials[0])
                {
                    if (obj.GetComponent<MeshRenderer>().materials[0].name == "StonePath(Version 3) (Instance)" | obj.GetComponent<MeshRenderer>().materials[0].name == "MetalPath (Instance)")
                    {
                        shader = Resources.Load<GameObject>("Buildings/Paths/Path/DirtDrivewayStraightPath")
                            .GetComponent<MeshRenderer>().materials[0].shader;
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(DrivewayModel), "Awake", new Type[] {})]
    public class AwakeDrivewayModelPatch
    {
        static void Postfix(DrivewayModel __instance)
        {
            TimberAPI.DependencyContainer.GetInstance<MorePathsService>().Awake(__instance);
        }
    }
    
    [HarmonyPatch(typeof(DrivewayModel), "UpdateModel", new Type[] {})]
    public class ChangeDrivewayModelPatch
    {
        static void Postfix(DrivewayModel __instance, ref GameObject ____model)
        {
            TimberAPI.DependencyContainer.GetInstance<MorePathsService>().UpdateAllDriveways(__instance, ____model);
        }
    }
}