using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;

namespace PathLinkUtilities
{
  public class Plugin : IModEntrypoint
  {
    public const string PluginGuid = "tobbert.pathlinkapi";
    public const string PluginName = "PathLinkAPI";
    public const string PluginVersion = "1.0.0";
    public static IConsoleWriter Log;

    public void Entry(IMod mod, IConsoleWriter consoleWriter)
    {
      Log = consoleWriter;
      new Harmony(PluginGuid).PatchAll();
    }
  }
}
