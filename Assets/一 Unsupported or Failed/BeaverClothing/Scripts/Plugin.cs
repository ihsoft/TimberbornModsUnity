using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;
using Timberborn.Beavers;
using Timberborn.Bots;
using Timberborn.EntitySystem;
using Timberborn.LifeSystem;
using UnityEngine;

namespace BeaverHats
{
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
    public class BeaverFactoryPatch
    {
        public static Transform AdultBeaver;
        public static Transform ChildBeaver;
        
        static void Postfix(ref Beaver ____adultPrefab, ref Beaver ____childPrefab)
        {
            AdultBeaver = ____adultPrefab.transform;
            ChildBeaver = ____childPrefab.transform;
        }
    }
    
    [HarmonyPatch(typeof(BotFactory), "Load")]
    public class BotFactoryPatch
    {
        public static Transform Bot;
        
        static void Postfix(ref Bot ____botPrefab)
        {
            Bot = ____botPrefab.transform;
        }
    }
}