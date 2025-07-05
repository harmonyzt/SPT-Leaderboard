using System.Reflection;
using EFT;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;

namespace SPTLeaderboard.Patches
{
    internal class OnEndRaidPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Class303).GetMethod(
                "LocalRaidEnded",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix()
        {
            LeaderboardPlugin.SendHeartbeat(PlayerState.IN_MENU);
            LeaderboardPlugin.logger.LogWarning("Player ended raid");
            return true;
        }
    }
}