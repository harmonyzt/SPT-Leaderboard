using System.Collections.Generic;
using System.IO;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Data;

public static class GlobalData
{
    public const string Version = "4.0.0";
    
#if DEBUG || BETA
    public const string SubVersion = "4";
#endif
    
    public const string BaseSptVersion = "3.11.3";
    
    public const int HeartbeatCooldownSeconds = 60;
    
    // URLs
    public static string HeartbeatUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}heartbeat/v2.php";
    public static string ProfileUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}v2/v2.php";
    public static string IconUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}client/avatar_processor.php";
    public static string PreRaidUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}client/check_version.php";
    public static string ConfigUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}client/get_config.php";
    
    // Paths
    public static string SptRootPath = Path.GetFullPath(Path.Combine(BepInEx.Paths.PluginPath, "..", "..")); 
    public static string PathToken => Path.Combine(BepInEx.Paths.PluginPath, "SPT-Leaderboard", "secret.token");
    public static string PathMigrationToken => Path.Combine(UserModsPath, "SPT-Leaderboard", "src", "secret.token");
    public static string UserModsPath = Path.GetFullPath(Path.Combine(SptRootPath, "user", "mods")); 
    public static string LeaderboardIconPath = Path.GetFullPath(Path.Combine(SptRootPath, "BepInEx", "plugins", "SPT-Leaderboard", "SavedIcon.png"));
    public static string LeaderboardFullImagePath = Path.GetFullPath(Path.Combine(SptRootPath, "BepInEx", "plugins", "SPT-Leaderboard", "SavedFull.png"));

    // Limits equipment capacity
    public static EquipmentData EquipmentLimits = new()
    {
        TacticalVest = 25,
        Pockets = 6,
        Backpack = 48,
        SecuredContainer = 12,
        Stash = 850
    };
    
    public static readonly Dictionary<string, string> TraderMap = new() {
        { "6617beeaa9cfa777ca915b7c", "REF" },
        { "54cb50c76803fa8b248b4571", "PRAPOR" },
        { "54cb57776803fa99248b456e", "THERAPIST" },
        { "579dc571d53a0658a154fbec", "FENCE" },
        { "58330581ace78e27b8b10cee", "SKIER" },
        { "5935c25fb3acc3127c3d8cd9", "PEACEKEEPER" },
        { "5a7c2eca46aef81a7ca2145d", "MECHANIC" },
        { "5ac3b934156ae10c4430e83c", "RAGMAN" },
        { "638f541a29ffd1183d187f57", "LIGHTKEEPER" },
        { "656f0f98d80a697f855d34b1", "BTR_DRIVER" },
        { "5c0647fdd443bc2504c2d371", "JAEGER" }
    };
}
