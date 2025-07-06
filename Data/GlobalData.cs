using SPTLeaderboard.Models;

namespace SPTLeaderboard.Data;

public class GlobalData
{
    public static string Version = "2.6.0";
    public static string BaseSPTVersion = "3.11.3";
    public static string HeartbeatUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}heartbeat/v1.php";
    public static string ProfileUrl = $"https://{SettingsModel.Instance.PhpEndpoint.Value}{SettingsModel.Instance.PhpPath.Value}v1/main.php";
    public static bool IsCasual = false;
}