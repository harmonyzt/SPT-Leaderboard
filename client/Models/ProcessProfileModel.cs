using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.UI;
using Newtonsoft.Json;
using SPTLeaderboard.Data;
using SPTLeaderboard.Utils;
using UnityEngine;
using TraderData = SPTLeaderboard.Data.TraderData;

namespace SPTLeaderboard.Models;

public class ProcessProfileModel
{
    public static ProcessProfileModel Instance { get; private set; }

    public void ProcessAndSendProfile(GClass1959 resultRaid, LocalRaidSettings localRaidSettings, HitsData hitsData, List<float> dataDistanceHits)
    {
        if (!SettingsModel.Instance.EnableSendData.Value || PlayerHelper.GetLimitViolationsSilent(PlayerHelper.GetEquipmentData()))
            return;
        
        if (Singleton<PreloaderUI>.Instantiated)
        {
            var session = PlayerHelper.GetSession();
            if (session.Profile != null)
            {
                var profileID = session.Profile.Id;
                
                var pmcData = session.GetProfileBySide(ESideType.Pmc);
                var scavData = session.GetProfileBySide(ESideType.Savage);

                string nameKiller = "";
                if (resultRaid.result == ExitStatus.Killed)
                {
                    nameKiller = PlayerHelper.TryGetAgressorName(session.Profile);
                    if (string.IsNullOrEmpty(nameKiller))
                    {
                        nameKiller = PlayerHelper.TryGetAgressorName(scavData);
                    }
                }
                
                var gameVersion = session.Profile.Info.GameVersion;
                var lastRaidLocationRaw = localRaidSettings.location;
                var lastRaidLocation = DataUtils.GetPrettyMapName(lastRaidLocationRaw.ToLower());
                
                ProfileData profileData = null;
                try
                {
                    profileData = JsonConvert.DeserializeObject<ProfileData>(resultRaid.profile.JObject.ToString());
                }
                catch (Exception e)
                {
                    LeaderboardPlugin.logger.LogError($"Cant parse data profile {e.Message}");
                    return;
                }
                
                bool isScavRaid = session.Profile.Side == EPlayerSide.Savage;
                if (profileData != null)
                {
                    isScavRaid = profileData.Info.Side == "Savage";
                }
                
                bool discFromRaid = resultRaid.result == ExitStatus.Left;
                
                var isTransition = false;
                var lastRaidTransitionTo = "None";

                DataUtils.TryGetTransitionData(resultRaid, (s, b) =>
                {
                    lastRaidTransitionTo = s;
                    isTransition = b;
                });

                var allAchievementsDict = pmcData.AchievementsData.ToDictionary(
                    pair => pair.Key.ToString(),
                    pair => pair.Value
                );
                
                #region CheckGodBalaclava
                
                var allItemsRaw = pmcData.Inventory.GetPlayerItems();
                var allItems = allItemsRaw.ToList();
                
                bool haveDevItems = DataUtils.CheckDevItems(allItems);
                
                if (haveDevItems)
                {
                    LocalizationModel.NotificationWarning(LocalizationModel.Instance.GetLocaleErrorText(ErrorType.DEVITEMS),
                        ServerErrorHandler.GetDurationType(ErrorType.DEVITEMS));
#if DEBUG
                    if (SettingsModel.Instance.Debug.Value)
                    {
                        haveDevItems = false;
                    }
                    else
                    {
                        return;
                    }
#else
                    return;
#endif
                }
                
                #endregion
                
                #region CheckHasKappa

                bool hasKappa = DataUtils.CheckHasKappa(allItems);
                
                #endregion

                #region Stats
                
                #region PMCStats
                
                var MaxHealth = pmcData.Health.BodyParts.Where(
                    bodyPart => bodyPart.Value?.Health != null).
                    Sum(bodyPart => bodyPart.Value.Health.Maximum);
                                
                var CurrentHealth = pmcData.Health.BodyParts.Where(
                    bodyPart => bodyPart.Value?.Health != null).
                    Sum(bodyPart => bodyPart.Value.Health.Current);
                
                var KilledPmc = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledPmc);
                var KilledSavage = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledSavage);
                var KilledBoss = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledBoss);
                var LongestShot = (int)session.Profile.Stats.Eft.SessionCounters.GetFloat(SessionCounterTypesAbstractClass.LongestShot);
                var ExpLooting = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.ExpLooting);
                var HitCount = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.HitCount);
                var TotalDamage = (int)session.Profile.Stats.Eft.SessionCounters.GetFloat(SessionCounterTypesAbstractClass.CauseBodyDamage);
                var AverageShot = 0.0f;
                if (dataDistanceHits.Count > 1)
                {
                    AverageShot = dataDistanceHits.Average();
                    AverageShot = (float)Math.Round(AverageShot, 1);
#if DEBUG || BETA
                    LeaderboardPlugin.logger.LogWarning($"[Session Counter] AverageShot {AverageShot}");
#endif
                }

                LeaderboardPlugin.logger.LogWarning($"Death coordinates {PlayerHelper.Instance.LastDeathPosition}");

                PlayerHelper.Instance.LastDeathPosition = Vector3.zero;
                if (!isScavRaid)
                {
#if DEBUG || BETA
                    LeaderboardPlugin.logger.LogWarning($"\n");
                    LeaderboardPlugin.logger.LogWarning($"\n[Session Counter] KilledPmc {KilledPmc}");
                    LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledSavage {KilledSavage}");
                    LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledBoss {KilledBoss}");
                    LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestShot {LongestShot}");
                    LeaderboardPlugin.logger.LogWarning($"[Session Counter] CauseBodyDamage {TotalDamage}");
                    LeaderboardPlugin.logger.LogWarning($"[Session Counter] ExpLooting {ExpLooting}");
                    LeaderboardPlugin.logger.LogWarning($"[Session Counter] HitCount {HitCount}");
#endif
                }

                #endregion
                
                #region ScavStats
                
                if (isScavRaid)
                {
                    KilledPmc = scavData.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledPmc);
                    KilledSavage = scavData.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledSavage);
                    KilledBoss = scavData.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledBoss);
                    LongestShot = (int)scavData.Stats.Eft.SessionCounters.GetFloat(SessionCounterTypesAbstractClass.LongestShot);
                    HitCount = scavData.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.HitCount);
                    TotalDamage = (int)scavData.Stats.Eft.SessionCounters.GetFloat(SessionCounterTypesAbstractClass.CauseBodyDamage);

#if DEBUG || BETA
                        LeaderboardPlugin.logger.LogWarning($"\n");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledPmc Scav {KilledPmc}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledSavage Scav {KilledSavage}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledBoss Scav {KilledBoss}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestShot Scav {LongestShot}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] HitCount Scav {HitCount}");
                        LeaderboardPlugin.logger.LogWarning($"[Session Counter] CauseBodyDamage Scav {TotalDamage}");
#endif
                }
                
                #endregion
                
                if (HitCount <= 0) {
                    HitCount = 0;
                }
                
                #endregion
                
                var listModsPlayer = DataUtils.GetServerMods()
                    .Concat(DataUtils.GetUserMods())
                    .Concat(DataUtils.GetBepinexMods())
                    .Concat(DataUtils.GetBepinexDll())
                    .ToList();
                
                #region StatTrack

                var statTrackIsUsed = StatTrackInterop.Loaded();
                Dictionary<string, Dictionary<string, WeaponInfo>> processedStatTrackData = new Dictionary<string, Dictionary<string, WeaponInfo>>();
                
                if (!SettingsModel.Instance.EnableModSupport.Value && !statTrackIsUsed)
                {
                    LeaderboardPlugin.logger.LogWarning(
                        $"StatTrack process data skip. StatTrack Find? : {statTrackIsUsed} | Enabled Mod Support? : {SettingsModel.Instance.EnableModSupport.Value}");
                    processedStatTrackData = null;
                }
                else
                {
                    LeaderboardPlugin.logger.LogWarning($"Loaded StatTrack plugin {statTrackIsUsed}");
                    
                    var dataStatTrack = StatTrackInterop.LoadFromServer();
                    if (dataStatTrack != null)
                    {
#if DEBUG || BETA
                        LeaderboardPlugin.logger.LogWarning(
                            $"Data raw StatTrack {JsonConvert.SerializeObject(dataStatTrack).ToJson()}");
#endif
                        processedStatTrackData = StatTrackInterop.GetAllValidWeapons(profileID ,dataStatTrack);
#if DEBUG || BETA
                        if (processedStatTrackData != null)
                        {
                            LeaderboardPlugin.logger.LogWarning("processedStatTrackData != null: Data -> "+JsonConvert.SerializeObject(processedStatTrackData).ToJson());
                        }
#endif
                    }
                }

                #endregion
                
                var baseData = new BaseData {
                    AccountType = gameVersion,
                    Health = CurrentHealth,
                    Id = profileID,
                    IsScav = isScavRaid,
                    LastPlayed = DataUtils.CurrentTimestamp,
#if DEBUG
                    ModInt = SettingsModel.Instance.Debug.Value ? "445ca392f8b6b353a82962aee50a097e5a0eacb9fcbf20624e8cd7fe4862161b" : EncryptionModel.Instance.GetHashMod(), //TODO: Delete debug. BEFORE PROD
                    Mods = SettingsModel.Instance.Debug.Value ? ["IhanaMies-LootValueBackend", "SpecialSlots"] : listModsPlayer, //TODO: Delete debug. BEFORE PROD
#else
                    ModInt = EncryptionModel.Instance.GetHashMod(),
                    Mods = listModsPlayer,
#endif
                    Name = session.Profile.Nickname,
                    PmcHealth = MaxHealth,
                    PmcLevel = pmcData.Info.Level,
                    RaidKills = KilledPmc,
                    RaidResult = resultRaid.result.ToString(),
                    RaidTime = resultRaid.playTime,
                    SptVersion = DataUtils.GetSptVersion(),
                    Token = EncryptionModel.Instance.Token,
                    DBinInv = haveDevItems,
                    IsCasual = SettingsModel.Instance.ModCasualMode.Value
                };

                if (!SettingsModel.Instance.PublicProfile.Value)
                {
                    var privateProfileData = new PrivateProfileData(baseData);

#if DEBUG
                    LeaderboardPlugin.logger.LogWarning(
                        $"DATA privateProfileData {JsonConvert.SerializeObject(privateProfileData)}");
#endif
                    
#if BETA
                    var betaDataPrivateProfile = PrivateProfileData.MakeBetaCopy(privateProfileData);
                    betaDataPrivateProfile.ModInt = "BETA";
                    betaDataPrivateProfile.Mods = ["BETA"];
                    betaDataPrivateProfile.Token = "BETA";
                    
                    LeaderboardPlugin.logger.LogWarning(
                        $"DATA privateProfileData {JsonConvert.SerializeObject(betaDataPrivateProfile)}");
#endif

                    LeaderboardPlugin.SendRaidData(privateProfileData);
                }
                else if (SettingsModel.Instance.PublicProfile.Value && !isScavRaid)
                {
                    var traderInfoData = DataUtils.GetTraderInfo(pmcData);
                    
                    var pmcProfileData = new AdditiveProfileData(baseData)
                    {
                        DiscFromRaid = discFromRaid,
                        AgressorName = nameKiller,
                        IsTransition = isTransition,
                        IsUsingStattrack = statTrackIsUsed,
                        LastRaidEXP = ExpLooting,
                        LastRaidHits = HitCount,
                        LastRaidMap = lastRaidLocation,
                        LastRaidMapRaw = lastRaidLocationRaw,
                        LastRaidTransitionTo = lastRaidTransitionTo,
                        RaidHits = hitsData,
                        AllAchievements = allAchievementsDict,
                        LongestShot = LongestShot,
                        AverageShot = AverageShot,
                        DiedAtX = PlayerHelper.Instance.LastDeathPosition.x,
                        DiedAtY = PlayerHelper.Instance.LastDeathPosition.y,
                        BossKills = KilledBoss,
                        SavageKills = KilledSavage,
                        ModWeaponStats = processedStatTrackData,
                        PlayedAs = "PMC",
                        PmcSide = pmcData.Side.ToString(),
                        Prestige = pmcData.Info.PrestigeLevel,
                        PublicProfile = true,
                        HasKappa = hasKappa,
                        ScavLevel = scavData.Info.Level, 
                        RaidDamage = TotalDamage,
                        RegistrationDate = session.Profile.Info.RegistrationDate,
                        TraderInfo = traderInfoData
                    };
                    
#if DEBUG
                    LeaderboardPlugin.logger.LogWarning($"DATA PMC {JsonConvert.SerializeObject(pmcProfileData)}");
#endif
                    
#if BETA
                    var betaDataPmcProfile = AdditiveProfileData.MakeBetaCopy(pmcProfileData);
                    betaDataPmcProfile.ModInt = "BETA";
                    betaDataPmcProfile.Mods = ["BETA"];
                    betaDataPmcProfile.Token = "BETA";
                    
                    LeaderboardPlugin.logger.LogWarning($"DATA PMC {JsonConvert.SerializeObject(betaDataPmcProfile)}");
#endif

                    LeaderboardPlugin.SendRaidData(pmcProfileData);
                }
                else if (SettingsModel.Instance.PublicProfile.Value && isScavRaid)
                {
                    var traderInfoData = DataUtils.GetTraderInfo(pmcData);
                    
                    var scavProfileData = new AdditiveProfileData(baseData)
                    {
                        DiscFromRaid = discFromRaid,
                        AgressorName = nameKiller,
                        IsTransition = isTransition,
                        IsUsingStattrack = statTrackIsUsed,
                        LastRaidEXP = 0,
                        LastRaidHits = HitCount,
                        LastRaidMap = lastRaidLocation,
                        LastRaidMapRaw = lastRaidLocationRaw,
                        LastRaidTransitionTo = lastRaidTransitionTo,
                        RaidHits = hitsData,
                        AllAchievements = allAchievementsDict,
                        LongestShot = LongestShot,
                        BossKills = KilledBoss,
                        SavageKills = KilledSavage,
                        ModWeaponStats = processedStatTrackData,
                        PlayedAs = "SCAV",
                        PmcSide = pmcData.Side.ToString(),
                        Prestige = pmcData.Info.PrestigeLevel,
                        PublicProfile = true,
                        HasKappa = hasKappa,
                        ScavLevel = scavData.Info.Level, 
                        RaidDamage = TotalDamage,
                        RegistrationDate = session.Profile.Info.RegistrationDate,
                        TraderInfo = traderInfoData
                    };
                    
#if DEBUG
                    LeaderboardPlugin.logger.LogWarning(
                        $"DATA SCAV {JsonConvert.SerializeObject(scavProfileData)}");
#endif
                    
#if BETA
                    var betaDataScavProfile = AdditiveProfileData.MakeBetaCopy(scavProfileData);
                    betaDataScavProfile.ModInt = "BETA";
                    betaDataScavProfile.Mods = ["BETA"];
                    betaDataScavProfile.Token = "BETA";
                    
                    LeaderboardPlugin.logger.LogWarning($"DATA SCAV {JsonConvert.SerializeObject(betaDataScavProfile)}");
#endif

                    LeaderboardPlugin.SendRaidData(scavProfileData);
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
}