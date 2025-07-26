using System.Reflection;
using EFT;
using EFT.UI.Matchmaker;
using SPT.Reflection.Patching;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Patches
{
    internal class OpenLoadingRaidScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(MatchmakerTimeHasCome).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                [
                    typeof(ISession), 
                    typeof(RaidSettings),
                    typeof(MatchmakerPlayerControllerClass)
                ],
                null
            );

        [PatchPrefix]
        static bool Prefix()
        {
            if (!SettingsModel.Instance.EnableSendData.Value)
                return true;
            
            LeaderboardPlugin.logger.LogWarning("Player opened Loading raid screen");
            LeaderboardPlugin.Instance.CreateIconFullBodyPlayer();
            return true;
        }
    }
}