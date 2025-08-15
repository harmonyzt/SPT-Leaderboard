using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPTLeaderboard.Data;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Utils;

public static class StatTrackInterop
{
    public static readonly Version RequiredVersion = new Version(1, 2, 2);

    public static Dictionary<string, Dictionary<string, CustomizedObject>> WeaponInfoOutOfRaid { get; set; } = new Dictionary<string, Dictionary<string, CustomizedObject>>();
    
    public static bool? StatTrackLoaded;
    
    public static bool Loaded()
    {
        if (!StatTrackLoaded.HasValue)
        {
            bool present = Chainloader.PluginInfos.TryGetValue("com.acidphantasm.stattrack", out PluginInfo pluginInfo);
            StatTrackLoaded = present && pluginInfo.Metadata.Version >= RequiredVersion;
        }

        return StatTrackLoaded.Value;
    }
    
    public static Dictionary<string, Dictionary<string, CustomizedObject>> LoadFromServer()
    {
        if (Loaded())
        {
            try
            {
                string json = RequestHandler.GetJson("/stattrack/load");
                WeaponInfoOutOfRaid = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, CustomizedObject>>>(json);
                return WeaponInfoOutOfRaid;
            }
            catch (Exception ex)
            {
                LeaderboardPlugin.logger.LogError("[StatTrackInterop] Failed to load: " + ex.ToString());
                return null;
            }
        }

        return null;
    }
    
    public static Dictionary<string, Dictionary<string, WeaponInfo>> GetAllValidWeapons(string sessionId, Dictionary<string, Dictionary<string, CustomizedObject>> info)
    {
        if (!info.ContainsKey(sessionId))
        {
            LeaderboardPlugin.logger.LogWarning($"[StatTrack] Not exists data for current session: {sessionId}");
            return null;
        }

        var result = new Dictionary<string, Dictionary<string, WeaponInfo>>
        {
            [sessionId] = new Dictionary<string, WeaponInfo>()
        };

        foreach (var weaponInfo in info[sessionId])
        {
            string weaponId = weaponInfo.Key;
            CustomizedObject weaponStats = weaponInfo.Value;
            string weaponName = LocalizationModel.GetLocaleName(weaponId + " ShortName");

            // Skip weapons with unknown names
            if (weaponName == "Unknown")
            {
#if DEBUG || BETA
                LeaderboardPlugin.logger.LogWarning($"[StatTrack] Not exists locale {weaponId + " ShortName"}");
#endif
                continue;
            }
#if DEBUG || BETA
            LeaderboardPlugin.logger.LogWarning($"[StatTrack] Add {weaponId + " ShortName"}");
#endif
            result[sessionId][weaponName] = new WeaponInfo
            {
                stats = weaponStats,
                originalId = weaponId
            };
        }

        if (result[sessionId].Count == 0)
        {
#if DEBUG || BETA
            LeaderboardPlugin.logger.LogWarning($"[StatTrack] list is empty. Return NULL");
#endif
            return null;
        }

        return result;
    }
}