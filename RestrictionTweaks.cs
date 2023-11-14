using BepInEx;
using HarmonyLib;
using System.Reflection;
using BepInEx.Logging;
using BuildRestrictionTweaksSync.Configs;
using BepInEx.Configuration;
using UnityEngine;

namespace BuildRestrictionTweaksSync
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, Jotunn.Main.Version)]
    internal class RestrictionTweaks : BaseUnityPlugin
    {
        internal const string Author = "Searica";
        public const string PluginName = "BuildRestrictionTweaksSync";
        public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "1.0.2";

        private const string MainSection = "Global";

        public static ConfigEntry<bool> DisableAllRestrictions { get; private set; }

        public static ConfigEntry<bool> IgnoreInvalid { get; private set; }

        public static ConfigEntry<bool> IgnoreBlockedbyPlayer { get; private set; }

        public static ConfigEntry<bool> IgnoreBuildZone { get; private set; }

        public static ConfigEntry<bool> IgnoreSpaceRestrictions { get; private set; }

        public static ConfigEntry<bool> IgnoreTeleportAreaRestrictions { get; private set; }

        public static ConfigEntry<bool> IgnoreMissingStation { get; private set; }

        public static ConfigEntry<bool> IgnoreMissingStationExtension { get; private set; }

        public static ConfigEntry<bool> IgnoreBiomeRestrictions { get; private set; }

        public static ConfigEntry<bool> IgnoreCultivationRestrictions { get; private set; }

        public static ConfigEntry<bool> IgnoreDirtRestrictions { get; private set; }

        public static ConfigEntry<bool> IgnoreDungeonRestrictions { get; private set; }

        public void Awake()
        {
            Log.Init(Logger);

            ConfigManager.Init(PluginGUID, Config, false);
            Initialize();
            ConfigManager.SaveOnConfigSet(true);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);

            Game.isModded = true;

            ConfigManager.SetupWatcher();
        }

        public void OnDestroy()
        {
            ConfigManager.Save();
        }

        /// <summary>
        ///     Set up configuration entries
        /// </summary>
        internal static void Initialize()
        {
            DisableAllRestrictions = ConfigManager.BindConfig(
                MainSection,
                "​​​​\u200BDisableAllRestrictions",
                false,
                "Remove all build restrictions."
            );
            IgnoreBlockedbyPlayer = ConfigManager.BindConfig(
                MainSection,
                "ignoreBlockedbyPlayer",
                false,
                "Ignore player blocking build."
            );
            IgnoreInvalid = ConfigManager.BindConfig(
                MainSection,
                "IgnoreInvalid",
                false,
                "Prevent misc build restrictions."
            );
            IgnoreBuildZone = ConfigManager.BindConfig(
                MainSection,
                "IgnoreBuildZone",
                false,
                "Ignore zone restrictions."
            );
            IgnoreSpaceRestrictions = ConfigManager.BindConfig(
                MainSection,
                "IgnoreSpaceRestrictions",
                false,
                "Ignore space restrictions."
            );
            IgnoreTeleportAreaRestrictions = ConfigManager.BindConfig(
                MainSection,
                "IgnoreTeleportAreaRestrictions",
                false,
                "Ignore teleport area restrictions."
            );
            IgnoreMissingStationExtension = ConfigManager.BindConfig(
                MainSection,
                "IignoreMissingStationExtension",
                false,
                "Ignore missing station extension."
            );
            IgnoreMissingStation = ConfigManager.BindConfig(
                MainSection,
                "IgnoreMissingStation",
                false,
                "Ignore missing station."
            );
            IgnoreBiomeRestrictions = ConfigManager.BindConfig(
                MainSection,
                "IgnoreBiomeRestrictions",
                false,
                "Ignore biome restrictions."
            );
            IgnoreCultivationRestrictions = ConfigManager.BindConfig(
                MainSection,
                "IgnoreCultivationRestrictions",
                false,
                "Ignore need for cultivated ground."
            );
            IgnoreDirtRestrictions = ConfigManager.BindConfig(
                MainSection,
                "IgnoreDirtRestrictions",
                false,
                "Ignore need for dirt."
            );
            IgnoreDungeonRestrictions = ConfigManager.BindConfig(
                MainSection,
                "IgnoreDungeonRestrictions",
                false,
                "Ignore indoor restrictions."
            );
        }
    }

    /// <summary>
    ///     Log level to control output to BepInEx log
    /// </summary>
    internal enum LogLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    /// <summary>
    ///     Helper class for properly logging from static contexts.
    /// </summary>
    internal static class Log
    {
        #region Verbosity

        internal static ConfigEntry<LogLevel> Verbosity { get; set; }
        internal static LogLevel VerbosityLevel => Verbosity.Value;

        #endregion Verbosity

        private static ManualLogSource _logSource;

        private const BindingFlags AllBindings =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        internal static void LogDebug(object data) => _logSource.LogDebug(data);

        internal static void LogError(object data) => _logSource.LogError(data);

        internal static void LogFatal(object data) => _logSource.LogFatal(data);

        internal static void LogInfo(object data, LogLevel level = LogLevel.Low)
        {
            if (Verbosity is null || VerbosityLevel >= level)
            {
                _logSource.LogInfo(data);
            }
        }

        internal static void LogMessage(object data) => _logSource.LogMessage(data);

        internal static void LogWarning(object data) => _logSource.LogWarning(data);

        #region Logging Unity Objects

        internal static void LogGameObject(GameObject prefab, bool includeChildren = false)
        {
            LogInfo("***** " + prefab.name + " *****");
            foreach (Component compo in prefab.GetComponents<Component>())
            {
                LogComponent(compo);
            }

            if (!includeChildren) { return; }

            LogInfo("***** " + prefab.name + " (children) *****");
            foreach (Transform child in prefab.transform)
            {
                LogInfo($" - {child.gameObject.name}");
                foreach (Component compo in child.gameObject.GetComponents<Component>())
                {
                    LogComponent(compo);
                }
            }
        }

        internal static void LogComponent(Component compo)
        {
            LogInfo($"--- {compo.GetType().Name}: {compo.name} ---");

            PropertyInfo[] properties = compo.GetType().GetProperties(AllBindings);
            foreach (var property in properties)
            {
                LogInfo($" - {property.Name} = {property.GetValue(compo)}");
            }

            FieldInfo[] fields = compo.GetType().GetFields(AllBindings);
            foreach (var field in fields)
            {
                LogInfo($" - {field.Name} = {field.GetValue(compo)}");
            }
        }

        #endregion Logging Unity Objects
    }
}