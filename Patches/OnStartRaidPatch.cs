using System.Reflection;
using EFT;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;

namespace SPTLeaderboard.Patches
{
    internal class OnStartRaidPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Class303).GetMethod(
                "LocalRaidStarted",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(LocalRaidSettings)},
                null
            );

        [PatchPrefix]
        static bool Prefix()
        {
            LeaderboardPlugin.SendHeartbeat(PlayerState.IN_RAID);
            LeaderboardPlugin.logger.LogWarning("Player started raid");
            return true;
        }
    }
}