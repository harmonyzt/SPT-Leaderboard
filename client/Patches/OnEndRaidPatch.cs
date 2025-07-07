using System.Reflection;
using EFT;
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
        static bool Prefix(LocalRaidSettings settings, GClass1959 results)
        {
            LeaderboardPlugin.Instance.StopInRaidHeartbeat();
            ProcessProfileModel.Create().ProcessAndSendProfile(results, settings);
            LeaderboardPlugin.SendHeartbeat(PlayerState.RAID_END);
            LeaderboardPlugin.logger.LogWarning("[State] Player ended raid");
            return true;
        }
    }
}