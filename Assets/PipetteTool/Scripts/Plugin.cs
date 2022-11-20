using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.DependencyContainerSystem;
using TimberApi.ModSystem;
using Timberborn.AssetSystem;
using Timberborn.BlockSystem;
using Timberborn.Planting;
using Timberborn.SelectionSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using UnityEngine.UIElements;

namespace PipetteTool
{
    public class Plugin : IModEntrypoint
    {
        public const string PluginGuid = "tobbert.pipettetool";
        
        public static IConsoleWriter Log;
        
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            Log = consoleWriter; 
            
            new Harmony(PluginGuid).PatchAll();
        }
    }
    
    [HarmonyPatch]
    public class BlockObjectToolButtonFactoryPatch
    {
        public static IEnumerable<MethodInfo> TargetMethods()
        {
            return new List<MethodInfo>()
            {
                AccessTools.Method(AccessTools.TypeByName("BlockObjectToolButtonFactory"), "Create", new[]
                {
                    typeof(PlaceableBlockObject), typeof(ToolGroup), typeof(VisualElement)
                }),
                AccessTools.Method(AccessTools.TypeByName("BlockObjectToolButtonFactory"), "Create", new []
                {
                    typeof(PlaceableBlockObject)
                })
            };
        }
        
        static void Postfix(PlaceableBlockObject prefab, ref ToolButton __result)
        {
            if (prefab.TryGetComponent(out BlockObject _))
            {
                DependencyContainer.GetInstance<IPipetteTool>().AddToolButtonToDictionary(prefab.gameObject, __result);
            }
        }
    }
    
    [HarmonyPatch]
    public class PlantingToolButtonFactoryPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("PlantingToolButtonFactory"), "CreatePlantingTool", new []
            {
                typeof(Plantable), typeof(VisualElement), typeof(ToolGroup)
            });
        }
        
        static void Postfix(Plantable plantable, ref ToolButton __result)
        {
            if (plantable.TryGetComponent(out BlockObject _))
            {
                DependencyContainer.GetInstance<IPipetteTool>().AddToolButtonToDictionary(plantable.gameObject, __result);
            }
        }
    }
    
    [HarmonyPatch]
    public class SelectionManagerPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("SelectionManager"), "SelectSelectable", new []{typeof(SelectableObject)});
        }
        
        static void Postfix(SelectableObject target)
        {
            DependencyContainer.GetInstance<IPipetteTool>().OnSelectableObjectSelected(target.gameObject);
        }
    }
    
    [HarmonyPatch]
    public class CursorServicePatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("CursorService"), "GetCursor", new []{typeof(string)});
        }
        
        static bool Prefix(ref string cursorName, IResourceAssetLoader ____resourceAssetLoader, ref object __result)
        {
            if (cursorName == DependencyContainer.GetInstance<IPipetteTool>().CursorKey)
            {
                __result = ____resourceAssetLoader.Load<Object>("tobbert.pipettetool/tobbert_pipettetool/PipetteToolCursor");
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch]
    public class ConstructionModeServicePatch
    {
        public static bool SkipNext;
        
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("ConstructionModeService"), "CanExitConstructionMode");
        }
        
        static bool Prefix(ref bool __result)
        {
            if (!SkipNext) 
                return true;
            
            SkipNext = false;
            __result = true;
            return false;

        }
    }
}