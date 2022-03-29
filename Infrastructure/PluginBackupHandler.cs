using System;
using System.Timers;
using Dalamud.DrunkenToad;

namespace Infrastructure
{
    public class PluginBackupHandler
    {
        private readonly Timer _backupTimer;
        private readonly BackupManager _backupManager;
        private readonly SpendingHabitsFileManager _fileManager;

        public PluginBackupHandler(string configDirectory, SpendingHabitsFileManager fileManager)
        {
            _backupManager = new BackupManager(configDirectory);
            _fileManager = fileManager;

            //Initialize routine timer backups per config
            _backupTimer = new Timer
            {
                Interval = _fileManager.Config.BackupFrequency,
                Enabled = false,
            };

            //Set what happens when the backup timer elapses
            _backupTimer.Elapsed += RunIntervalBackup;
        }

        /// <summary>
        /// Event method to run a backup at the given interval set in the user's config.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">Arguments passed into the interval event.</param>
        private void RunIntervalBackup(object sender, ElapsedEventArgs? eventArgs)
        {
            if (DateUtil.CurrentTime() <= _fileManager.Config.LastBackup + _fileManager.Config.BackupFrequency) return;
            
            RunBackup();
        }

        /// <summary>
        /// Runs an actual backup and then trims
        /// </summary>
        public void RunBackup()
        {
            Logger.LogInfo($"Backup started at {DateTime.Now}");
            _fileManager.Config.LastBackup = DateUtil.CurrentTime();
            _fileManager.SaveConfig();
            _backupManager.CreateBackup();
            TrimOldBackups();
        }

        /// <summary>
        /// Trims excess backups above the number set in the user's config.
        /// </summary>
        private void TrimOldBackups()
        {
            _backupManager.DeleteBackups(_fileManager.Config.BackupRetention);
        }
    }
}