using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.DependencyContainerSystem;
using TimberApi.ModSystem;
using Timberborn.Beavers;
using Timberborn.EntitySystem;
using Timberborn.LifeSystem;

namespace BeaverHats
{
    // [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    // [BepInDependency("com.timberapi.timberapi")]
    // public class Plugin : BaseUnityPlugin
    // {
    //     public const string PluginGuid = "tobbert.beaverhats";
    //     public const string PluginName = "Beaver Hats";
    //     public const string PluginVersion = "1.0.0";
    //     
    //     public static ManualLogSource Log;
    //
    //     void Awake()
    //     {
    //         Log = Logger;
    //         
    //         Log.LogInfo($"Loaded {PluginName} Version: {PluginVersion}!");
    //                     
    //         TimberAPI.AssetRegistry.AddSceneAssets(PluginGuid, SceneEntryPoint.InGame);
    //         TimberAPI.DependencyRegistry.AddConfigurator(new BeaverHatsConfigurator());
    //         new Harmony(PluginGuid).PatchAll();
    //     }
    // }
    
    public class Plugin : IModEntrypoint
    {
        public const string PluginGuid = "tobbert.beaverhats";
        
        public static IConsoleWriter Log;
        
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            Log = consoleWriter;
            
            new Harmony(PluginGuid).PatchAll();
        }
    }
    

    [HarmonyPatch(typeof(BeaverFactory), "InjectDependencies", typeof(EntityService), typeof(LifeService), typeof(BeaverNameService))]
    public class Patch
    {
        public static Beaver AdultBeaver;
        public static Beaver ChildBeaver;
        
        static void Postfix(ref Beaver ____adultPrefab, ref Beaver ____childPrefab)
        {
            AdultBeaver = ____adultPrefab;
            ChildBeaver = ____childPrefab;
            // DependencyContainer.GetInstance<BeaverHatsService>().InitiateClothings(ref ____adultPrefab);
            // DependencyContainer.GetInstance<BeaverHatsService>().InitiateClothings(ref ____childPrefab);
        }
    }
}