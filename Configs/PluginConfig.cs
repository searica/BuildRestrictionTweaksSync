using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BuildRestrictionTweaksSync.Logging;
using Jotunn.Managers;
using System;
using System.IO;
using System.Reflection;

namespace BuildRestrictionTweaksSync.Configs
{
    internal class PluginConfig
    {
        private static BaseUnityPlugin configurationManager;

        private static readonly string ConfigFileName = Plugin.PluginGUID + ".cfg";

        private static readonly string ConfigFileFullPath = string.Concat(
            Paths.ConfigPath,
            Path.DirectorySeparatorChar,
            ConfigFileName
        );

        private static ConfigFile configFile;

        private static readonly ConfigurationManagerAttributes AdminConfig = new() { IsAdminOnly = true };
        private static readonly ConfigurationManagerAttributes ClientConfig = new() { IsAdminOnly = false };

        internal enum LoggerLevel
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }

        private const string MainSectionName = "\u200BGlobal";

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

        internal static ConfigEntry<LoggerLevel> Verbosity { get; private set; }
        internal static LoggerLevel VerbosityLevel => Verbosity.Value;

        internal static bool IsVerbosityLow => Verbosity.Value >= LoggerLevel.Low;
        internal static bool IsVerbosityMedium => Verbosity.Value >= LoggerLevel.Medium;
        internal static bool IsVerbosityHigh => Verbosity.Value >= LoggerLevel.High;

        private static readonly AcceptableValueList<bool> AcceptableBoolValuesList = new(new bool[] { false, true });

        internal static ConfigEntry<T> BindConfig<T>(
            string section,
            string name,
            T value,
            string description,
            AcceptableValueBase acceptVals = null,
            bool synced = true
        )
        {
            string extendedDescription = GetExtendedDescription(description, synced);
            ConfigEntry<T> configEntry = configFile.Bind(
                section,
                name,
                value,
                new ConfigDescription(
                    extendedDescription,
                    acceptVals,
                    synced ? AdminConfig : ClientConfig
                )
            );
            return configEntry;
        }

        internal static string GetExtendedDescription(
            string description,
            bool synchronizedSetting
        )
        {
            return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
        }

        internal static void Init(ConfigFile config)
        {
            configFile = config;
            configFile.SaveOnConfigSet = false;
        }

        internal static void Save()
        {
            configFile.Save();
        }

        internal static void SaveOnConfigSet(bool value)
        {
            configFile.SaveOnConfigSet = value;
        }

        internal static void SetUpConfig()
        {
            DisableAllRestrictions = BindConfig<bool>(
                MainSectionName,
                "​​​​\u200BDisableAllRestrictions",
                false,
                "Remove all build restrictions.",
                AcceptableBoolValuesList
            );
            IgnoreBlockedbyPlayer = BindConfig<bool>(
                MainSectionName,
                "ignoreBlockedbyPlayer",
                false,
                "Ignore player blocking build.",
                AcceptableBoolValuesList
            );
            IgnoreInvalid = BindConfig<bool>(
                MainSectionName,
                "IgnoreInvalid",
                false,
                "Prevent misc build restrictions.",
                AcceptableBoolValuesList
            );
            IgnoreBuildZone = BindConfig<bool>(
                MainSectionName,
                "IgnoreBuildZone",
                false,
                "Ignore zone restrictions.",
                AcceptableBoolValuesList
            );
            IgnoreSpaceRestrictions = BindConfig<bool>(
                MainSectionName,
                "IgnoreSpaceRestrictions",
                false,
                "Ignore space restrictions.",
                AcceptableBoolValuesList
            );
            IgnoreTeleportAreaRestrictions = BindConfig<bool>(
                MainSectionName,
                "IgnoreTeleportAreaRestrictions",
                false,
                "Ignore teleport area restrictions.",
                AcceptableBoolValuesList
            );
            IgnoreMissingStationExtension = BindConfig<bool>(
                MainSectionName,
                "IignoreMissingStationExtension",
                false,
                "Ignore missing station extension.",
                AcceptableBoolValuesList
            );
            IgnoreMissingStation = BindConfig<bool>(
                MainSectionName,
                "IgnoreMissingStation",
                false,
                "Ignore missing station.",
                AcceptableBoolValuesList
            );
            IgnoreBiomeRestrictions = BindConfig<bool>(
                MainSectionName,
                "IgnoreBiomeRestrictions",
                false,
                "Ignore biome restrictions.",
                AcceptableBoolValuesList
            );
            IgnoreCultivationRestrictions = BindConfig<bool>(
                MainSectionName,
                "IgnoreCultivationRestrictions",
                false,
                "Ignore need for cultivated ground.",
                AcceptableBoolValuesList
            );
            IgnoreDirtRestrictions = BindConfig<bool>(
                MainSectionName,
                "IgnoreDirtRestrictions",
                false,
                "Ignore need for dirt.",
                AcceptableBoolValuesList
            );
            IgnoreDungeonRestrictions = BindConfig<bool>(
                MainSectionName,
                "IgnoreDungeonRestrictions",
                false,
                "Ignore indoor restrictions.",
                AcceptableBoolValuesList
            );
            Save();
        }

        internal static void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReloadConfigFile;
            watcher.Created += ReloadConfigFile;
            watcher.Renamed += ReloadConfigFile;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private static void ReloadConfigFile(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Log.LogInfo("Reloading config file");

                // turn off saving on config entry set
                var saveOnConfigSet = configFile.SaveOnConfigSet;
                configFile.SaveOnConfigSet = false;

                configFile.Reload();

                // reset config saving state
                configFile.SaveOnConfigSet = saveOnConfigSet;
            }
            catch
            {
                Log.LogError($"There was an issue loading your {ConfigFileName}");
                Log.LogError("Please check your config entries for spelling and format!");
            }
            // Do whatever else I might need to
        }

        /// <summary>
        ///
        /// </summary>
        internal static void CheckForConfigManager()
        {
            if (GUIManager.IsHeadless())
            {
                return;
            }

            if (
                Chainloader.PluginInfos.TryGetValue(
                    "com.bepis.bepinex.configurationmanager",
                    out PluginInfo configManagerInfo
                )
                && configManagerInfo.Instance
            )
            {
                configurationManager = configManagerInfo.Instance;
                Log.LogDebug("Configuration manager found, hooking DisplayingWindowChanged");

                EventInfo eventinfo = configurationManager.GetType()
                    .GetEvent("DisplayingWindowChanged");

                if (eventinfo != null)
                {
                    Action<object, object> local = new(OnConfigManagerDisplayingWindowChanged);
                    Delegate converted = Delegate.CreateDelegate(
                        eventinfo.EventHandlerType,
                        local.Target,
                        local.Method
                    );
                    eventinfo.AddEventHandler(configurationManager, converted);
                }
            }
        }

        private static void OnConfigManagerDisplayingWindowChanged(object sender, object e)
        {
            //Jotunn.Logger.LogDebug("OnConfigManagerDisplayingWindowChanged recieved.");
            PropertyInfo pi = configurationManager.GetType().GetProperty("DisplayingWindow");
            bool cmActive = (bool)pi.GetValue(configurationManager, null);

            if (!cmActive)
            {
                // do whatever I need to do when the cfg file reloads
            }
        }
    }
}