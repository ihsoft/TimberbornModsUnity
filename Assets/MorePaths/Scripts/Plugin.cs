using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.DependencyContainerSystem;
using TimberApi.ModSystem;
using Timberborn.PathSystem;
using UnityEngine;
using Timberborn.TerrainSystem;

namespace MorePaths
{
    // [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    // [BepInDependency("com.timberapi.timberapi")]
    // [BepInDependency("tobbert.categorybutton")]
    // public class Plugin : BaseUnityPlugin
    // {
    //     public const string PluginGuid = "tobbert.morepaths";
    //     public const string PluginName = "More Paths";
    //     public const string PluginVersion = "1.0.0";
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
    //         TimberAPI.DependencyRegistry.AddConfigurator(new MorePathsConfigurator());
    //         new Harmony(PluginGuid).PatchAll();
    //     }
    // }

    public class Plugin : IModEntrypoint
    {
        public const string PluginGuid = "tobbert.morepaths";
        public const string PluginName = "More Paths";
        public const string PluginVersion = "1.0.0";
        
        public static IConsoleWriter Log;
        
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            Log = consoleWriter;
            
            new Harmony(PluginGuid).PatchAll();
        }
    }
    
    // [HarmonyPatch(typeof(TimberApiResourceAssetLoader), "FixMaterialShader", typeof(GameObject), typeof(Shader))]
    // public class ChangeShaderPatch
    // {
    //     static void Prefix(ref GameObject obj, ref Shader shader)
    //     {
    //         var pathMaterials = TimberAPI.DependencyContainer.GetInstance<MorePathsService>().PathMaterials;
    //         
    //         if (!obj.GetComponent<MeshRenderer>()) return;
    //         if (!obj.GetComponent<MeshRenderer>().materials[0]) return;
    //         if (!pathMaterials.Contains(obj.GetComponent<MeshRenderer>().materials[0].name)) return;
    //         
    //         GameObject gameObject = Resources.Load<GameObject>("Buildings/Paths/Path/DirtDrivewayStraightPath");
    //         Material material = gameObject.GetComponent<MeshRenderer>().materials[0];
    //         shader = material.shader;
    //
    //         obj.GetComponent<MeshRenderer>().materials[0].renderQueue = 2998;
    //     }
    //     
    //     static void Postfix(ref GameObject obj)
    //     {
    //         var pathMaterials = TimberAPI.DependencyContainer.GetInstance<MorePathsService>().PathMaterials;
    //         if (!obj.GetComponent<MeshRenderer>()) return;
    //         if (!obj.GetComponent<MeshRenderer>().materials[0]) return;
    //         if (!pathMaterials.Contains(obj.GetComponent<MeshRenderer>().materials[0].name)) return;
    //         
    //         obj.GetComponent<MeshRenderer>().materials[0].renderQueue = 2998;
    //     }
    // }
    
    [HarmonyPatch(typeof(DrivewayModel), "Awake", new Type[] {})]
    public class AwakeDrivewayModelPatch
    {
        static void Postfix(DrivewayModel __instance)
        {
            DependencyContainer.GetInstance<MorePathsService>().Awake(__instance);
        }
    }
    
    [HarmonyPatch(typeof(DrivewayModel), "UpdateModel", new Type[] {})]
    public class ChangeDrivewayModelPatch
    {
        static void Postfix(DrivewayModel __instance, ref GameObject ____model, ref ITerrainService ____terrainService)
        {
            DependencyContainer.GetInstance<MorePathsService>().UpdateAllDriveways(__instance, ____model, ____terrainService);
        }
    }
    
    // [HarmonyPatch(typeof(EffectDescriber), "DescribeRangeEffects", typeof(IEnumerable<ContinuousEffectSpecification>), typeof(StringBuilder), typeof(StringBuilder), typeof(int))]
    // public class PreventDescriberPatch
    // {
    //     static void Prefix(
    //         ref IEnumerable<ContinuousEffectSpecification> effects,
    //         StringBuilder description,
    //         StringBuilder tooltip,
    //         int range)
    //     {
    //         foreach (var continuousEffectSpecification in effects)
    //         {
    //             if ( continuousEffectSpecification.NeedId == "PathMovementSpeed")
    //             {
    //                 var effectList = effects.ToList(); 
    //                 
    //                 effectList.Remove(effectList.First(x => continuousEffectSpecification.NeedId == "PathMovementSpeed"));
    //                 
    //                 IEnumerable<ContinuousEffectSpecification> test = effectList;
    //             
    //                 effects = test;
    //                 
    //             }
    //         }
    //     }
    // }
    
    // [HarmonyPatch(typeof(RangedEffectBuilding), "RangeNames", new Type[] {})]
    // public class PreventOrangePatch
    // {
    //     static void Postfix(ref IEnumerable<string> __result)
    //     {
    //         foreach (var rangeName in __result)
    //         {
    //             if (rangeName == "MetalPath" | rangeName == "WoodPath.Folktails" | rangeName == "WoodPath.IronTeeth")
    //             {
    //                 __result = Enumerable.Empty<string>();
    //             }
    //         }
    //     }
    // }
    
    // THIS HAS ABUG WITH EDITING MAPS AND PLACING DOWN BUILDINGS IN THE EDITOR
    // [HarmonyPatch(typeof(BlockObjectTool), "Enter", new Type[] {})]
    // public class EnterBlockObjectToolPatch
    // {
    //     static void Prefix(BlockObjectTool __instance)
    //     {
    //         TimberAPI.DependencyContainer.GetInstance<MorePathsService>().previewPrefab = __instance.Prefab;
    //     }
    // }
    //
    // [HarmonyPatch(typeof(BlockObjectTool), "Exit", new Type[] {})]
    // public class ExitBlockObjectToolPatch
    // {
    //     static void Prefix(BlockObjectTool __instance)
    //     {
    //         TimberAPI.DependencyContainer.GetInstance<MorePathsService>().previewPrefab = null;
    //     }
    // }
}