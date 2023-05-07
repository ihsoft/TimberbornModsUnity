using System;
using System.Collections.Generic;
using HarmonyLib;
using Timberborn.Beavers;
using Timberborn.BlockSystem;
using Timberborn.PathSystem;
using Timberborn.PrefabOptimization;
using Timberborn.Ruins;
using Timberborn.StatusSystem;
using Timberborn.Yielding;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

namespace TobbyTools.DynamicSpecificationSystem
{
    public static class UnallowedTypes
    {
        public static readonly Type[] Types = {
            // TODO Change fix that this class gets processed
            typeof(List<>).MakeGenericType(AccessTools.TypeByName("ProgressStep")),
            typeof(List<>).MakeGenericType(AccessTools.TypeByName("RecipeModel")),
            AccessTools.TypeByName("BlockObjectNavMeshEdgeSpecification"),
            AccessTools.TypeByName("NeedApplierEffectSpecification"),
            AccessTools.TypeByName("NeedSuspender"),
            AccessTools.TypeByName("FloatLimits"),
            typeof(List<string>),
            //
            
            
            typeof(AutoAtlasSpecification[]),
            typeof(YielderSpecification),
            typeof(RuinModelVariant[]),
            typeof(PathModelVariant),
            typeof(StatusIconCycler),
            typeof(List<Vector2Int>),
            typeof(ParticleSystem),
            typeof(List<Material>),
            typeof(PanelSettings),
            typeof(RectTransform),
            typeof(GameObject[]),
            typeof(BlockObject),        
            typeof(StatusIcon),    
            typeof(List<Mesh>),
            typeof(UIDocument),
            typeof(GameObject),
            typeof(AudioMixer),
            typeof(Texture2D),
            typeof(Transform),
            typeof(Gradient),
            typeof(Material),
            typeof(Texture),
            typeof(Beaver),
            typeof(Sprite),
            typeof(Image),
            typeof(Color),
            typeof(Mesh),
            AccessTools.TypeByName("MechanicalModelVariantSpecification").MakeArrayType(),
            AccessTools.TypeByName("WindRotator").MakeArrayType(),
            AccessTools.TypeByName("DayStageColors"),
            AccessTools.TypeByName("WindRotator"),
        };
    }
}