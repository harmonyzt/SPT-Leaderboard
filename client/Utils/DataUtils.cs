using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Bootstrap;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using UnityEngine;
using TraderData = SPTLeaderboard.Data.TraderData;

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

        return GlobalData.BaseSptVersion;
    }
    
    /// <summary>
    /// Get list loaded mods from server for user
    /// </summary>
    /// <returns></returns>
    public static List<string> GetServerMods()
    {
        List<string> listServerMods = new List<string>();

        try
        {
            string json = RequestHandler.GetJson("/launcher/server/serverModsUsedByProfile");
            LeaderboardPlugin.logger.LogWarning($"GetServerMods: {json}");
            if (string.IsNullOrWhiteSpace(json))
                return listServerMods;
            
            List<ModItem> serverMods = Json.Deserialize<List<ModItem>>(json);

            if (serverMods != null)
            {
                var listMods = serverMods.Select(mod => mod.Name).ToList();
                listServerMods.AddRange(listMods);
            }
        }
        catch (Exception ex)
        {
            LeaderboardPlugin.logger.LogWarning($"GetServerMods failed: {ex}");
        }

        return listServerMods;
    }

    
    
    /// <summary>
    /// Get final price from list items
    /// </summary>
    /// <returns></returns>
    public static int GetPriceItems(List<string> listItems)
    {
        var price = 0;

        var data = new ItemsData()
        {
            Items = listItems
        };
        
        try
        {
            var json = RequestHandler.PostJson("/SPTLB/GetItemPrices", JsonConvert.SerializeObject(data));
            
            if (string.IsNullOrWhiteSpace(json))
                return price;

            price = int.Parse(json);
            
            LeaderboardPlugin.logger.LogWarning($"GetPriceItems Response = {price}");
        }
        catch (Exception ex)
        {
            LeaderboardPlugin.logger.LogWarning($"GetPriceItems failed: {ex}");
            return price;
        }

        return price;
    }
    
    /// <summary>
    /// Get price from item
    /// </summary>
    /// <returns></returns>
    public static int GetPriceItem(MongoID item)
    {
        var price = 0;

        var listItems = new List<string>()
        {
            item.ToString()
        };
        
        var data = new ItemsData()
        {
            Items = listItems
        };
        
        try
        {
            var json = RequestHandler.PostJson("/SPTLB/GetItemPrices", JsonConvert.SerializeObject(data));

            if (string.IsNullOrWhiteSpace(json))
                return price;

            price = int.Parse(json);
            
            LeaderboardPlugin.logger.LogWarning($"GetPriceItems Response = {price}");
        }
        catch (Exception ex)
        {
            LeaderboardPlugin.logger.LogWarning($"GetPriceItems failed: {ex}");
            return price;
        }
        
        return price;
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

    public static string GetRaidGameTime()
    {
        try
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                string time = gameWorld.GameDateTime.Calculate().ToString("HH:mm:ss");
                return time;
            }

            return "";
        }
        catch
        {
            return "";
        }
    }
    
    public static string GetRaidPlayerSide()
    {
        try
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                string side = gameWorld.MainPlayer.Side.ToString();
                return side;
            }

            return "";
        }
        catch
        {
            return "";
        }
    }
    
    public static string GetRaidRawMap()
    {
        try
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld != null)
            {
                string rawMap = gameWorld.LocationId;
                return rawMap;
            }

            return "";
        }
        catch
        {
            return "";
        }
    }
    
    public static Dictionary<string, TraderData> GetTraderInfo(Profile pmcData)
    {
        var traderInfoPmc = pmcData.TradersInfo;
        
        Dictionary<string, TraderData> tradersData = new Dictionary<string, TraderData>();
        foreach (var trader in GlobalData.TraderMap)
        {
            if (traderInfoPmc.ContainsKey(trader.Key))
            {
                tradersData[trader.Value] = new TraderData
                {
                    ID = trader.Key,
                    SalesSum = traderInfoPmc[trader.Key].SalesSum,
                    Unlocked = traderInfoPmc[trader.Key].Unlocked,
                    Standing = traderInfoPmc[trader.Key].Standing,
                    LoyaltyLevel = traderInfoPmc[trader.Key].LoyaltyLevel,
                    Disabled = traderInfoPmc[trader.Key].Disabled
                };
            }
            else
            {
                tradersData[trader.Value] = new TraderData
                {
                    ID = trader.Key,
                    SalesSum = 0,
                    Unlocked = false,
                    Standing = 0,
                    LoyaltyLevel = 0,
                    Disabled = true,
                    NotFound = true
                };
            }
        }

        return tradersData;
    }
    
    public static string GetPrettyMapName(string entry)
    {
        return entry switch
        {
            "bigmap" => "Customs",
            "factory4_day" => "Factory",
            "factory4_night" => "Night Factory",
            "interchange" => "Interchange",
            "laboratory" => "Labs",
            "rezervbase" => "Reserve",
            "shoreline" => "Shoreline",
            "woods" => "Woods",
            "lighthouse" => "Lighthouse",
            "tarkovstreets" => "Streets of Tarkov",
            "sandbox" => "Ground Zero - Low",
            "sandbox_high" => "Ground Zero - High",
            _ => "UNKNOWN"
        };
    }

    public static void TryGetTransitionData(RaidEndDescriptorClass resultRaid, Action<string, bool> callback)
    {
        var lastRaidTransitionTo = "None";
        if (resultRaid.result == ExitStatus.Transit
            && TransitControllerAbstractClass.Exist<LocalGameTransitControllerClass>(out var transitController))
        {
            var locationTransit = transitController.alreadyTransits[resultRaid.ProfileId];
            lastRaidTransitionTo = GetPrettyMapName(locationTransit.location.ToLower());
            
            LeaderboardPlugin.logger.LogWarning($"Player transit to map PRETTY {lastRaidTransitionTo}");
            LeaderboardPlugin.logger.LogWarning($"Player transit to map RAW {locationTransit.location}");
            callback.Invoke(lastRaidTransitionTo, true);
            return;
        }
        callback.Invoke(lastRaidTransitionTo, false);
    }
    
    /// <summary>
    /// Checking exists kappa secured container in items 
    /// </summary>
    /// <param name="allItems"></param>
    /// <returns></returns>
    public static bool CheckHasKappa(IEnumerable<Item> allItems)
    {
        foreach (var item in allItems)
        {
            if (item.TemplateId == "676008db84e242067d0dc4c9" || item.TemplateId == "5c093ca986f7740a1867ab12")
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Checking exists dev items in items 
    /// </summary>
    /// <param name="allItems"></param>
    /// <returns></returns>
    public static bool CheckDevItems(IEnumerable<Item> allItems)
    {
        foreach (var item in allItems)
        {
            if (item.TemplateId == "58ac60eb86f77401897560ff" || item.TemplateId == "5c0a5a5986f77476aa30ae64")
            {
                return true;
            }
        }
        return false;
    }
}