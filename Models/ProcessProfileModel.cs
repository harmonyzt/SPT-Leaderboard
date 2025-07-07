using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Newtonsoft.Json;
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
                else
                {
                    LeaderboardPlugin.logger.LogWarning($"AgressorData.Name Null:c ");
                }
                
                
                
                var gameVersion = session.Profile.Info.GameVersion;
                var lastRaidLocationRaw = localRaidSettings.location;
                var lastRaidLocation = GetPrettyMapName(lastRaidLocationRaw);
                
                var PmcData = session.GetProfileBySide(ESideType.Pmc);
                
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

                GClass1676 transitController;
                if (resultRaid.result == ExitStatus.Transit &&
                    TransitControllerAbstractClass.Exist<GClass1676>(out transitController))
                {
                    if (transitController.localRaidSettings_0.location != "None")
                    {
                        isTransition = true;
                        var locationTransit = transitController.alreadyTransits[resultRaid.ProfileId];
                        lastRaidTransitionTo = GetPrettyMapName(locationTransit.location);
                        LeaderboardPlugin.logger.LogWarning($"Player transit to map {GetPrettyMapName(lastRaidTransitionTo)}");
                    }
                }

                var allAchievementsDict = PmcData.AchievementsData.ToDictionary(
                    pair => pair.Key.ToString(),
                    pair => pair.Value
                );
                
                #region CheckGodBalaclava

                bool godBalaclava = false;
                
                var allItems = PmcData.Inventory.GetPlayerItems();
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
                    godBalaclava = false; //Debug line
                }
                
                #endregion

                #region Stats
                
                var MaxHealth = PmcData.Health.BodyParts.Where(
                    bodyPart => bodyPart.Value?.Health != null
                ).Sum(
                    bodyPart => bodyPart.Value.Health.Maximum);
                
                var CurrentHealth = PmcData.Health.BodyParts.Where(
                    bodyPart => bodyPart.Value?.Health != null
                ).Sum(
                    bodyPart => bodyPart.Value.Health.Current);                
                
                var Kills = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.Kills);
                var KilledSavage = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledSavage);
                var KilledPmc = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledPmc);
                var KilledBear = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledBear);
                var KilledBoss = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledBoss);
                var HeadShots = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.HeadShots);
                var LongestShot = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.LongestShot);
                var LongestKillShot = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.LongestKillShot);
                var LongestKillStreak = session.Profile.Stats.Eft.SessionCounters.GetLong(SessionCounterTypesAbstractClass.LongestKillStreak);
                var TotalDamage = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.CauseBodyDamage);
                var ExpLooting = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.ExpLooting);
                var HitCount = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.HitCount);
                
                string TotalDamageString = TotalDamage.ToString();
                string trimmedDamage = TotalDamageString.Substring(0, TotalDamageString.Length - 2);
                int NewDamageBody = int.Parse(trimmedDamage);
                
                if (HitCount <= 0) {
                    HitCount = 0;
                }
                
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
                
                #endregion

                var baseData = new BaseData
                {
                    AccountType = gameVersion,
                    Health = CurrentHealth,
                    Id = profileID,
                    IsScav = isScavRaid,
                    LastPlayed = DataUtils.CurrentTimestamp,
                    ModInt = EncryptionModel.Instance.GetHashMod(),
                    Mods = ["IhanaMies-LootValueBackend", "SpecialSlots"],//TODO
                    Name = session.Profile.Nickname,
                    PmcHealth = MaxHealth,
                    PmcLevel = PmcData.Info.Level,
                    RaidKills = Kills,
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
                    
                    LeaderboardPlugin.logger.LogWarning($"DATA privateProfileData {JsonConvert.SerializeObject(privateProfileData)}");
                    
                    LeaderboardPlugin.SendProfileData(privateProfileData);
                }
                else if (SettingsModel.Instance.PublicProfile.Value && !isScavRaid)
                {
                    var traderInfoData = GetTraderInfo(PmcData);
                    
                    var pmcProfileData = new AdditiveProfileData(baseData)
                    {
                        DiscFromRaid = discFromRaid,
                        IsTransition = isTransition,
                        IsUsingStattrack = false, //TODO: Add support mod Sttattrack
                        LastRaidEXP = ExpLooting,
                        LastRaidHits = HitCount,
                        LastRaidMap = lastRaidLocation,
                        LastRaidMapRaw = lastRaidLocationRaw,
                        LastRaidTransitionTo = lastRaidTransitionTo,
                        AllAchievements = allAchievementsDict,
                        LongestShot = LongestShot,
                        ModWeaponStats = null,
                        PlayedAs = "PMC",
                        PmcSide = PmcData.Side.ToString(),
                        Prestige = PmcData.Info.PrestigeLevel,
                        PublicProfile = true,
                        RaidDamage = NewDamageBody,
                        RegistrationDate = session.Profile.Info.RegistrationDate,
                        TraderInfo = traderInfoData
                    };
                    LeaderboardPlugin.logger.LogWarning($"DATA PMC {JsonConvert.SerializeObject(pmcProfileData)}");

                    LeaderboardPlugin.SendProfileData(pmcProfileData);
                }
                else if (SettingsModel.Instance.PublicProfile.Value && isScavRaid)
                {
                    var traderInfoData = GetTraderInfo(PmcData);
                    
                    var scavProfileData = new AdditiveProfileData(baseData)
                    {
                        DiscFromRaid = discFromRaid,
                        IsTransition = isTransition,
                        IsUsingStattrack = false, //TODO: Add support mod Sttattrack
                        LastRaidEXP = ExpLooting,
                        LastRaidHits = HitCount,
                        LastRaidMap = lastRaidLocation,
                        LastRaidMapRaw = lastRaidLocationRaw,
                        LastRaidTransitionTo = lastRaidTransitionTo,
                        AllAchievements = allAchievementsDict,
                        LongestShot = LongestShot,
                        ModWeaponStats = null,
                        PlayedAs = "SCAV",
                        PmcSide = PmcData.Side.ToString(),
                        Prestige = PmcData.Info.PrestigeLevel,
                        PublicProfile = true,
                        RaidDamage = NewDamageBody,
                        RegistrationDate = session.Profile.Info.RegistrationDate,
                        TraderInfo = traderInfoData
                    };
                    LeaderboardPlugin.logger.LogWarning($"DATA SCAV {JsonConvert.SerializeObject(scavProfileData)}");

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
            "RezervBase" => "Reserve",
            "shoreline" => "Shoreline",
            "woods" => "Woods",
            "lighthouse" => "Lighthouse",
            "TarkovStreets" => "Streets of Tarkov",
            "Sandbox" => "Ground Zero - Low",
            "Sandbox_high" => "Ground Zero - High",
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

    private Dictionary<string, string> TraderMap = new() {
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