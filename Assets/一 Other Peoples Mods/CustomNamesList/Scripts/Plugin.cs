using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;

namespace CustomNameList
{
    public class Plugin : IModEntrypoint
    {
        private const string PluginGuid = "tobbert.customnameslist";
        
        public static IConsoleWriter Log;

        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            Log = consoleWriter;

            new Harmony(PluginGuid).PatchAll();
        }
    }
}
