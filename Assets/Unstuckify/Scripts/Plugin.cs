using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;

namespace PipetteTool
{
    public class Plugin : IModEntrypoint
    {
        public const string PluginGuid = "tobbert.unstuckify";
        
        public static IConsoleWriter Log;
        
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            Log = consoleWriter; 
            
            new Harmony(PluginGuid).PatchAll();
        }
    }
}