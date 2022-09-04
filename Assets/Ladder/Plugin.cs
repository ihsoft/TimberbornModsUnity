using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.EntitySystem;
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
    
    [HarmonyPatch(typeof(Accessible))]
    public class Patch4
    {
        static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(Accessible), "FindRoadToTerrainPath", new Type[] { typeof(Accessible), typeof(Vector3).MakeByRefType(), typeof(float).MakeByRefType()});
        }
        
        static void Postfix(
            Accessible end,
            Vector3 endOfRoad,
            float totalDistance,
            ref bool __result)
        {
            if (!end.Accesses.Any()) return;
            
            bool flag1 = end.Accesses.Last().x == -1;
            bool flag2 = end.Accesses.Last().y == -1;
            bool flag3 = end.Accesses.Last().z == -1;
            if (flag1 && flag2 && flag3)
            {
                __result = true;
                endOfRoad = end.Accesses.ToList()[-2];
                Plugin.Log.LogFatal(end.Accesses.ToList()[-1]);
                Plugin.Log.LogFatal(end.Accesses.ToList()[-2]);
            }

            Plugin.Log.LogInfo(__result);
            Plugin.Log.LogFatal(endOfRoad);
            Plugin.Log.LogFatal(totalDistance);
        }
    }
    
    [HarmonyPatch(typeof(Accessible))]
    public class Patch5
    {
        static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(Accessible), "Enable", new Type[] { typeof(IEnumerable<Vector3>), typeof(Vector3)});
        }
        
        static void Postfix(Accessible __instance, ref IEnumerable<Vector3> accesses)
        {

            // while ((obj = TimberAPI.DependencyContainer.GetInstance<BlockService>()
            //        .GetObjectsWithComponentAt<Prefab>(__instance.GetComponent<BlockObject>().Coordinates)
            //        .Where(item => item.PrefabName.Contains("Ladder"))) ==  )
            // {
            //     var objects = .PrefabName.Contains("Ladder");
            // }
            
            var list = accesses.ToList();

            var coords = __instance.GetComponent<BlockObject>().Coordinates;
            var checkingCoords = coords;

            checkingCoords.y -= 1;

            var objects = TimberAPI.DependencyContainer.GetInstance<BlockService>().GetObjectsWithComponentAt<Prefab>(checkingCoords);
            foreach (var prefab in objects)
            {
                if (prefab.PrefabName.Contains("Ladder"))
                {
                    var buildingModel = prefab.GetComponent<BuildingModel>();
                    var type = typeof(BuildingModel).GetField("_unfinishedModel", BindingFlags.NonPublic | BindingFlags.Instance);
                    var value1 = type.GetValue(buildingModel) is bool ? (bool)type.GetValue(buildingModel) : false;
                    if (value1) continue;
                    list.Add(new Vector3(coords.x + 0.5f + 1, coords.z, coords.y + 0.5f));
                    list.Add(new Vector3(-1, -1, -1));
                    break;
                }
            }

            accesses = list;
            
            Plugin.Log.LogInfo(accesses.FirstOrDefault());
            foreach (var VARIABLE in accesses)
            {
                Plugin.Log.LogFatal(VARIABLE);
            }
            Plugin.Log.LogInfo(__instance.GetComponent<BlockObject>().Coordinates);
        }
    }
}