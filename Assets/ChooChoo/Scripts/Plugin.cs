using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;
using Timberborn.Characters;
using Timberborn.GameDistricts;
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
                typeof(GameObject), typeof(System.Type)
            });
        }
        
        static bool Prefix(GameObject gameObject, System.Type componentType, ref Component __result)
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
}