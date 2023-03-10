using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;
using Timberborn.Buildings;
using Timberborn.Characters;
using Timberborn.Common;
using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Navigation;
using Timberborn.PrefabSystem;
using UnityEngine;

namespace ChooChoo
{
    public class Plugin : IModEntrypoint
    {
        private const string PluginGuid = "tobbert.choochoo";
        
        public static IConsoleWriter Log;
        
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            Log = consoleWriter; 
            
            new Harmony(PluginGuid).PatchAll();
        }
    }
    
    [HarmonyPatch]
    public class MigrationTriggerPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("MigrationTrigger"), "RegisterDistributorToCheck", new []
            {
                typeof(Citizen)
            });
        }
        
        static bool Prefix(Citizen citizen)
        {
            if (citizen.GetComponent<Prefab>().PrefabName.Contains("Train"))
            {
                return false;
            }
            if (citizen.GetComponent<Prefab>().PrefabName.Contains("Cart"))
            {
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch]
    public class InstantiatorPatch
    {
        private static readonly List<string> PreventDecorators = new()
        {
            nameof(Citizen),
            nameof(CharacterTint),
            "StrandedStatus"
        };

        private static readonly List<string> GameObjectsToCheck = new()
        {
            "Train",
            "Cart"
        };

        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("Instantiator"), "AddComponent", new []
            {
                typeof(GameObject), typeof(Type)
            });
        }
        
        static bool Prefix(GameObject gameObject, MemberInfo componentType, ref Component __result)
        {
            if (gameObject == null)
                return true;
            var prefab = gameObject.GetComponent<Prefab>();
            if (prefab == null)
                return true;

            var prefabName = gameObject.GetComponent<Prefab>().PrefabName;
            foreach (var nameToCheck in GameObjectsToCheck)
            {
                if (prefabName.Contains(nameToCheck))
                {
                    // Plugin.Log.LogWarning(gameObject + "      " + componentType.Name);
                    if (PreventDecorators.Contains(componentType.Name))
                    {
                        // Plugin.Log.LogError("Preventing");
                        __result = new Component();
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
    
    [HarmonyPatch]
    public class GoodCarrierFragmentPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("GoodCarrierFragment"), "ShowFragment", new[] {typeof(GameObject)});
        }
        
        static bool Prefix(GameObject entity)
        {
            return !entity.TryGetComponent(out TrainWagonManager _);
        }
    }
    
    [HarmonyPatch]
    public class CharacterBatchControlTabPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("CharacterBatchControlTab"), "GetSortingKey", new[] {typeof(string)});
        }
        
        static bool Prefix(string locKey, ref string __result)
        {
            if (locKey == "Tobbert.Train.PrefabName" || locKey == "Tobbert.Wagon.PrefabName")
            {
                __result = "4";
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch]
    public class ObtainGoodWorkplaceBehaviorPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("DistrictInventoryPicker"), "ClosestInventoryWithStock",
                new[]
                {
                    typeof(Accessible),
                    typeof(string),
                    typeof(Predicate<Inventory>),
                });
        }

        static bool Prefix(
            Accessible start,
            string goodId,
            Predicate<Inventory> inventoryFilter,
            DistrictInventoryRegistry ____districtInventoryRegistry,
            ref Inventory __result)
        {
            List<Inventory> inventoryList = ____districtInventoryRegistry.ActiveInventoriesWithStock(goodId).ToList();
            
            // Added a check so it doesn't deliver to itself
            var goodsStation = start.GetComponent<GoodsStation>();
            var originalInventory = goodsStation == null ? null : goodsStation.Inventory;
            if (inventoryList.Contains(originalInventory))
                inventoryList.Remove(originalInventory);
            
            Inventory inventory = null;
            float num = float.MaxValue;
            foreach (var component in inventoryList)
            {
                Accessible enabledComponent = component.GetEnabledComponent<Accessible>();
                float distance;
                if (inventoryFilter(component) && enabledComponent.FindRoadPath(start, out distance) && (double)distance < (double)num)
                {
                    inventory = component;
                    num = distance;
                }
            }

            __result = inventory;
            return false;
        }

        [HarmonyPatch]
        public class ClosestInventoryWithCapacityBehaviorPatch
        {
            public static MethodInfo TargetMethod()
            {
                return AccessTools.Method(AccessTools.TypeByName("DistrictInventoryPicker"),
                    "ClosestInventoryWithCapacity",
                    new[]
                    {
                        typeof(Accessible),
                        typeof(GoodAmount),
                    });
            }

            static bool Prefix(
                Accessible start,
                GoodAmount goodAmount,
                DistrictInventoryRegistry ____districtInventoryRegistry,
                ref Inventory __result)
            {
                List<Inventory> inventoryList = ____districtInventoryRegistry.ActiveInventoriesWithCapacity(goodAmount.GoodId).ToList();

                var originalIsGoodsStation = start.TryGetComponent(out GoodsStation _);

                Inventory inventory1 = null;
                float num = float.MaxValue;

                foreach (var inventory2 in inventoryList)
                {
                    // Prevent goods stations from delivering to other goods stations inside same district
                    if (originalIsGoodsStation && inventory2.TryGetComponent(out GoodsStation _)) 
                        continue;

                    float distance;
                    if (inventory2.GetEnabledComponent<Accessible>().FindRoadPath(start, out distance) &&
                        (double)distance < (double)num 
                        && inventory2.HasUnreservedCapacity(goodAmount) 
                        && inventory2.GetComponent<IInventoryValidator>().ValidInventory 
                        && inventory2.GetComponent<BlockableBuilding>().IsUnblocked)
                    {
                        inventory1 = inventory2;
                        num = distance;
                    }
                }

                __result = inventory1;
                return false;
            }
        }
        
        // [HarmonyPatch]
        // public class RoadNavMeshGraphPatch
        // {
        //     private static PathLinkPointService _pathLinkPointService;
        //     
        //     private static PathLinkPointService PathLinkPointService
        //     {
        //         get
        //         {
        //             return _pathLinkPointService ??= TimberApi.DependencyContainerSystem.DependencyContainer.GetInstance<PathLinkPointService>();
        //         }
        //     }
        //     
        //     private static ChooChooCore _chooChooCore;
        //     
        //     public static ChooChooCore ChooChooCore
        //     {
        //         get
        //         {
        //             return _chooChooCore ??= TimberApi.DependencyContainerSystem.DependencyContainer.GetInstance<ChooChooCore>();
        //         }
        //     }
        //     
        //     public static MethodInfo TargetMethod()
        //     {
        //         return AccessTools.Method(AccessTools.TypeByName("RoadNavMeshGraph"), "VerifyAfterChange", new[] {typeof(int), typeof(int)});
        //     }
        //
        //     static void Prefix(int aNodeId, int bNodeId, RoadNavMeshGraph __instance)
        //     {
        //         // Plugin.Log.LogWarning(aNodeId + "");
        //     
        //         var teleporterLink = PathLinkPointService.GetTeleporterLink(aNodeId);
        //         
        //         if (teleporterLink != null)
        //         {
        //             ChooChooCore.InvokePrivateMethod(__instance, "AddOneWayConnection", new object[] { teleporterLink.StartNodeId, teleporterLink.GoToNodeId, true});
        //             ChooChooCore.InvokePrivateMethod(__instance, "AddOneWayConnection", new object[] { teleporterLink.GoToNodeId, teleporterLink.StartNodeId, true});
        //         }
        //     }
        // }
        
        //
        // [HarmonyPatch]
        // public class RoadNavMeshGraphPatch
        // {
        //     public static IEnumerable<MethodInfo> TargetMethods()
        //     {
        //         return new[]
        //         {
        //             // AccessTools.Method(AccessTools.TypeByName("RoadAStarPathfinder"), "FillFlowFieldWithPath",
        //             //     new[]
        //             //     {
        //             //         typeof(RoadNavMeshGraph), typeof(PathFlowField), typeof(float), typeof(int), typeof(int)
        //             //     }),
        //             AccessTools.Method(AccessTools.TypeByName("RoadAStarPathfinder"), "FillFlowFieldWithPath",
        //                 new[]
        //                 {
        //                     typeof(RoadNavMeshGraph), typeof(PathFlowField), typeof(float), typeof(int), typeof(IReadOnlyList<int>), typeof(int).MakeByRefType()
        //                 })
        //         };
        //     }
        //
        //     static void Postfix(PathFlowField flowField, bool __result)
        //     {
        //         // Plugin.Log.LogError((bool)RoadAStarPathfinderPatch.ChooChooCore.GetInaccessibleField(flowField, "_fullyFilled") + "");
        //         Plugin.Log.LogError(__result + "");
        //     }
        // }
        //
        // [HarmonyPatch]
        // public class CharacterBatchControlTabPatch
        // {
        //     public static IEnumerable<MethodInfo> TargetMethods()
        //     {
        //         return new[]
        //         {
        //             // AccessTools.Method(AccessTools.TypeByName("RoadAStarPathfinder"), "FillFlowFieldWithPath",
        //             //     new[]
        //             //     {
        //             //         typeof(RoadNavMeshGraph), typeof(PathFlowField), typeof(float), typeof(int), typeof(int)
        //             //     }),
        //             AccessTools.Method(AccessTools.TypeByName("RoadAStarPathfinder"), "FillFlowFieldWithPath",
        //             new[]
        //             {
        //                 typeof(RoadNavMeshGraph), typeof(PathFlowField), typeof(float), typeof(int), typeof(IReadOnlyList<int>), typeof(int).MakeByRefType()
        //             })
        //         };
        //     }
        //
        //     static void Postfix(PathFlowField flowField, bool __result)
        //     {
        //         // Plugin.Log.LogError((bool)RoadAStarPathfinderPatch.ChooChooCore.GetInaccessibleField(flowField, "_fullyFilled") + "");
        //         Plugin.Log.LogError(__result + "");
        //     }
        // }
        //
        // [HarmonyPatch]
        // public class RoadAStarPathfinderPatch
        // {
        //     private static readonly Type Type = AccessTools.TypeByName("RoadNavMeshNode");
        //
        //     private static PathLinkPointService _teleporterService;
        //
        //     private static PathLinkPointService PathLinkPointService
        //     {
        //         get
        //         {
        //             return _teleporterService ??= TimberApi.DependencyContainerSystem.DependencyContainer.GetInstance<PathLinkPointService>();
        //         }
        //     }
        //     
        //     private static ChooChooCore _chooChooCore;
        //
        //     public static ChooChooCore ChooChooCore
        //     {
        //         get
        //         {
        //             return _chooChooCore ??= TimberApi.DependencyContainerSystem.DependencyContainer.GetInstance<ChooChooCore>();
        //         }
        //     }
        //     
        //     private static RoadAStarPathfinder _roadAStarPathfinder;
        //
        //     private static RoadAStarPathfinder RoadAStarPathfinder
        //     {
        //         get
        //         {
        //             return _roadAStarPathfinder ??= TimberApi.DependencyContainerSystem.DependencyContainer.GetInstance<RoadAStarPathfinder>();
        //         }
        //     }
        //
        //     public static MethodInfo TargetMethod()
        //     {
        //         return AccessTools.Method(AccessTools.TypeByName("RoadAStarPathfinder"), "VisitNode", new[]
        //         {
        //             AccessTools.TypeByName("AStarNode"),
        //             AccessTools.TypeByName("RoadNavMeshNode"),
        //         });
        //     }
        //
        //     static void Postfix(AStarNode parentNode, RoadNavMeshNode node, PathFlowField ____flowField, float ____maxDistance)
        //     {
        //         int id = (int)ChooChooCore.GetPublicProperty(node, "Id");
        //         
        //         Plugin.Log.LogWarning(id + "");
        //
        //         var teleporterLink = PathLinkPointService.GetTeleporterLink(id);
        //         
        //         if (teleporterLink != null)
        //         {
        //             Plugin.Log.LogInfo("PathLinkPoint Node");
        //             
        //             // if (____flowField.HasNode(id))
        //             //     return;
        //             int num = (bool)ChooChooCore.GetPublicProperty(node, "IsFree") ? 0 : 1;
        //             float gScore = (float)ChooChooCore.GetPublicProperty(parentNode, "GScore") + num;
        //             if ((double) gScore > (double) ____maxDistance)
        //                 return;
        //             // PushNode(id, parentNode.NodeId, gScore);
        //             var parentNodeID = (int)ChooChooCore.GetPublicProperty(parentNode, "NodeId");
        //             _chooChooCore.InvokePrivateMethod(RoadAStarPathfinder, "PushNode", new object[] { teleporterLink.GoToNodeId, parentNodeID, gScore});
        //         }
        //     }
        // }
    }
}