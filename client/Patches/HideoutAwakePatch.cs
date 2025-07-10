using System.Reflection;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    internal class HideoutAwakePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutScreenOverlay), nameof(HideoutScreenOverlay.Show));
        }

        [PatchPostfix]
        private static void Postfix()
        {
            HeartbeatSender.Send(PlayerState.IN_HIDEOUT);
            LeaderboardPlugin.logger.LogWarning("[State] Player entered in hideout");
        }
    }
}