using System.IO;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Data;

public class GlobalData
{
    public static string Version = "3.6.0";
    public static string BaseSPTVersion = "3.11.3";
    public static string HeartbeatUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}heartbeat/v1.php";
    public static string ProfileUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}v1/main.php";
    
    public const int HeartbeatCooldownSeconds = 10;
    
    public static string SptRootPath = Path.GetFullPath(Path.Combine(BepInEx.Paths.PluginPath, "..", "..")); 
    public static string UserModsPath = Path.GetFullPath(Path.Combine(SptRootPath, "user", "mods")); 
    public static string LocalePath = Path.GetFullPath(Path.Combine(BepInEx.Paths.PluginPath, "SPT-Leaderboard", "data", "StatTrackLocale.json")); 
}