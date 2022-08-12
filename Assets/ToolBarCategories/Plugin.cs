using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.InputSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using Timberborn.WaterSystemRendering;
using TimberbornAPI;
using TimberbornAPI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace ToolBarCategories
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.toolbarcategories";
        public const string PluginName = "ToolBarCategories";
        public const string PluginVersion = "1.0.0";
        
        public static ManualLogSource Log;
        
        void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
                        
            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.InGame);
            TimberAPI.DependencyRegistry.AddConfigurator(new ToolBarCategoriesConfigurator());
            new Harmony(PluginGuid).PatchAll();
        }
    }
    
    [HarmonyPatch(typeof(BlockObjectToolButtonFactory), "Create", new Type[] {typeof(PlaceableBlockObject), typeof(ToolGroup), typeof(VisualElement)})]
    public class BlockObjectToolPatch
    {
        static bool Prefix(
            BlockObjectToolButtonFactory __instance,
            PlaceableBlockObject prefab,
            ToolGroup toolGroup,
            VisualElement buttonParent,
            ToolButton __result)
        {
            GameObject gameObject = TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().GetObject();
    
            List<PlaceableBlockObject> list = new List<PlaceableBlockObject>();
            list.Add(gameObject.GetComponent<PlaceableBlockObject>());
            
            if (prefab.TryGetComponent(out ToolBarCategory toolBarCategory))
            {
                // foreach (var blockObject in list)
                // {
                //     if (blockObject.UsableWithCurrentFeatureToggles)
                //     {
                //         
                //     }
                // }
                
                __result = TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().CreateFakeToolButton(prefab, (ToolGroup) toolGroup, buttonParent, toolBarCategory);
                return false;
            }
            
            return true;
            // TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().Show();
        }
        
        static void Postfix(
            BlockObjectToolButtonFactory __instance,
            PlaceableBlockObject prefab,
            ToolGroup toolGroup,
            VisualElement buttonParent,
            ToolButton __result)
        {
            TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().TryToAddButtonToCategory(__result, prefab);
        }
    }
    
    [HarmonyPatch(typeof(ToolManager), "SwitchTool", new Type[] {typeof(Tool)})]
    public class ToolSwitcherPatch
    {
        static void Prefix(
            ref ToolManager __instance, 
            Tool tool, 
            InputService ____inputService,
            Tool ____defaultTool, 
            EventBus ____eventBus,
            WaterOpacityToggle ____waterOpacityToggle)
        {
            foreach (var toolBarCategoryTool in TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>()
                         .ToolBarCategoryTools)
            {
                TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>()
                    .SaveOrExitCategoryTool(__instance.ActiveTool, tool, toolBarCategoryTool);
            }
        }
        
        // static bool Prefix(
        //     ref ToolManager __instance, 
        //     Tool tool, 
        //     InputService ____inputService,
        //     Tool ____defaultTool, 
        //     EventBus ____eventBus,
        //     WaterOpacityToggle ____waterOpacityToggle)
        // {
        //     if (__instance.ActiveTool != null && __instance.ActiveTool.ToString() == "ToolBarCategories.ToolBarCategoryTool")
        //     { 
        //         bool runOriginal = TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().PreventToolSwitch(
        //             __instance,
        //             __instance.ActiveTool, 
        //             tool, 
        //             ____defaultTool, 
        //             ____inputService, 
        //             ____eventBus,
        //             ____waterOpacityToggle);
        //         
        //         Plugin.Log.LogInfo("Toolbar active");
        //         Plugin.Log.LogInfo("runOriginal: " + runOriginal);
        //         
        //         return runOriginal;
        //     }
        //     else
        //     {
        //         TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().DisableAllCategoryTools(
        //             __instance,
        //             __instance.ActiveTool, 
        //             tool, 
        //             ____defaultTool, 
        //             ____inputService, 
        //             ____eventBus,
        //             ____waterOpacityToggle);
        //         return true;
        //     }
        // }
        
        [HarmonyPatch(typeof(ToolManager), "ExitTool", new Type[] {})]
        public class ExitToolPatch
        {
            static bool Prefix(ref ToolManager __instance, ref Tool __state)
            {
                foreach (var toolBarCategoryTool in TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().ToolBarCategoryTools)
                {
                    if (__instance.ActiveTool == toolBarCategoryTool)
                    {
                        __state = __instance.ActiveTool;
                        
                        bool runOriginal = TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().ShouldExitToolBeSkipped();
                        
                        
                        return runOriginal;
                    }
                }

                return true;
            }
        }
        
        // static void Postfix(ref ToolManager __instance, bool __state)
        // {
        //     if (__state)
        //     {
        //         TimberAPI.DependencyContainer.GetInstance<ToolBarCategoriesService>().SetActiveTool(__instance.ActiveTool); 
        //     }
        // }
    }
}