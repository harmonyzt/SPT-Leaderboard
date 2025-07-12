using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Bootstrap;
using Comfort.Common;
using EFT;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using UnityEngine;

namespace SPTLeaderboard.Utils;

public static class DataUtils
{
    public static long CurrentTimestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    
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
    /// Parsing version SPT from PlayerPrefs
    /// </summary>
    /// <returns></returns>
    public static string GetSptVersion()
    {
        var rawString = PlayerPrefs.GetString("SPT_Version");
        var match = Regex.Match(rawString, @"SPT\s+([0-9\.]+)\s+-");
        if (match.Success)
        {
            string version = match.Groups[1].Value;
            return version;
        }

        return GlobalData.BaseSPTVersion;
    }
    
    /// <summary>
    /// Get list loaded mods from server in user
    /// </summary>
    /// <returns></returns>
    public static List<string> GetServerMods()
    {
        List<string> listServerMods = new List<string>();

        try
        {
            string json = RequestHandler.GetJson("/launcher/profile/info");

            if (string.IsNullOrWhiteSpace(json))
                return listServerMods;

            ServerProfileInfo serverProfileInfo = Json.Deserialize<ServerProfileInfo>(json);

            if (serverProfileInfo?.sptData?.Mods != null)
            {
                foreach (var serverMod in serverProfileInfo.sptData.Mods)
                {
                    if (serverMod?.Name != null)
                        listServerMods.Add(serverMod.Name);
                }
            }
        }
        catch (Exception ex)
        {
            LeaderboardPlugin.logger.LogWarning($"GetServerMods failed: {ex}");
        }

        return listServerMods;
    }

    public static List<string> GetUserMods()
    {
        return GetDirectories(GlobalData.UserModsPath);
    }
    
    public static List<string> GetBepinexMods()
    {
        return GetDirectories(BepInEx.Paths.PluginPath);
    }
    
    public static List<string> GetBepinexDll()
    {
        return GetDllFiles(BepInEx.Paths.PluginPath);
    }
    
    public static List<string> GetDirectories(string dirPath)
    {
        if (!Directory.Exists(dirPath))
            return new List<string>();

        return Directory.GetDirectories(dirPath)
            .Select(path => Path.GetFileName(path))
            .ToList();
    }
    
    public static List<string> GetDllFiles(string dirPath)
    {
        if (!Directory.Exists(dirPath))
            return new List<string>();

        return Directory.GetFiles(dirPath, "*.dll", SearchOption.TopDirectoryOnly)
            .Select(file => Path.GetFileName(file))
            .ToList();
    }
    
    public static bool TryGetPlugin(string pluginGUID, out BaseUnityPlugin plugin)
    {
        PluginInfo pluginInfo;
        bool flag = Chainloader.PluginInfos.TryGetValue(pluginGUID, out pluginInfo);
        plugin = (flag ? pluginInfo.Instance : null);
        return flag;
    }

    public static void Load(Action<bool> callback)
    {
        BaseUnityPlugin FikaCoreBLYAT;
        TryGetPlugin("com.fika.core", out FikaCoreBLYAT);
        FikaCore = FikaCoreBLYAT;
        IsLoaded = true;
        callback.Invoke(FikaCore != null);
    }
    
    public static bool IsFika = FikaCore != null;
    
    public static bool IsLoaded = false;
    
    public static BaseUnityPlugin FikaCore;
    
    public static Type GetPluginType(BaseUnityPlugin plugin, string typePath)
    {
        if (plugin == null)
        {
            throw new ArgumentNullException("plugin");
        }
        return plugin.GetType().Assembly.GetType(typePath, true);
    }
}