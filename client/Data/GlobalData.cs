using System.IO;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Data;

public static class GlobalData
{
    public const string Version = "2.6.0";
    public const string SubVersion = "3";
    public const string BaseSPTVersion = "3.11.3";
    public static string HeartbeatUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}heartbeat/v1.php";
    public static string ProfileUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}v2/v2.php";
    public static string IconUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}client/avatar_processor.php";
    public static string PreRaidUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}client/check_version.php";
    
    public const int HeartbeatCooldownSeconds = 60;
    
    public static string SptRootPath = Path.GetFullPath(Path.Combine(BepInEx.Paths.PluginPath, "..", "..")); 
    public static string PathToken => Path.Combine(BepInEx.Paths.PluginPath, "SPT-Leaderboard", "secret.token");
    public static string PathMigrationToken => Path.Combine(UserModsPath, "SPT-Leaderboard", "src", "secret.token");
    public static string UserModsPath = Path.GetFullPath(Path.Combine(SptRootPath, "user", "mods")); 
    public static string LeaderboardIconPath = Path.GetFullPath(Path.Combine(SptRootPath, "BepInEx", "plugins", "SPT-Leaderboard", "SavedIcon.png"));
    public static string LeaderboardFullImagePath = Path.GetFullPath(Path.Combine(SptRootPath, "BepInEx", "plugins", "SPT-Leaderboard", "SavedFull.png"));

    public static EquipmentData EquipmentLimits = new()
    {
        TacticalVest = 25,
        Pockets = 6,
        Backpack = 48,
        SecuredContainer = 12
    };
}