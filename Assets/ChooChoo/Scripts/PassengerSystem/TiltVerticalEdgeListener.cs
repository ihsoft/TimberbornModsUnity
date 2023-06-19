using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TimberApi.DependencyContainerSystem;
using UnityEngine;

namespace ChooChoo
{
    [HarmonyPatch]
    public class TiltVerticalEdgeListener
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("PathReconstructor"), "TiltVerticalEdge", new[]
            {
                typeof(List<Vector3>), 
                typeof(int),
                typeof(int)
            });
        }
        
        public static bool Prefix(ref List<Vector3> pathCorners, int startIndex, int endIndex)
        { 
            return !DependencyContainer.GetInstance<PathCorrector>().ShouldCorrectPath(ref pathCorners, startIndex, endIndex);
        }
    }
}