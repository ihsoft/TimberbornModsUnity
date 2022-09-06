using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.BehaviorSystem;
using Timberborn.BottomBarSystem;
using Timberborn.EnterableSystem;
using Timberborn.Navigation;
using Timberborn.WalkingSystem;
using TimberbornAPI;
using TimberbornAPI.Common;
using UnityEngine;
using UnityEngine.UIElements;

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
    
    [HarmonyPatch(typeof(BehaviorManager), "ProcessBehaviors")]
    public class BehaviorManagerPatch
    {
        static bool Prefix(
            BehaviorManager __instance,
            bool ____returnToBehavior,
            Behavior ____runningBehavior,
            List<RootBehavior> ____rootBehaviors)
        {
            Plugin.Log.LogFatal(Traverse.Create(__instance).Property("gameObject"));
            TimberAPI.DependencyContainer.GetInstance<MultithreadedNavigationService>().BehaviorManagers.Add(new MyBehaviorManager(__instance, ____returnToBehavior, ____runningBehavior, ____rootBehaviors));
            return false;
        }
    }
}