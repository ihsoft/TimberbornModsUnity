using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;
using Timberborn.AssetSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.BuildingsBlocking;
using Timberborn.Carrying;
using Timberborn.Characters;
using Timberborn.GameDistricts;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.Navigation;
using Timberborn.PrefabSystem;
using Timberborn.StatusSystem;
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
            if (citizen.GetComponentFast<Prefab>().PrefabName.Contains("Train"))
            {
                return false;
            }
            if (citizen.GetComponentFast<Prefab>().PrefabName.Contains("Wagon"))
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
            "StrandedStatus",
            "ControllableCharacter"
        };

        private static readonly List<string> GameObjectsToCheck = new()
        {
            "Train",
            "Wagon"
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
            return AccessTools.Method(AccessTools.TypeByName("GoodCarrierFragment"), "ShowFragment", new[] {typeof(BaseComponent)});
        }
        
        static bool Prefix(BaseComponent entity)
        {
            return !entity.TryGetComponentFast(out WagonManager _);
        }
    }
    
    [HarmonyPatch]
    public class CarrierInventoryFinderPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(
                AccessTools.TypeByName("CarrierInventoryFinder"), 
                "TryCarryFromAnyInventoryLimited",
                new[]
                {
                    typeof(string),
                    typeof(Inventory),
                    typeof(int)
                });
        }
        
        static bool Prefix(
            CarrierInventoryFinder __instance, 
            string goodId,
            Inventory receivingInventory,
            int maxAmount,
            ref bool __result)
        {
            if (!receivingInventory.TryGetComponentFast(out GoodsStation goodsStation)) 
                return true;
            __result =  (bool)ChooChooCore.InvokePrivateMethod(__instance, "TryCarryFromAnyInventoryInternal", new object[]
            {
                goodId, 
                receivingInventory, 
                (Predicate<Inventory>) (inventory => inventory.GetComponentFast<GoodsStation>() != goodsStation), 
                maxAmount
            });
            
            return false;

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
    public class StatusSpriteLoaderPatch
    {
        public static MethodInfo TargetMethod()
        {
            return AccessTools.Method(AccessTools.TypeByName("StatusSpriteLoader"), "LoadSprite", new[] {typeof(string)});
        }
        
        static bool Prefix(string spriteName, IResourceAssetLoader ____resourceAssetLoader, ref Sprite __result)
        {
            try
            {
                __result = ____resourceAssetLoader.Load<Sprite>(Path.Combine("Sprites/StatusIcons", spriteName));
            }
            catch (Exception)
            {
                __result = ____resourceAssetLoader.Load<Sprite>(spriteName);
            }

            return false;
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
    }
}