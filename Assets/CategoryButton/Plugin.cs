using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.BottomBarSystem;
using Timberborn.ToolSystem;
using TimberbornAPI;
using TimberbornAPI.Common;
using UnityEngine.UIElements;

namespace CategoryButton
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.categorybutton";
        public const string PluginName = "CategoryButton";
        public const string PluginVersion = "1.1.0";
        
        public static ManualLogSource Log;
        
        void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
                        
            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.InGame);
            TimberAPI.DependencyRegistry.AddConfigurator(new ToolBarCategoriesConfigurator());
            TimberAPI.DependencyRegistry.AddConfigurator(new ToolBarCategoriesConfigurator(), SceneEntryPoint.MapEditor);
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
            BlockObjectToolDescriber ____blockObjectToolDescriber,
            ToolButtonFactory ____toolButtonFactory,
            ref ToolButton __result)
        {
            if (prefab.TryGetComponent(out CategoryButtonComponent toolBarCategory))
            {
                __result = TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().CreateFakeToolButton(prefab, toolGroup, buttonParent, toolBarCategory, ____blockObjectToolDescriber, ____toolButtonFactory);
                return false;
            }
            
            return true;
        }
        
        static void Postfix(
            BlockObjectToolButtonFactory __instance,
            PlaceableBlockObject prefab,
            ToolGroup toolGroup,
            VisualElement buttonParent,
            ToolButton __result)
        {
            TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().AddButtonToCategory(__result, prefab);
        }
    }
    
    [HarmonyPatch(typeof(BottomBarPanel), "InitializeSections", new Type[] {})]
    public class ToolGroupFactoryPatch
    {
        static void Postfix(BottomBarPanel __instance, VisualElement ____mainElements, VisualElement ____subElements)
        {
            TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().AddButtonsToCategory();
        }
    }
    
    [HarmonyPatch(typeof(ToolManager), "SwitchTool", new Type[] {typeof(Tool)})]
    public class ToolSwitcherPatch
    {
        static void Prefix(ref ToolManager __instance, Tool tool)
        {
            TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().SaveOrExitCategoryTool(__instance.ActiveTool, tool);
        }
    }
    
    [HarmonyPatch(typeof(ToolManager), "ExitTool", new Type[] {})]
    public class ExitToolPatch
    {
        static bool Prefix(ref ToolManager __instance)
        {
            foreach (var toolBarCategoryTool in TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().ToolBarCategoryTools)
            {
                if (__instance.ActiveTool == toolBarCategoryTool)
                {
                    return false;
                }
            }

            return true;
        }
    }
    
    // [HarmonyPatch(typeof(ScreenSettingsController), "UpdateSettings", new Type[] {})]
    // public class ScreenSettingsPatch
    // {
    //     static void Postfix(ref ScreenSettingsController __instance, ScreenSettings ____screenSettings)
    //     {
    //         try { TimberAPI.DependencyContainer.GetInstance<CategoryButtonService>().UpdateScreenSize(); }
    //         catch (BindingNotFoundException e) {}
    //     }
    // }
}