using System.Reflection;
using SPT.Reflection.Patching;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

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
            
            HitsTracker.Instance.Clear();
            LeaderboardPlugin.Instance.StartInRaidHeartbeat();
            LeaderboardPlugin.logger.LogWarning("[State] Player started raid");
            return true;
        }
    }
}