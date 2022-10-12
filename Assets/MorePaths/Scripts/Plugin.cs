using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.DependencyContainerSystem;
using TimberApi.ModSystem;
using Timberborn.AssetSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.Coordinates;
using Timberborn.Core;
using Timberborn.PathSystem;
using UnityEngine;
using Timberborn.TerrainSystem;

namespace MorePaths
{
    public class Plugin : IModEntrypoint
    {
        public const string PluginGuid = "tobbert.morepaths";
        public static string myPath;
        
        public static IConsoleWriter Log;
        
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            myPath = mod.DirectoryPath;
            
            Log = consoleWriter;

            new Harmony(PluginGuid).PatchAll();
        }
    }

    [HarmonyPatch(typeof(DrivewayModel), "Awake", new Type[] {})]
    public class AwakeDrivewayModelPatch
    {
        static void Postfix(DrivewayModel __instance)
        {
            DependencyContainer.GetInstance<MorePathsService>().InstantiateDriveways(__instance);
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
    
    [HarmonyPatch]
    [HarmonyPriority(399)]
    public class BlockObjectToolPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("FactionObjectCollection"), "GetObjects");
        }
        
        static void Postfix(ref IEnumerable<UnityEngine.Object> __result)
        {
            DependencyContainer.GetInstance<MorePathsService>().AddFakePathsToObjectsPatch(ref __result);
        }
    }
    
    
    [HarmonyPatch]
    public class PreventInstantiatePatch
    {
        public static bool RunInstantiate = true;
        
        public static IEnumerable<MethodInfo> TargetMethods()
        {
            GameObject originalPathGameObject = new ResourceAssetLoader().Load<GameObject>("Buildings/Paths/Path/Path.IronTeeth");
            var list = originalPathGameObject.GetComponents<object>();

            var methodInfoList = new List<MethodInfo>();

            List<string> test = new List<string>()
            {
                "Prefab",
                "BuildingConstructionRegistrar",
                "PlaceableBlockObject",
                "LabeledPrefab",
            };

            foreach (var obj in list)
            {
                var name = obj.GetType().Name;
                if (!test.Contains(name))
                {
                    methodInfoList.Add(AccessTools.Method(AccessTools.TypeByName(name), "Awake"));
                }
                
            }
            
            methodInfoList.Add(AccessTools.Method(AccessTools.TypeByName("BuildingModel"), "Start"));

            return methodInfoList;
        }
        static bool Prefix()
        {
            return RunInstantiate;
        }
    }
    
    [HarmonyPatch]
    public class CreateFakePathsPatch
    {
        public static MethodInfo TargetMethod()
        { 
            return AccessTools.Method(AccessTools.TypeByName("BuildingModel"), "Start");
        }
        static bool Prefix(BuildingModel __instance)
        {
            if (!DependencyContainer.GetInstance<MapEditorMode>().IsMapEditor)
            {
                return !DependencyContainer.GetInstance<MorePathsService>().FakePaths.Select(path => path.PathGameObject).Contains(__instance.gameObject);
            }

            return true;
        }
    }
    
    [HarmonyPatch]
    public class DeletePathCornersPatch
    {
        public static MethodInfo TargetMethod()
        { 
            return AccessTools.Method(AccessTools.TypeByName("AreaPicker"), "GetBlocks");
        }
        static void Postfix(PlaceableBlockObject blockObject, ref (IEnumerable<Vector3Int>, Orientation) __result)
        {
            if (blockObject.TryGetComponent(out DynamicPathModel dynamicPathModel))
            {
                __result.Item2 = Orientation.Cw0;
            }
        }
    }
    
    [HarmonyPatch(typeof(BlockObjectTool), "RotateClockwise")]
    public class RotateClockwisePatch
    {
        static bool Prefix(BlockObjectTool __instance)
        {
            return !__instance.Prefab.TryGetComponent(out DynamicPathModel dynamicPathModel);
        }
    }
    
    [HarmonyPatch(typeof(BlockObjectTool), "RotateCounterclockwise")]
    public class RotateCounterclockwisePatch
    {
        static bool Prefix(BlockObjectTool __instance)
        {
            return !__instance.Prefab.TryGetComponent(out DynamicPathModel dynamicPathModel);
        }
    }
    
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