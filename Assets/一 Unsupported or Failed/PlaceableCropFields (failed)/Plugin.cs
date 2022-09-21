// using System;
// using System.Collections.Generic;
// using System.Linq;
// using BepInEx;
// using BepInEx.Logging;
// using HarmonyLib;
// using Timberborn.AreaSelectionSystem;
// using Timberborn.BlockObjectTools;
// using Timberborn.BlockSystem;
// using Timberborn.Common;
// using Timberborn.Core;
// using Timberborn.Planting;
// using Timberborn.PlantingUI;
// using Timberborn.TerrainSystem;
// using Timberborn.ToolSystem;
// using TimberbornAPI;
// using TimberbornAPI.Common;
// using UnityEngine;
// using UnityEngine.UIElements;
//
// namespace PlaceableCropFields
// {
//     [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
//     [BepInDependency("com.timberapi.timberapi")]
//     public class Plugin : BaseUnityPlugin
//     {
//         public const string PluginGuid = "tobbert.placeablecropfields";
//         public const string PluginName = "Placeable Crop Fields";
//         public const string PluginVersion = "1.0.0";
//         
//         public static ManualLogSource Log;
//         
//         void Awake()
//         {
//             Log = Logger;
//             
//             Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
//                         
//             TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.InGame);
//             TimberAPI.DependencyRegistry.AddConfigurator(new PlaceableCropFieldsConfigurator());
//             TimberAPI.DependencyRegistry.AddConfigurator(new PlaceableCropFieldsConfigurator(), SceneEntryPoint.MapEditor);
//             new Harmony(PluginGuid).PatchAll();
//         }
//     }
//     
//     // [HarmonyPatch(typeof(AreaPicker), "GetTerrainBlocks", typeof(Ray), typeof(Ray))]
//     // public class BlockObjectToolPatch
//     // {
//     //     static bool Prefix(AreaPicker __instance, Ray startRay, Ray endRay,  TerrainPicker ____terrainPicker, AreaIterator ____areaIterator, ref IEnumerable<Vector3Int> __result)
//     //     {
//     //         return TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test(__instance, startRay, endRay, ____terrainPicker, ____areaIterator, __result);
//     //     }
//     // }
//     
//     // [HarmonyPatch(typeof(AreaPicker), "GetTerrainBlocks", typeof(Ray), typeof(Ray))]
//     // public class BlockObjectToolPatch
//     // {
//     //     // static bool Prefix(AreaPicker __instance, Ray startRay, Ray endRay,  TerrainPicker ____terrainPicker, AreaIterator ____areaIterator, ref IEnumerable<Vector3Int> __result)
//     //     // {
//     //     //     return TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test(__instance, startRay, endRay, ____terrainPicker, ____areaIterator, __result);
//     //     // }
//     //     
//     //     static void Postix(AreaPicker __instance, Ray startRay, Ray endRay,  TerrainPicker ____terrainPicker, AreaIterator ____areaIterator, ref IEnumerable<Vector3Int> __result)
//     //     {
//     //         foreach (var vector3 in __result)
//     //         {
//     //             Plugin.Log.LogFatal(vector3);
//     //         }
//     //         TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test(__instance, startRay, endRay, ____terrainPicker, ____areaIterator, __result);
//     //     }
//     // }
//     
//     // [HarmonyPatch(typeof(TerrainPicker), "PickTerrainCoordinates", typeof(Ray))]
//     // public class TerrainPatch
//     // {
//     //     // static bool Prefix(AreaPicker __instance, Ray startRay, Ray endRay,  TerrainPicker ____terrainPicker, AreaIterator ____areaIterator, ref IEnumerable<Vector3Int> __result)
//     //     // {
//     //     //     return TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test(__instance, startRay, endRay, ____terrainPicker, ____areaIterator, __result);
//     //     // }
//     //     
//     //     static void Postfix(AreaPicker __instance, Ray ray, TraversedCoordinates? __result)
//     //     {
//     //         if (__result.HasValue)
//     //         {
//     //             Plugin.Log.LogFatal(__result.Value.Coordinates);
//     //         }
//     //         // foreach (var vector3 in __result)
//     //         // {
//     //         //     Plugin.Log.LogFatal(vector3);
//     //         // }
//     //         // TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test(__instance, startRay, endRay, ____terrainPicker, ____areaIterator, __result);
//     //     }
//     // }
//     
//     [HarmonyPatch(typeof(TerrainPicker), "PickCoordinates", typeof(Ray), typeof(Predicate<Vector3Int>))]
//     public class Terrain2Patch
//     {
//         // static bool Prefix(AreaPicker __instance, Ray startRay, Ray endRay,  TerrainPicker ____terrainPicker, AreaIterator ____areaIterator, ref IEnumerable<Vector3Int> __result)
//         // {
//         //     return TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test(__instance, startRay, endRay, ____terrainPicker, ____areaIterator, __result);
//         // }
//         
//         static bool Prefix(AreaPicker __instance, Ray ray, Predicate<Vector3Int> predicate, ref TraversedCoordinates? __result)
//         {
//             if (TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().IsPlantingTool())
//             {
//                 __result = TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test2(__instance, ray, predicate, __result);
//                 
//                 return false;
//             }
//             
//             return true;
//             // Plugin.Log.LogFatal(__result.Value.Coordinates);
//             // foreach (var vector3 in __result)
//             // {
//             //     Plugin.Log.LogFatal(vector3);
//             // }
//             // TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test(__instance, startRay, endRay, ____terrainPicker, ____areaIterator, __result);
//         }
//     }
//     
//     [HarmonyPatch(typeof(TerrainAreaService), "InMapLeveledCoordinates", typeof(IEnumerable<Vector3Int>), typeof(Ray))]
//     public class Terrain3Patch
//     {
//         static void Postfix(TerrainPicker __instance, IEnumerable<Vector3Int> inputBlocks, Ray ray, ITerrainService ____terrainService,TerrainPicker ____terrainPicker, ref IEnumerable<Vector3Int> __result)
//         {
//
//             if (TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().IsPlantingTool())
//             {
//                 // var test = ____terrainPicker.PickTerrainCoordinates(ray);
//                 // if (test.HasValue)
//                 // {
//                 //     Plugin.Log.LogFatal(test.Value.Coordinates);
//                 // }
//                 TraversedCoordinates? nullable = ____terrainPicker.PickTerrainCoordinates(ray);
//                 int startHeight = nullable.HasValue ? nullable.GetValueOrDefault().Coordinates.z + 1 : 0;
//                 
//                 
//                 List<Vector3Int> newInputBlocks = new List<Vector3Int>();
//                 foreach (var inputBlock in inputBlocks)
//                 {
//                     int height;
//                     if (____terrainService.TryGetCellHeight(inputBlock.XY(), out height) && startHeight == height)
//                     {
//                         newInputBlocks.Add(new Vector3Int(inputBlock.x, inputBlock.y, height));
//                         continue;
//                     }
//                     
//                     if (TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().IsPlaceableCropField(inputBlock))
//                     {
//                         var newInputBlock = inputBlock;
//                         newInputBlock.z += 1;
//                         newInputBlocks.Add(newInputBlock);
//                     }
//                 }
//
//                 __result = newInputBlocks;
//             }
//             // if (TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().Test3(__instance, inputBlocks, ray, __result))
//             // {
//             //     __result = inputBlocks;
//             //     // List<Vector3Int> newInputBlocks = new List<Vector3Int>();
//             //     // foreach (var vector3Int in inputBlocks)
//             //     // {
//             //     //     var v3 = vector3Int;
//             //     //     v3.z += 1;
//             //     //     newInputBlocks.Add(v3);
//             //     // }
//             //     //
//             //     // IEnumerable<Vector3Int> list = newInputBlocks;
//             //     // __result = list;
//             // }
//         }
//     }
//     
//     [HarmonyPatch(typeof(PlantingAreaValidator), "CanPlant", typeof(Vector3Int), typeof(string))]
//     public class CanPlantPatch
//     {
//         static void Postfix(Vector3Int coordinates, string name, ref bool __result)
//         {
//             if (TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().IsPlantingTool())
//             {
//                 if (TimberAPI.DependencyContainer.GetInstance<PlaceableCropFieldsService>().IsPlaceableCropField(coordinates))
//                 {
//                     __result = true;
//                 }
//             }
//         }
//     }
// }