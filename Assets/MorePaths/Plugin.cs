using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.BlockObjectTools;
using Timberborn.BuildingRange;
using Timberborn.BuildingsUI;
using Timberborn.Effects;
using Timberborn.EntityPanelSystem;
using Timberborn.NeedSpecifications;
using Timberborn.PathSystem;
using Timberborn.PreviewSystem;
using TimberbornAPI;
using TimberbornAPI.AssetLoaderSystem.ResourceAssetPatch;
using TimberbornAPI.Common;
using UnityEngine;
using Timberborn.RangedEffectSystem;
using Timberborn.TerrainSystem;

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
        static void Postfix(DrivewayModel __instance, ref GameObject ____model, ref ITerrainService ____terrainService)
        {
            TimberAPI.DependencyContainer.GetInstance<MorePathsService>().UpdateAllDriveways(__instance, ____model, ____terrainService);
        }
    }
    
    [HarmonyPatch(typeof(EffectDescriber), "DescribeRangeEffects", new Type[] {typeof(IEnumerable<ContinuousEffectSpecification>), typeof(StringBuilder), typeof(StringBuilder), typeof(int)})]
    public class PreventDescriberPatch
    {
        static void Prefix(
            ref IEnumerable<ContinuousEffectSpecification> effects,
            StringBuilder description,
            StringBuilder tooltip,
            int range)
        {
            ;
            foreach (var continuousEffectSpecification in effects)
            {
                if ( continuousEffectSpecification.NeedId == "PathMovementSpeed")
                {
                    var effectList = effects.ToList(); 
                    
                    effectList.Remove(effectList.First(x => continuousEffectSpecification.NeedId == "PathMovementSpeed"));
                    
                    IEnumerable<ContinuousEffectSpecification> test = effectList;
                
                    effects = test;
                    
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(RangedEffectBuilding), "RangeNames", new Type[] {})]
    public class PreventOrangePatch
    {
        static void Postfix(ref IEnumerable<string> __result)
        {
            foreach (var rangeName in __result)
            {
                if (rangeName == "StonePath" | rangeName == "MetalPath")
                {
                    __result = Enumerable.Empty<string>();
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(BlockObjectTool), "Enter", new Type[] {})]
    public class EnterBlockObjectToolPatch
    {
        static void Prefix(BlockObjectTool __instance)
        {
            TimberAPI.DependencyContainer.GetInstance<MorePathsService>().previewPrefab = __instance.Prefab;
        }
    }
    
    [HarmonyPatch(typeof(BlockObjectTool), "Exit", new Type[] {})]
    public class ExitBlockObjectToolPatch
    {
        static void Prefix(BlockObjectTool __instance)
        {
            TimberAPI.DependencyContainer.GetInstance<MorePathsService>().previewPrefab = null;
        }
    }
}