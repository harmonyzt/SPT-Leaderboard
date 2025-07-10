using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPTLeaderboard.Data;

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
}