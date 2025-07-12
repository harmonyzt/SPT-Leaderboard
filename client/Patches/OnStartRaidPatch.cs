using System.Reflection;
using System.Timers;
using EFT;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Patches
{
    internal class OnStartRaidPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Class303).GetMethod(
                "LocalRaidStarted",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix()
        {
            if (!SettingsModel.Instance.EnableSendData.Value)
                return true;
            
            LeaderboardPlugin.Instance.StartInRaidHeartbeat();
            LeaderboardPlugin.logger.LogWarning("[State] Player started raid");
            return true;
        }
    }
}