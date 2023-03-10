﻿using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Timberborn.CharacterMovementSystem;
using Timberborn.Common;
using UnityEngine;

namespace PathLinkUtilities
{
  [HarmonyPatch]
  public class RoadNavMeshGraphPatch
  {
    private static readonly Dictionary<Transform, PathLinkWaiter> PathLinkAwaiters = new();

    public static IEnumerable<MethodInfo> TargetMethods() => new MethodInfo[1]
    {
      AccessTools.Method(AccessTools.TypeByName("PathFollower"), "MoveAlongPath", new Type[3]
      {
        typeof (float),
        typeof (string),
        typeof (float)
      })
    };

    private static bool Prefix(
      Transform ____transform,
      IReadOnlyList<Vector3> ____pathCorners,
      ref int ____nextCornerIndex,
      MovementAnimator ____movementAnimator)
    {
      return !PathLinkAwaiters.GetOrAdd(____transform, ____transform.GetComponent<PathLinkWaiter>).ShouldWait(____pathCorners, ref ____nextCornerIndex, ____movementAnimator);
    }
  }
}
