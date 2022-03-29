using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using Business;
using Dalamud.Data;
using Dalamud.DrunkenToad;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Infrastructure;
using XivCommon;
using XivCommon.Functions;

namespace SpendingHabitsFFXIV
{
    public class SpendingHabitsPlugin : IDalamudPlugin
    {
        public string Name => "SpendingHabits";

        //Public instanced variables
        public XivCommonBase XivCommon = null!;
        public PluginBackupHandler BackupManager = null!;
        public SpendingHabitsFileManager FileManager { get; set; } = null!;
        public BaseRepository BaseRepository { get; set; } = null!;
        public GoodsOrServiceTracker GoodsOrServiceTracker { get; set; } = null!;
        public PluginCommandHandler PluginCommandHandler { get; set; } = null!;

        //Static Variables
        [PluginService]
        public static DalamudPluginInterface PluginInterface  { get; private set; } = null!;

        [PluginService]
        public static CommandManager CommandManager  { get; private set; } = null!;

        [PluginService]
        public static ChatGui GameChat  { get; private set; } = null!;

        [PluginService]
        public static ClientState ClientState  { get; private set; } = null!;

        [PluginService]
        public static Framework Framework  { get; private set; } = null!;

        [PluginService]
        public static DataManager DataManager  { get; private set; } = null!;
        
        //Private Variables
        private Localization _pluginLocalization = null!; 

        public SpendingHabitsPlugin()
        {
            Task.Run(() =>
            {
                try
                {
                    //Assign basic project variables
                    XivCommon = new XivCommonBase(Hooks.Talk | Hooks.ChatBubbles | Hooks.BattleTalk);
                    _pluginLocalization = new Localization(PluginInterface, CommandManager);
                    FileManager = new SpendingHabitsFileManager(PluginInterface);
                    var bigSheet = DataManager.Excel.GetSheet<Lumina.Excel.GeneratedSheets.GilShopItem>();
                    
                    //Setup relevant plugin services
                    BackupManager = new PluginBackupHandler(GetConfigDirectory(), FileManager);
                    BaseRepository = new BaseRepository(GetConfigDirectory());
                    GoodsOrServiceTracker = new GoodsOrServiceTracker(FileManager, PluginInterface, GameChat);

                    //Send a message when plugin is first loaded.
                    GameChat.PluginPrintNotice("SpendingHabits has successfully loaded!");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Failed to start {Name} plugin.");
                }
            });
        }

        public void Dispose()
        {
            try
            {
                _pluginLocalization.Dispose();
                XivCommon.Dispose();
                GoodsOrServiceTracker.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Failed to stop {Name} plugin.");
            }
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