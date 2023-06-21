﻿using System.Reflection;
using HarmonyLib;
using Timberborn.BlockSystem;
using Timberborn.Coordinates;
using UnityEngine;

namespace ChooChoo
{
    
    [HarmonyPatch]
    public class OnBlockObjectUpdateValues
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("BlockObject"), "UpdateValues", new[]
            {
                typeof(Vector3Int), 
                typeof(Orientation)
            });
        }
        
        public static void Postfix(BlockObject __instance)
        {
            if (__instance.TryGetComponentFast(out TrackPiece trackPiece))
            {
                trackPiece.UpdateValues();
            }
        }
    }
}