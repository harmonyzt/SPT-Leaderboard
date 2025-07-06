using Comfort.Common;
using EFT;
using EFT.UI;
using SPTLeaderboard.Data;
using SPTLeaderboard.Utils;
using UnityEngine;

namespace SPTLeaderboard.Models;

public class ProcessProfileModel
{
    public static ProcessProfileModel Instance { get; private set; }

    public void ProcessProfile(GClass1959 resultRaid)
    {
        if (Singleton<PreloaderUI>.Instantiated)
        {
            var session = DataUtils.GetSession();
            if (session.Profile != null)
            {
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
                        LeaderboardPlugin.logger.LogWarning($"TRANSITION MAP {lastRaidTransitionTo}");
                    }
                    else
                    {
                        isTransition = false;
                        lastRaidTransitionTo = "None";
                    }
                }

                
                var baseData = new BaseData
                {
                    AccountType = session.Profile.Info.GameVersion,
                    Health = 440,
                    Id = session.Profile.Id,
                    IsScav = session.Profile.Side == EPlayerSide.Savage,
                    LastPlayed = DataUtils.CurrentTimestamp,
                    ModInt = "fb75631b7a153b1b95cdaa7dfdc297b4a7c40f105584561f78e5353e7e925c6f",
                    Mods = ["IhanaMies-LootValueBackend", "SpecialSlots"],
                    Name = session.Profile.Nickname,
                    PmcHealth = 440,
                    PmcLevel = session.GetProfileBySide(ESideType.Pmc).Info.Level,
                    RaidKills = 1,
                    RaidResult = resultRaid.result.ToString(),
                    RaidTime = resultRaid.playTime,
                    SptVersion = DataUtils.ParseVersion(PlayerPrefs.GetString("SPT_Version")),
                    Token = EncryptionModel.Instance.Token,
                    DBinInv = false,
                    IsCasual = SettingsModel.Instance.ModCasualMode.Value
                };
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