using System;
using HarmonyLib;
using Timberborn.AssetSystem;
using Timberborn.Cutting;
using Timberborn.MeshyEditorTools;
using Timberborn.Yielding;
using UnityEngine;
using UnityEngine.UIElements;

namespace TobbyTools.DynamicSpecificationSystem
{
    public static class SkippableTypes
    {
        public static readonly Type[] Types = {
            typeof(ModelMetadata),
            typeof(MeshRenderer),
            typeof(UIDocument),
            typeof(MeshFilter),
            typeof(BinaryData),
            typeof(Transform),
            AccessTools.TypeByName("UniversalAdditionalCameraData"),
            AccessTools.TypeByName("UniversalAdditionalLightData"),
            AccessTools.TypeByName("CanvasScaler"),
            
            // TODO Change fix that this class gets processed
            typeof(YielderSpecification),
            typeof(Cuttable),
        };
    }
}