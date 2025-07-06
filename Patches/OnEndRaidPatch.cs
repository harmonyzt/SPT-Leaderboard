using System.Reflection;
using EFT;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Patches
{
    internal class OnEndRaidPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Class303).GetMethod(
                "LocalRaidEnded",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix(GClass1959 results)
        {
            LeaderboardPlugin.Instance.StopInRaidHeartbeat();
            ProcessProfileModel.Create().ProcessProfile(results);
            LeaderboardPlugin.SendHeartbeat(PlayerState.RAID_END);
            LeaderboardPlugin.logger.LogWarning("Player ended raid");
            return true;
        }
    }
}