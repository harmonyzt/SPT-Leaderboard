using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.UI;
using Newtonsoft.Json;
using SPTLeaderboard.Data;
using SPTLeaderboard.Utils;
using UnityEngine;

namespace SPTLeaderboard.Models;

public class ProcessProfileModel
{
    public static ProcessProfileModel Instance { get; private set; }

    public void ProcessAndSendProfile(GClass1959 resultRaid)
    {
        if (Singleton<PreloaderUI>.Instantiated)
        {
            var session = DataUtils.GetSession();
            if (session.Profile != null)
            {
                var profileID = session.Profile.Id;
                
                var gameVersion = session.Profile.Info.GameVersion;
                
                var dataPmc = session.GetProfileBySide(ESideType.Pmc);
                var dataScav = session.GetProfileBySide(ESideType.Savage);

                bool isScavRaid = session.Profile.Side == EPlayerSide.Savage;
                
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
                        lastRaidTransitionTo = transitController.localRaidSettings_0.location;
                        var locationTransit = transitController.alreadyTransits[resultRaid.ProfileId];
                        LeaderboardPlugin.logger.LogWarning($"TRANSITION MAP {lastRaidTransitionTo}");
                        LeaderboardPlugin.logger.LogWarning($"TRANSITION MAP 2 {locationTransit.location}");
                    }
                    else
                    {
                        isTransition = false;
                        lastRaidTransitionTo = "None";
                    }
                }

                var MaxHealth = dataPmc.Health.BodyParts.Where(
                    bodyPart => bodyPart.Value?.Health != null
                    ).Sum(
                    bodyPart => bodyPart.Value.Health.Maximum);
                
                var CurrentHealth = dataPmc.Health.BodyParts.Where(
                    bodyPart => bodyPart.Value?.Health != null
                ).Sum(
                    bodyPart => bodyPart.Value.Health.Current);
                
                
                var Kills = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.Kills);
                var KilledSavage = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledSavage);
                var KilledPmc = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledPmc);
                var KilledBear = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledBear);
                var KilledBoss = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.KilledBoss);
                var HeadShots = session.Profile.Stats.Eft.SessionCounters.GetInt(SessionCounterTypesAbstractClass.HeadShots);
                var LongestShot = session.Profile.Stats.Eft.SessionCounters.GetLong(SessionCounterTypesAbstractClass.LongestShot);
                var LongestKillShot = session.Profile.Stats.Eft.SessionCounters.GetLong(SessionCounterTypesAbstractClass.LongestKillShot);
                var LongestKillStreak = session.Profile.Stats.Eft.SessionCounters.GetLong(SessionCounterTypesAbstractClass.LongestKillStreak);
                
                
                LeaderboardPlugin.logger.LogWarning($"\n[Session Counter] Kills {Kills}");
                LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledSavage {KilledSavage}");
                LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledPmc {KilledPmc}");
                LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledBear {KilledBear}");
                LeaderboardPlugin.logger.LogWarning($"[Session Counter] KilledBoss {KilledBoss}");
                LeaderboardPlugin.logger.LogWarning($"[Session Counter] HeadShots {HeadShots}");
                LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestShot {LongestShot}");
                LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestKillShot {LongestKillShot}");
                LeaderboardPlugin.logger.LogWarning($"[Session Counter] LongestKillStreak {LongestKillStreak}\n");

                var baseData = new BaseData
                {
                    AccountType = gameVersion,
                    Health = MaxHealth, //Maybe use current health?
                    Id = profileID,
                    IsScav = isScavRaid, //Doubt that it is working
                    LastPlayed = DataUtils.CurrentTimestamp,
                    ModInt = EncryptionModel.Instance.GetHashMod(),
                    Mods = ["IhanaMies-LootValueBackend", "SpecialSlots"],
                    Name = session.Profile.Nickname,
                    PmcHealth = MaxHealth,
                    PmcLevel = dataPmc.Info.Level,
                    RaidKills = Kills,
                    RaidResult = resultRaid.result.ToString(),
                    RaidTime = resultRaid.playTime,
                    SptVersion = DataUtils.GetSptVersion(),
                    Token = EncryptionModel.Instance.Token,
                    DBinInv = false,
                    IsCasual = SettingsModel.Instance.ModCasualMode.Value
                };

                if (!SettingsModel.Instance.PublicProfile.Value)
                {
                    var privateProfileData = new PrivateProfileData()
                    {
                        AccountType = baseData.AccountType,
                        Health = baseData.Health,
                        Id = baseData.Id,
                        IsScav = baseData.IsScav,
                        LastPlayed = baseData.LastPlayed,
                        ModInt = baseData.ModInt,
                        Mods = baseData.Mods,
                        Name = baseData.Name,
                        PmcHealth = baseData.PmcHealth,
                        PmcLevel = baseData.PmcLevel,
                        RaidKills = baseData.RaidKills,
                        RaidResult = baseData.RaidResult,
                        RaidTime = baseData.RaidTime,
                        SptVersion = baseData.SptVersion,
                        Token = baseData.Token,
                        DBinInv = baseData.DBinInv,
                        IsCasual = baseData.IsCasual,
                        IsPublicProfile = false
                    };
                    
                    LeaderboardPlugin.SendProfileData(privateProfileData);
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