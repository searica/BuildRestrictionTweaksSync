using BepInEx;
using HarmonyLib;
using BuildRestrictionTweaksSync.Configs;
using BuildRestrictionTweaksSync.Logging;
using System.Reflection;

namespace BuildRestrictionTweaksSync
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, Jotunn.Main.Version)]
    internal class Plugin : BaseUnityPlugin
    {
        internal const string Author = "Searica";
        public const string PluginName = "BuildRestrictionTweaksSync";
        public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "1.0.0";

        private Harmony _harmony;

        public void Awake()
        {
            Log.Init(Logger);

            PluginConfig.Init(Config);
            PluginConfig.SetUpConfig();

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);

            Game.isModded = true;

            PluginConfig.SetupWatcher();
        }

        public void OnDestroy()
        {
            PluginConfig.Save();
            _harmony?.UnpatchSelf();
        }
    }
}