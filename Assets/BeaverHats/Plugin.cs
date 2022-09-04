using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Timberborn.Beavers;
using Timberborn.EntitySystem;
using Timberborn.LifeSystem;
using TimberbornAPI;
using TimberbornAPI.Common;

namespace BeaverHats
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.timberapi.timberapi")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "tobbert.beaverhats";
        public const string PluginName = "Beaver Hats";
        public const string PluginVersion = "1.0.0";
        
        public static ManualLogSource Log;

        void Awake()
        {
            Log = Logger;
            
            Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
                        
            TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.InGame);
            TimberAPI.DependencyRegistry.AddConfigurator(new BeaverHatsConfigurator());
            new Harmony(PluginGuid).PatchAll();
        }
    }

    [HarmonyPatch(typeof(BeaverFactory), "InjectDependencies", typeof(EntityService), typeof(LifeService), typeof(BeaverNameService))]
    public class Patch
    {
        static void Postfix(ref Beaver ____adultPrefab, ref Beaver ____childPrefab)
        {
            TimberAPI.DependencyContainer.GetInstance<BeaverHatsService>().InitiateClothings(ref ____adultPrefab);
            TimberAPI.DependencyContainer.GetInstance<BeaverHatsService>().InitiateClothings(ref ____childPrefab);
        }
    }
}