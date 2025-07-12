using System.Reflection;
using EFT;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    internal class OpenMainMenuScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(MenuScreen).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[]
                {
                    typeof(Profile), 
                    typeof(MatchmakerPlayerControllerClass),
                    typeof(ESessionMode)
                },
                null
            );

        [PatchPrefix]
        static bool Prefix()
        {
            if (!SettingsModel.Instance.EnableSendData.Value)
                return true;
            
            if (!PlayerHelper.HasRaidStarted())
            {
                HeartbeatSender.Send(PlayerState.IN_MENU);
                LeaderboardPlugin.logger.LogWarning("[State] Player opened MainMenu screen");
                return true;
            }
            return true;
        }
    }
}