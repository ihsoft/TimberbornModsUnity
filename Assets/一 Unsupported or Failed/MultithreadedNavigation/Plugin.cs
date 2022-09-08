using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.BehaviorSystem;
using Timberborn.BottomBarSystem;
using Timberborn.CharacterModelSystem;
using Timberborn.EnterableSystem;
using Timberborn.Navigation;
using Timberborn.NeedBehaviorSystem;
using Timberborn.NeedSystem;
using Timberborn.SlotSystem;
using Timberborn.WalkingSystem;
using TimberbornAPI;
using TimberbornAPI.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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
    
    // [HarmonyPatch(typeof(Walker), "GoTo", typeof(IDestination))]
    // public class WalkerPatch
    // {
    //     static void Postfix(
    //         Walker __instance,
    //         IDestination destination,
    //         IDestination ____currentDestination,
    //         IDestination ____previousDestination,
    //         Accessible ____accessibleOccupiedOnGoTo,
    //         Enterer ____enterer)
    //     {
    //         TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>();
    //     }
    // }
    
    [HarmonyPatch(typeof(BehaviorManager), "Tick")]
    public class BehaviorManagerPatch
    {
        private static Stopwatch _stopwatch = new Stopwatch();
        static bool Prefix(
            BehaviorManager __instance
            // ref bool ____returnToBehavior,
            // ref Behavior ____runningBehavior,
            // ref List<RootBehavior> ____rootBehaviors,
            // ref IExecutor ____runningExecutor
            )
        {
            // _stopwatch.Start();
            
            
            // Plugin.Log.LogFatal(Traverse.Create(__instance).Property("gameObject"));
            // Plugin.Log.LogFatal("reset");
            // Plugin.Log.LogInfo(____runningBehavior == null);
            // Plugin.Log.LogFatal((bool)(Object)____runningBehavior);
            foreach (var myBehaviorManager in TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().BehaviorManagers)
            {
                // Plugin.Log.LogFatal(Traverse.Create(__instance).Property("gameObject"));
                // Plugin.Log.LogFatal(Traverse.Create(myBehaviorManager.behaviorManager).Property("gameObject").GetValue<GameObject>());
                // Plugin.Log.LogInfo(Traverse.Create(__instance).Property("gameObject").GetValue<GameObject>() == Traverse.Create(myBehaviorManager.behaviorManager).Property("gameObject").GetValue<GameObject>());
                if (Traverse.Create(__instance).Property("gameObject").GetValue<GameObject>() == Traverse.Create(myBehaviorManager.behaviorManager).Property("gameObject").GetValue<GameObject>())
                {
                    // Plugin.Log.LogFatal("prevent");
                    return true;
                }
            }
            
            // TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().BehaviorManagers.Add(new MyBehaviorManager(__instance, ref ____returnToBehavior, ref ____runningBehavior, ref ____rootBehaviors, ref ____runningExecutor));
            TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().BehaviorManagers.Add(new MyBehaviorManager(__instance));
            return false;
        }
        
        
        // static void Postfix()
        // {
        //     _stopwatch.Stop();
        //     Plugin.Log.LogFatal(_stopwatch.ElapsedTicks);
        // }
        
    }
    
    // [HarmonyPatch(typeof(DistrictNeedBehaviorService), "PickBestAction", typeof(NeedManager), typeof(Vector3), typeof(float), typeof(NeedFilter))]
    // public class DistrictNeedBehaviorServicePatch
    // {
    //     static bool Prefix(
    //         DistrictNeedBehaviorService __instance,
    //         NeedManager needManager,
    //         Vector3 essentialActionPosition,
    //         float hoursLeftForNonEssentialActions,
    //         NeedFilter needFilter,
    //         ref AppraisedAction? __result)
    //     {
    //         var result = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedPickBestAction(__instance,  needManager, essentialActionPosition, hoursLeftForNonEssentialActions, needFilter);
    //         if (result.Item2)
    //         {
    //             return true;
    //         }
    //     
    //         __result = result.Item1;
    //         return false;
    //     }
    // }
    //
    // [HarmonyPatch(typeof(DistrictDestinationPicker), "GetRandomDestination", typeof(District), typeof(Vector3))]
    // public class DistrictDestinationPickerPatch
    // {
    //     private static Stopwatch _stopwatch = new Stopwatch();
    //
    //     static bool Prefix(
    //         DistrictDestinationPicker __instance,
    //         District district, 
    //         Vector3 coordinates,
    //         ref Vector3 __result)
    //     {
    //         _stopwatch.Start();
    //         var result = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedGetRandomDestination(__instance,  district, coordinates);
    //         if (result.Item2)
    //         {
    //             return true;
    //         }
    //     
    //         __result = result.Item1;
    //         return false;
    //     }
    //     
    //     // static void Postfix()
    //     // {
    //     //     _stopwatch.Stop();
    //     //     Plugin.Log.LogFatal(_stopwatch.ElapsedTicks);
    //     //     _stopwatch.Reset();
    //     // }
    // }
    //
    // // [HarmonyPatch(typeof(SlotManager), "AddEnterer", typeof(Enterer), typeof(bool))]
    // // public class SlotManagerPatch
    // // {
    // //     static bool Prefix(
    // //         SlotManager __instance,
    // //         Enterer enterer, 
    // //         bool assign,
    // //         ref bool __result)
    // //     {
    // //         var result = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedAddEnterer(__instance, enterer, assign);
    // //         if (result.Item2)
    // //         {
    // //             return true;
    // //         }
    // //     
    // //         __result = result.Item1;
    // //         return false;
    // //     }
    // // }
    //
    // [HarmonyPatch(typeof(CharacterModel), "UpdateVisibility")]
    // public class CharacterModelPatch
    // {
    //     static bool Prefix(CharacterModel __instance)
    //     {
    //         // var result = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedUpdateVisibility(__instance);
    //         // if (result)
    //         // {
    //         //     return true;
    //         // }
    //     
    //         return false;
    //     }
    // }
    //
    //
    // [HarmonyPatch(typeof(RoadSpillFlowField), "AddNode", typeof(int), typeof(int), typeof(int), typeof(float))]
    // public class RoadSpillFlowFieldPatch
    // {
    //     static bool Prefix(RoadSpillFlowField __instance, int nodeId, int parentNodeId, int roadParentNodeId, float distanceToRoad)
    //     {
    //         var result = TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedAddNode(__instance, nodeId, parentNodeId, roadParentNodeId, distanceToRoad);
    //         if (result)
    //         {
    //             return true;
    //         }
    //     
    //         return false;
    //         
    //         
    //         // TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().LockedAddNode(ref ____nodes, nodeId, parentNodeId, roadParentNodeId, distanceToRoad);
    //         // return false;
    //     }
    // }
    
    
    
    
    
    
    
    
    
    
    
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