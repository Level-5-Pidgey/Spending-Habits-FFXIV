using Core;
using Dalamud.Plugin;

namespace Business
{
    public interface ISpendingHabitsFileManager
    {
        //Properties
        SpendingHabitsConfig Config { get; }
        static DalamudPluginInterface PluginInterface { get; } = null!;

        void SaveConfig();
    }
}