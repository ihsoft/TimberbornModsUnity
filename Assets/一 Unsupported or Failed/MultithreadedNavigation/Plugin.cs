using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.BehaviorSystem;
using Timberborn.InventorySystem;
using Timberborn.Metrics;
using Timberborn.Navigation;
using Timberborn.NeedBehaviorSystem;
using Timberborn.NeedSystem;
using Timberborn.Planting;
using Timberborn.YielderFinding;
using TimberbornAPI;
using TimberbornAPI.Common;
using UnityEngine;

namespace MultithreadedNavigation
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.multithreadednavigation";
        public const string PluginName = "Multithreaded Navigation";
        public const string PluginVersion = "1.0.0";
        
        public static ManualLogSource Log;
        
        void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
                        
            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.InGame);
            TimberAPI.DependencyRegistry.AddConfigurator(new MultithreadedNavigationConfigurator());
            new Harmony(PluginGuid).PatchAll();
        }
    }
    
    // [HarmonyPatch(typeof(BehaviorManager), "InjectDependencies", typeof(IDayNightCycle), typeof(IDayNightCycle), typeof(TimerMetricCache<RootBehavior>))]
    [HarmonyPatch]
    public class Patch
    {
        static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("BehaviorManager"), "InjectDependencies", new Type[] { AccessTools.TypeByName("IDayNightCycle"), AccessTools.TypeByName("IInstantiator"), typeof(TimerMetricCache<RootBehavior>)});
        }

        static void Postfix(BehaviorManager __instance)
        {
            // Plugin.Log.LogInfo(__instance.name);
            TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().MyBehaviorManagers.Add(new MyBehaviorManager(__instance));
        }
    }
    

    [HarmonyPatch(typeof(BehaviorManager), "Tick")]
    public class BehaviorManagerPatch
    {
        static Stopwatch stopwatch = Stopwatch.StartNew();
        public static long Time;

        private static readonly StackTrace StackTrace = new StackTrace();
        
        // static bool Prefix(BehaviorManager __instance)
        // {
        //     stopwatch.Start();
        //     var test = _stackTrace.GetFrame(2).GetMethod().ReflectedType.Name;
        //     stopwatch.Stop();
        //     Plugin.Log.LogInfo(test);
        //     Plugin.Log.LogFatal(stopwatch.ElapsedTicks);
        //     stopwatch.Reset();
        //     return false;
        // }
        
        static bool Prefix()
        {
            // stopwatch.Start();
            var className = StackTrace.GetFrame(2).GetMethod().ReflectedType.Name;
            // stopwatch.Stop();
            // Plugin.Log.LogFatal(stopwatch.ElapsedTicks);
            // stopwatch.Reset();
            return className == "MultithreadedNavigationJob";
        
            // TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().BehaviorManagers.Add(new MyBehaviorManager(__instance));
        }
    
        // static void Prefix()
        // {
        //     stopwatch.Start();
        //     
        // }
        
        // static void Postfix()
        // {
        //     stopwatch.Stop();
        //     Time += stopwatch.ElapsedTicks;
        //     Plugin.Log.LogFatal(stopwatch.ElapsedTicks);
        //     stopwatch.Reset();
        // }
    }

    [HarmonyPatch]
    public class NotMainThreadPatch
    { 
        static readonly Thread MainThread = Thread.CurrentThread;
        private static readonly object InstantiateLock = new object();
        
        public static IEnumerable<MethodBase> TargetMethods()
        {
            IEnumerable<MethodBase> targetMethods = new[]
            {
                AccessTools.Method(AccessTools.TypeByName("CharacterModel"), "UpdateVisibility"),
                AccessTools.Method(AccessTools.TypeByName("Child"), "GrowUp"),
                AccessTools.Method(AccessTools.TypeByName("Mortal"), "DieIfItIsTime"),
            };

            return targetMethods;
        }
        
        static bool Prefix(object __instance, MethodBase __originalMethod)
        {
            
            lock (InstantiateLock)
            {
                if (Thread.CurrentThread != MainThread)
                {
                    var list = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().RunMethodsOnMainThread.MethodsList;
                    list.AddItem(new RunMethodsOnMainThread.OriginalMethod(__instance, __originalMethod));
                    return false;
                }
            
                return true;
            }
            // var list = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().RunMethodsOnMainThread.MethodsList;
            // list.AddItem(new RunMethodsOnMainThread.OriginalMethod(__instance, __originalMethod));
            // return false;
        }
    }

    [HarmonyPatch]
    public class NonVoidPatches
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            IEnumerable<MethodBase> targetMethods = new[]
            {
                AccessTools.Method(typeof(TerrainAStarPathfinder), "FillFlowFieldWithPath",
                    new[]
                    {
                        typeof(TerrainNavMeshGraph), typeof(PathFlowField), typeof(float), typeof(int),
                        typeof(IReadOnlyList<int>),
                        typeof(int).MakeByRefType()
                    }),
                AccessTools.Method(AccessTools.TypeByName("PathfindingService"), "FindTerrainPathUncached",
                    new[]
                    {
                        typeof(Vector3), typeof(Vector3), typeof(float), typeof(float).MakeByRefType(),
                        typeof(List<Vector3>)
                    }),
                AccessTools.Method(AccessTools.TypeByName("PathfindingService"), "FindTerrainPathUncached",
                    new[]
                    {
                        typeof(Vector3), typeof(IReadOnlyList<int>), typeof(float), typeof(float).MakeByRefType(),
                        typeof(List<Vector3>)
                    }),
                AccessTools.Method(typeof(FlowFieldPathFinder), "FindPathInFlowField",
                    new[]
                    {
                        typeof(PathFlowField), typeof(Vector3), typeof(Vector3), typeof(float).MakeByRefType(),
                        typeof(List<Vector3>)
                    }),
                AccessTools.Method(typeof(FlowFieldPathFinder), "FindPathInFlowField",
                    new[]
                    {
                        typeof(AccessFlowField), typeof(Vector3), typeof(Vector3), typeof(float).MakeByRefType(),
                        typeof(List<Vector3>)
                    }),
                AccessTools.Method(typeof(FlowFieldPathFinder), "FindPathInFlowField",
                    new[]
                    {
                        typeof(AccessFlowField), typeof(RoadSpillFlowField), typeof(Vector3), typeof(Vector3),
                        typeof(float).MakeByRefType(), typeof(List<Vector3>)
                    }),
                AccessTools.Method(typeof(FlowFieldPathFinder), "FindPathInFlowField",
                    new[]
                    {
                        typeof(PathFlowField), typeof(RoadSpillFlowField), typeof(Vector3), typeof(Vector3),
                        typeof(float).MakeByRefType(), typeof(List<Vector3>)
                    }),
                AccessTools.Method(typeof(FlowFieldPathFinder), "FindPathInFlowField",
                    new[]
                    {
                        typeof(Vector3), typeof(IReadOnlyList<Vector3>), typeof(AccessFlowField),
                        typeof(float).MakeByRefType(), typeof(List<Vector3>)
                    }),
                AccessTools.Method(typeof(DistrictDestinationPicker), "GetRandomDestination",
                    new[]
                    {
                        typeof(District), typeof(Vector3)
                    }),
                AccessTools.Method(AccessTools.TypeByName("NavigationService"), "DestinationIsReachableUnlimitedRange",
                    new[]
                    {
                        typeof(Vector3), typeof(Vector3)
                    }),
                AccessTools.Method(AccessTools.TypeByName("DistrictNeedBehaviorService"), "PickBestAction",
                    new[]
                    {
                        typeof(NeedManager), typeof(Vector3), typeof(float), typeof(NeedFilter)
                    }),
                AccessTools.Method(AccessTools.TypeByName("InventoryNeedBehavior"), "Decide",
                    new[]
                    {
                        typeof(GameObject)
                    }),
                AccessTools.Method(AccessTools.TypeByName("ClosestYielderFinder"), "FindYielder",
                    new[]
                    {
                        typeof(Inventory), typeof(int), typeof(IEnumerable<ReachableYielder>)
                    }),
                AccessTools.Method(AccessTools.TypeByName("ClosestYielderFinder"), "FindYielder",
                    new[]
                    {
                        typeof(Inventory), typeof(int), typeof(IEnumerable<ReachableYielder>), typeof(bool)
                    }),
                AccessTools.Method(AccessTools.TypeByName("PlantingCoordinatesFinder"), "FindClosest",
                    new[]
                    {
                        typeof(Vector3), typeof(Plantable)
                    }),
                AccessTools.Method(AccessTools.TypeByName("PlantingCoordinatesFinder"), "GetClosestOrDefault",
                    new[]
                    {
                        typeof(IEnumerable<Vector3Int>)
                    }),
                AccessTools.Method(AccessTools.TypeByName("PlantingCoordinatesFinder"), "GetNeighboring",
                    new[]
                    {
                        typeof(Vector3), typeof(Plantable)
                    }),
                AccessTools.Method(AccessTools.TypeByName("PlantingCoordinatesFinder"), "GetReachable",
                    new[]
                    {
                        typeof(Plantable)
                    }),
            };

            return targetMethods;
        }


        static bool Prefix(object __instance, object[] __args, MethodBase __originalMethod, ref object __result)
        {
            var result = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedNonVoidFunction(__instance, __args, __originalMethod);
            if (result.Item2)
            {
                return true;
            }
        
            __result = result.Item1;
            return false;
        }
        
        // public static void Prefix(object[] __args, MethodBase __originalMethod)
        // {
        //     // use dynamic code to handle all method calls
        //     var parameters = __originalMethod.GetParameters();
        //     Plugin.Log.LogFatal($"Method {__originalMethod.FullDescription()}:");
        //     for (var i = 0; i < __args.Length; i++)
        //         Plugin.Log.LogFatal($"{parameters[i].Name} of type {parameters[i].ParameterType} is {__args[i]}");
        // }
    }

    
    [HarmonyPatch]
    public class VoidPatches
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            IEnumerable<MethodBase> targetMethods = new[]
            {
                AccessTools.Method(typeof(TerrainAStarPathfinder), "FillFlowFieldWithPath",
                    new[]

                    {
                        typeof(TerrainNavMeshGraph), typeof(PathFlowField), typeof(float), typeof(int), typeof(int)
                    }),
                AccessTools.Method(AccessTools.TypeByName("RoadSpillFlowField"), "AddNode",
                    new[]
                    {
                        typeof(int), typeof(int), typeof(int), typeof(float)
                    }),
                AccessTools.Method(AccessTools.TypeByName("RegularRoadFlowFieldGenerator"), "FillFlowField",
                    new[]
                    {
                        typeof(RoadNavMeshGraph), typeof(AccessFlowField), typeof(AccessFlowField), typeof(int)
                    }),
                AccessTools.Method(AccessTools.TypeByName("TerrainFlowFieldGenerator"), "FillFlowFieldUpToDistance",
                    new[]
                    {
                        typeof(TerrainNavMeshGraph), typeof(AccessFlowField), typeof(float), typeof(int)
                    }),
                AccessTools.Method(AccessTools.TypeByName("PlantBehavior"), "ReserveCoordinates",
                    new[]
                    {
                        typeof(GameObject), typeof(bool)
                    }),
                AccessTools.Method(AccessTools.TypeByName("GoodReserver"), "UnreserveStock"),
                AccessTools.Method(AccessTools.TypeByName("NotificationPanel"), "PostLoad"),

            };

            return targetMethods;
        }


        static bool Prefix(object __instance, object[] __args, MethodBase __originalMethod)
        {
            var result = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedVoidFunction(__instance, __args, __originalMethod);
            if (result)
            {
                return true;
            }
        
            return false;
        }
        
        // public static void Prefix(object[] __args, MethodBase __originalMethod)
        // {
        //     // use dynamic code to handle all method calls
        //     var parameters = __originalMethod.GetParameters();
        //     Plugin.Log.LogFatal($"Method {__originalMethod.FullDescription()}:");
        //     for (var i = 0; i < __args.Length; i++)
        //         Plugin.Log.LogFatal($"{parameters[i].Name} of type {parameters[i].ParameterType} is {__args[i]}");
        // }
    }




























    // [HarmonyPatch(typeof(DistrictInterestingBuildingPicker), "GetRandom", typeof(Vector3), typeof(float))]
    // public class DistrictInterestingBuildingPickerPatch
    // {
    //     static bool Prefix(DistrictInterestingBuildingPicker __instance, Vector3 startCoordinates, float preferableMaxDistance, ref InterestingBuilding __result)
    //     {
    //         var result = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedGetRandom(__instance, startCoordinates, preferableMaxDistance);
    //         if (result.Item2)
    //         {
    //             return true;
    //         }
    //             
    //         __result = result.Item1;
    //         return false;
    //     }
    // }
    
    
    // [HarmonyPatch(typeof(RoadSpillFlowField), "AddNode")]
    // public static class MainMenuSceneConfiguratorPatch
    // {
    //     private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         return TestTranspiler.RemoveSpecificationBind(instructions);
    //     }
    // }
    //
    // [HarmonyPatch(typeof(RoadSpillFlowField), "AddNode")]
    // public static class MainMenuSceneConfiguratorPatch
    // {
    //     private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //     {
    //         return TestTranspiler.RemoveSpecificationBind(instructions);
    //     }
    // }
    //
    // public static class TestTranspiler
    // {
    //     public static IEnumerable<CodeInstruction> RemoveSpecificationBind(IEnumerable<CodeInstruction> instructions)
    //     {
    //         // IL instructions
    //         List<CodeInstruction> code = new(instructions);
    //
    //         int startIndex = -1;
    //         int endIndex = -1;
    //
    //         for (int i = 0; i < code.Count - 1; i++)
    //         {
    //             Plugin.Log.LogFatal(code[i]);
    //             
    //             // Search for the ISpecification bind call
    //             if (code[i].opcode != OpCodes.Ldarg_1 || code[i + 1].opcode != OpCodes.Callvirt ||
    //                 code[i + 1].operand.ToString() != "Bindito.Core.IBindingBuilder`1[Timberborn.Persistence.ISpecificationService] Bind[ISpecificationService]()")
    //                 continue;
    //
    //             // Loop to end of call to find last index
    //             for (int j = i; j < code.Count; j++)
    //             {
    //                 if (code[j].opcode != OpCodes.Callvirt || code[j].operand.ToString() != "Void AsSingleton()")
    //                     continue;
    //
    //                 endIndex = j;
    //                 break;
    //             }
    //
    //             startIndex = i;
    //             break;
    //         }
    //
    //         // If method was not found skip
    //         if (startIndex == -1 && endIndex == -1)
    //             return code;
    //
    //         // Removes the method out the IL code
    //         code.RemoveRange(startIndex, endIndex - startIndex + 1);
    //         return code;
    //     }
    // }
}