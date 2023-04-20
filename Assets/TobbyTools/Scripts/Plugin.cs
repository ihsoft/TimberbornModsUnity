using System;
using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;

namespace TextureReplacementTool
{
    public class Plugin : IModEntrypoint
    {
        public const string PluginGuid = "tobbert.texturereplacementtool";
        
        public static IConsoleWriter Log;
        
        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            Log = consoleWriter;
            
            try
            {
                new Harmony(PluginGuid).PatchAll();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}