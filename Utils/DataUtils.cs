using System;
using System.Text.RegularExpressions;
using Comfort.Common;
using EFT;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;

namespace SPTLeaderboard.Utils;

public static class DataUtils
{
    public static ISession GetSession(bool throwIfNull = false)
    {
        var session = ClientAppUtils.GetClientApp().Session;

        if (throwIfNull && session is null)
        {
            LeaderboardPlugin.logger.LogWarning("Trying to access the Session when it's null");
        }

        return session;
    }
    
    public static bool HasRaidStarted()
    {
        bool? inRaid = Singleton<AbstractGame>.Instance?.InRaid;
        return inRaid.HasValue && inRaid.Value;
    }
    
    public static long CurrentTimestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    
    /// <summary>
    /// Get string from enum PlayerState type
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static string GetPlayerState(PlayerState state)
    {
        return Enum.GetName(typeof(PlayerState), state)?.ToLower();
    }
    

    /// <summary>
    /// Parsing version SPT from string
    /// </summary>
    /// <param name="rawString"></param>
    /// <returns></returns>
    public static string ParseVersion(string rawString)
    {
        var match = Regex.Match(rawString, @"SPT\s+([0-9\.]+)\s+-");
        if (match.Success)
        {
            string version = match.Groups[1].Value;
            return version;
        }

        return GlobalData.BaseSPTVersion;
    }
}