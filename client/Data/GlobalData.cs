using System.IO;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Data;

public static class GlobalData
{
    public static string Version = "2.6.0";
    public static string BaseSPTVersion = "3.11.3";
    public static string HeartbeatUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}heartbeat/v1.php";
    public static string ProfileUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}v2/v2.php";
    public static string IconUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}client/avatar_processor.php";
    public static string PreRaidUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}client/preraid_processor.php";
    
    public const int HeartbeatCooldownSeconds = 60;
    
    public static string SptRootPath = Path.GetFullPath(Path.Combine(BepInEx.Paths.PluginPath, "..", "..")); 
    public static string UserModsPath = Path.GetFullPath(Path.Combine(SptRootPath, "user", "mods")); 
    public static string LeaderboardIconPath = Path.GetFullPath(Path.Combine(SptRootPath, "BepInEx", "plugins", "SPT-Leaderboard", "SavedIcon.png"));

    public static EquipmentData EquipmentLimits = new()
    {
        TacticalVest = 25,
        Pockets = 6,
        Backpack = 48,
        SecuredContainer = 12
    };
}