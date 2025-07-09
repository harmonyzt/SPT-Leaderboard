using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPTLeaderboard.Data;
using SPTLeaderboard.Utils;
using TraderData = SPTLeaderboard.Data.TraderData;

namespace SPTLeaderboard.Models;

public class ProcessProfileModel
{
    public static ProcessProfileModel Instance { get; private set; }

    public void ProcessAndSendProfile(GClass1959 resultRaid, LocalRaidSettings localRaidSettings)
    {
        if (Singleton<PreloaderUI>.Instantiated)
        {
            var session = DataUtils.GetSession();
            if (session.Profile != null)
            {
                var profileID = session.Profile.Id;

                GClass767 agressorData = session.Profile.EftStats.Aggressor;
                if (agressorData != null && resultRaid.result == ExitStatus.Killed)
                {
                    string nameKiller = string.Empty;
                    if (((GInterface187)agressorData).ProfileId != session.Profile.Id)
                    {
                        if (((GInterface187)agressorData).ProfileId == "66f3fad50ec64d74847d049d")
                        {
                            nameKiller = agressorData.Name.Localized(null);
                        }
                        else
                        {
                            nameKiller = agressorData.GetCorrectedNickname();
                        }
                    }
                    
                    LeaderboardPlugin.logger.LogWarning($"AgressorData.Name {nameKiller}");
                }
                
                var gameVersion = session.Profile.Info.GameVersion;
                var lastRaidLocationRaw = localRaidSettings.location;
                var lastRaidLocation = GetPrettyMapName(lastRaidLocationRaw.ToLower());
                
                var pmcData = session.GetProfileBySide(ESideType.Pmc);
                var scavData = session.GetProfileBySide(ESideType.Savage);
                
                ProfileData profileData = null;
                try
                {
                    profileData = JsonConvert.DeserializeObject<ProfileData>(resultRaid.profile.JObject.ToString());
                }
                catch (Exception e)
                {
                    LeaderboardPlugin.logger.LogError($"Cant parse data profile {e.Message}");
                }
                
                bool isScavRaid = session.Profile.Side == EPlayerSide.Savage;
                if (profileData != null)
                {
                    isScavRaid = profileData.Info.Side == "Savage";
                }
                
                bool discFromRaid = resultRaid.result == ExitStatus.Left;
                
                var isTransition = false;
                var lastRaidTransitionTo = "None";

                if (resultRaid.result == ExitStatus.Transit
                    && TransitControllerAbstractClass.Exist<GClass1676>(out var transitController))
                {
                    if (transitController.localRaidSettings_0.location != "None")
                    {
                        isTransition = true;
                        var locationTransit = transitController.alreadyTransits[resultRaid.ProfileId];
                        lastRaidTransitionTo = GetPrettyMapName(locationTransit.location);
                        
                        LeaderboardPlugin.logger.LogWarning($"Player transit to map PRETTY {GetPrettyMapName(lastRaidTransitionTo)}");
                        LeaderboardPlugin.logger.LogWarning($"Player transit to map RAW {locationTransit.location}");
                    }
                }

                var allAchievementsDict = pmcData.AchievementsData.ToDictionary(
                    pair => pair.Key.ToString(),
                    pair => pair.Value
                );
                
                #region CheckGodBalaclava

                bool godBalaclava = false;
                
                var allItems = pmcData.Inventory.GetPlayerItems();
                foreach (var item in allItems)
                {
                    if (item.TemplateId == "58ac60eb86f77401897560ff")
                    {
                        godBalaclava = true;
                    }
                }
                
                
                if (godBalaclava)
                {
                    LeaderboardPlugin.logger.LogWarning("Player has balaclava of a god, SUKA BLYAT!");
                    godBalaclava = !SettingsModel.Instance.Debug.Value; //TODO: Delete debug. BEFORE PROD
                }
                
                #endregion

                #region Stats
                
                #region PMCStats
                
                var MaxHealth = pmcData.Health.BodyParts.Where(
                    bodyPart => bodyPart.Value?.Health != null).
                    Sum(bodyPart => bodyPart.Value.Health.Maximum);
                                
                var CurrentHealth = pmcData.Health.BodyParts.Where(
                    bodyPart => bodyPart.Value?.Health != null).
                    Sum(bodyPart => bodyPart.Value.Health.Current);
                                
                var Kills = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.Kills);
                var KilledSavage = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledSavage);
                var KilledPmc = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledPmc);
                var KilledBear = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledBear);
                var KilledBoss = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledBoss);
                var HeadShots = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.HeadShots);
                var LongestShot = (int)session.Profile.Stats.Eft.SessionCounters.GetFloat(SessionCounterTypesAbstractClass.LongestShot);
                var LongestKillShot = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.LongestKillShot);
                var LongestKillStreak = session.Profile.Stats.Eft.SessionCounters.GetLong(SessionCounterTypesAbstractClass.LongestKillStreak);
                var ExpLooting = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.ExpLooting);
                var HitCount = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.HitCount);
                var TotalDamage = (int)session.Profile.Stats.Eft.SessionCounters.GetFloat(SessionCounterTypesAbstractClass.CauseBodyDamage);

                if (!isScavRaid)
                {
                    if (SettingsModel.Instance.Debug.Value)
                    {
                        LeaderboardPlugin.logger.LogWarning($"\n");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] Kills {Kills}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledSavage {KilledSavage}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledPmc {KilledPmc}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledBear {KilledBear}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledBoss {KilledBoss}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] HeadShots {HeadShots}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestShot {LongestShot}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestKillShot {LongestKillShot}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestKillStreak {LongestKillStreak}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] CauseBodyDamage {TotalDamage}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] ExpLooting {ExpLooting}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] HitCount {HitCount}");
                    }
                }

                #endregion
                
                #region ScavStats
                
                if (isScavRaid)
                {
                    KilledPmc = scavData.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledPmc);
                    LongestShot = (int)scavData.Stats.Eft.SessionCounters.GetFloat(SessionCounterTypesAbstractClass.LongestShot);
                    HitCount = scavData.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.HitCount);
                    TotalDamage = (int)scavData.Stats.Eft.SessionCounters.GetFloat(SessionCounterTypesAbstractClass.CauseBodyDamage);

                    if (SettingsModel.Instance.Debug.Value)
                    {
                        LeaderboardPlugin.logger.LogWarning($"\n");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledPmc Scav {KilledPmc}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestShot Scav {LongestShot}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] HitCount Scav {HitCount}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] CauseBodyDamage Scav {TotalDamage}");
                    }
                }
                
                #endregion
                
                if (HitCount <= 0) {
                    HitCount = 0;
                }
                
                #endregion
                
                var listModsPlayer = GetServerMods()
                    .Concat(GetDirectories(GlobalData.UserModsPath))
                    .Concat(GetDirectories(BepInEx.Paths.PluginPath))
                    .Concat(GetDirectories(BepInEx.Paths.PluginPath))
                    .ToList();
                
                #region StatTrack

                var statTrackIsUsed = StatTrackInterop.Loaded();
                Dictionary<string, Dictionary<string, WeaponInfo>> processedStatTrackData = new Dictionary<string, Dictionary<string, WeaponInfo>>();
                
                if (!SettingsModel.Instance.EnableModSupport.Value && !statTrackIsUsed)
                {
                    processedStatTrackData = null;
                }
                else
                {
                    LeaderboardPlugin.logger.LogWarning($"Loaded StatTrack plugin {statTrackIsUsed}");
                    
                    var dataStatTrack = StatTrackInterop.LoadFromServer();
                    if (dataStatTrack != null)
                    {
                        if (SettingsModel.Instance.Debug.Value)
                        {
                            LeaderboardPlugin.logger.LogWarning(
                                $"Data raw StatTrack {JsonConvert.SerializeObject(dataStatTrack).ToJson()}");
                        }

                        processedStatTrackData = GetAllValidWeapons(profileID ,dataStatTrack);
                        if (processedStatTrackData != null)
                        {
                            if (SettingsModel.Instance.Debug.Value)
                            {
                                LeaderboardPlugin.logger.LogWarning(JsonConvert.SerializeObject(processedStatTrackData).ToJson());
                            }
                        }
                    }
                }

                #endregion
                
                var baseData = new BaseData {
                    AccountType = gameVersion,
                    Health = CurrentHealth,
                    Id = profileID,
                    IsScav = isScavRaid,
                    LastPlayed = DataUtils.CurrentTimestamp,
                    ModInt = "fb75631b7a153b1b95cdaa7dfdc297b4a7c40f105584561f78e5353e7e925c6f", //TODO: Switch from static hash to dynamic calculation. BEFORE PROD
                    Mods = SettingsModel.Instance.Debug.Value ? ["IhanaMies-LootValueBackend", "SpecialSlots"] : listModsPlayer, //TODO: Delete debug. BEFORE PROD
                    Name = session.Profile.Nickname,
                    PmcHealth = MaxHealth,
                    PmcLevel = pmcData.Info.Level,
                    RaidKills = KilledPmc,
                    RaidResult = resultRaid.result.ToString(),
                    RaidTime = resultRaid.playTime,
                    SptVersion = DataUtils.GetSptVersion(),
                    Token = EncryptionModel.Instance.Token,
                    DBinInv = godBalaclava,
                    IsCasual = SettingsModel.Instance.ModCasualMode.Value
                };

                if (!SettingsModel.Instance.PublicProfile.Value)
                {
                    var privateProfileData = new PrivateProfileData(baseData);

                    if (SettingsModel.Instance.Debug.Value)
                    {
                        LeaderboardPlugin.logger.LogWarning(
                            $"DATA privateProfileData {JsonConvert.SerializeObject(privateProfileData)}");
                    }

                    LeaderboardPlugin.SendProfileData(privateProfileData);
                }
                else if (SettingsModel.Instance.PublicProfile.Value && !isScavRaid)
                {
                    var traderInfoData = GetTraderInfo(pmcData);
                    
                    var pmcProfileData = new AdditiveProfileData(baseData)
                    {
                        DiscFromRaid = discFromRaid,
                        IsTransition = isTransition,
                        IsUsingStattrack = statTrackIsUsed,
                        LastRaidEXP = ExpLooting,
                        LastRaidHits = HitCount,
                        LastRaidMap = lastRaidLocation,
                        LastRaidMapRaw = lastRaidLocationRaw,
                        LastRaidTransitionTo = lastRaidTransitionTo,
                        AllAchievements = allAchievementsDict,
                        LongestShot = LongestShot,
                        ModWeaponStats = processedStatTrackData,
                        PlayedAs = "PMC",
                        PmcSide = pmcData.Side.ToString(),
                        Prestige = pmcData.Info.PrestigeLevel,
                        PublicProfile = true,
                        ScavLevel = scavData.Info.Level, 
                        RaidDamage = TotalDamage,
                        RegistrationDate = session.Profile.Info.RegistrationDate,
                        TraderInfo = traderInfoData
                    };
                    
                    if (SettingsModel.Instance.Debug.Value)
                    {
                        LeaderboardPlugin.logger.LogWarning($"DATA PMC {JsonConvert.SerializeObject(pmcProfileData)}");
                    }

                    LeaderboardPlugin.SendProfileData(pmcProfileData);
                }
                else if (SettingsModel.Instance.PublicProfile.Value && isScavRaid)
                {
                    var traderInfoData = GetTraderInfo(pmcData);
                    
                    var scavProfileData = new AdditiveProfileData(baseData)
                    {
                        DiscFromRaid = discFromRaid,
                        IsTransition = isTransition,
                        IsUsingStattrack = statTrackIsUsed,
                        LastRaidEXP = 0,
                        LastRaidHits = HitCount,
                        LastRaidMap = lastRaidLocation,
                        LastRaidMapRaw = lastRaidLocationRaw,
                        LastRaidTransitionTo = lastRaidTransitionTo,
                        AllAchievements = allAchievementsDict,
                        LongestShot = LongestShot,
                        ModWeaponStats = processedStatTrackData,
                        PlayedAs = "SCAV",
                        PmcSide = pmcData.Side.ToString(),
                        Prestige = pmcData.Info.PrestigeLevel,
                        PublicProfile = true,
                        ScavLevel = scavData.Info.Level, 
                        RaidDamage = TotalDamage,
                        RegistrationDate = session.Profile.Info.RegistrationDate,
                        TraderInfo = traderInfoData
                    };
                    
                    if (SettingsModel.Instance.Debug.Value)
                    {
                        LeaderboardPlugin.logger.LogWarning(
                            $"DATA SCAV {JsonConvert.SerializeObject(scavProfileData)}");
                    }

                    LeaderboardPlugin.SendProfileData(scavProfileData);
                }
            }
        }
    }
    
    public static ProcessProfileModel Create()
    {
        if (Instance != null)
        {
            return Instance;
        }
        return Instance = new ProcessProfileModel();
    }

    private string GetPrettyMapName(string entry)
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

    private Dictionary<string, TraderData> GetTraderInfo(Profile pmcData)
    {
        var traderInfoPmc = pmcData.TradersInfo;
        
        Dictionary<string, TraderData> tradersData = new Dictionary<string, TraderData>();
        foreach (var trader in TraderMap)
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

    private readonly Dictionary<string, string> TraderMap = new() {
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
    
    private List<string> GetServerMods()
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

    private List<string> GetUserMods()
    {
        return GetDirectories(GlobalData.UserModsPath);
    }
    
    private List<string> GetBepinexMods()
    {
        return GetDirectories(BepInEx.Paths.PluginPath);
    }
    
    private List<string> GetBepinexDll()
    {
        return GetDllFiles(BepInEx.Paths.PluginPath);
    }
    
    private List<string> GetDirectories(string dirPath)
    {
        if (!Directory.Exists(dirPath))
            return new List<string>();

        return Directory.GetDirectories(dirPath)
            .Select(path => Path.GetFileName(path))
            .ToList();
    }
    
    private List<string> GetDllFiles(string dirPath)
    {
        if (!Directory.Exists(dirPath))
            return new List<string>();

        return Directory.GetFiles(dirPath, "*.dll", SearchOption.TopDirectoryOnly)
            .Select(file => Path.GetFileName(file))
            .ToList();
    }
    
    public static Dictionary<string, Dictionary<string, WeaponInfo>> GetAllValidWeapons(string sessionId, Dictionary<string, Dictionary<string, CustomizedObject>> info)
    {
        if (!info.ContainsKey(sessionId))
        {
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

            string weaponName = LocalizationModel.Instance.GetLocaleName(weaponId);

            // Skip weapons with unknown names
            if (weaponName == "Unknown")
            {
                if (SettingsModel.Instance.Debug.Value)
                {
                    LeaderboardPlugin.logger.LogWarning($"[StatTrack] Not exists locale {weaponName}");
                }
                continue;
            }

            if (SettingsModel.Instance.Debug.Value)
            {
                LeaderboardPlugin.logger.LogWarning($"[StatTrack] Add {weaponName}");
            }
            result[sessionId][weaponName] = new WeaponInfo
            {
                stats = weaponStats,
                originalId = weaponId
            };
        }

        if (result[sessionId].Count == 0)
        {
            if (SettingsModel.Instance.Debug.Value)
            {
                LeaderboardPlugin.logger.LogWarning($"[StatTrack] list is empty. Return NULL");
            }
            return null;
        }

        return result;
    }
}