using System.Collections.Generic;
using System.Reflection;
using Bindito.Core;
using HarmonyLib;
using Timberborn.BlockSystem;

namespace TobbyTools.DynamicSpecificationSystem
{
    [HarmonyPatch]
    public class BuildingSpecificationConfiguratorPatch
    {
        static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("BuildingSpecificationConfigurator"), "Configure", new []
            {
                typeof(IContainerDefinition)
            });
        }
        
        static bool Prefix()
        {
            return false;
        }
    }
}