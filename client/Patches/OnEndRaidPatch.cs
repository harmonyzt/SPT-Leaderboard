using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    internal class OnEndRaidPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Class303).GetMethod(
                "LocalRaidEnded",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix(LocalRaidSettings settings, GClass1959 results)
        {
            if (!SettingsModel.Instance.EnableSendData.Value)
                return true;

            var currentDataHits = HitsTracker.Instance.GetHitsData();
            LeaderboardPlugin.Instance.StopInRaidHeartbeat();
            ProcessProfileModel.Create().ProcessAndSendProfile(results, settings, currentDataHits);
            
            HeartbeatSender.Send(results.result == ExitStatus.Transit ? PlayerState.IN_TRANSIT : PlayerState.RAID_END);
            
            LeaderboardPlugin.logger.LogWarning("[State] Player ended raid");
            return true;
        }
    }
}