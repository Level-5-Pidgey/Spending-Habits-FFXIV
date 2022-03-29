using System;
using Business;
using Core;
using Dalamud.DrunkenToad;
using Dalamud.Plugin;

namespace Infrastructure
{
    public class SpendingHabitsFileManager : ISpendingHabitsFileManager
    {
        public SpendingHabitsConfig Config { get; private set; } = null!;
        private static DalamudPluginInterface PluginInterface { get; set; } = null!;

        public SpendingHabitsFileManager(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            
            //Attempt to load config into manager
            if (LoadConfig())
            {
                Logger.LogInfo("Successfully loaded plugin config into manager.");
            }
            else
            {
                Logger.LogError("Failed to load config - creating new one instead.");
            }
        }
        
        /// <summary>
        /// Loads the config into memory from the respective .json file.
        /// </summary>
        /// <returns>True if successfully loaded, false if an exception was caught.</returns>
        private bool LoadConfig()
        {
            //Load plugin config
            try
            {
                Config = PluginInterface.GetPluginConfig() as SpendingHabitsConfig ?? new
                    SpendingHabitsConfig();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception occurred trying to load config.");
                Config = new SpendingHabitsConfig();
                SaveConfig();
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Saves plugin configuration to Dalamud plugin directory.
        /// </summary>
        public void SaveConfig()
        {
            PluginInterface.SavePluginConfig(Config);
        }

        /// <summary>
        /// Gets the location of the plugin's config.
        /// </summary>
        /// <returns>The directory of the plugin configuration .json file.</returns>
        public static string GetConfigDirectory()
        {
            return PluginInterface.GetPluginConfigDirectory();
        }

    }
}