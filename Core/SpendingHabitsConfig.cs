using Dalamud.Configuration;

namespace Core
{
    public class SpendingHabitsConfig : IPluginConfiguration
    {
        /// <inheritdoc />
        public int Version { get; set; }

        /// <summary>
        /// Toggle if the plugin is active or not.
        /// </summary>
        public bool Active = true;

        /// <summary>
        /// Backup frequency in minutes.
        /// </summary>
        public long BackupFrequency = (6 * 60 * 60); //Defaults to 6 hours.

        /// <summary>
        /// Number of backups to retain before deleting the oldest one.
        /// </summary>
        public int BackupRetention = (4 * 7); //Defaults to a weeks worth of 6-hourly backups.

        /// <summary>
        /// How many minutes since last backup.
        /// </summary>
        public long LastBackup;
    }
}