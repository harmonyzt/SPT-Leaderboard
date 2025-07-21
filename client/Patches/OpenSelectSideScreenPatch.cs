using System.Reflection;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Matchmaker;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    internal class OpenSelectSideScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(MatchMakerSideSelectionScreen).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(MatchMakerSideSelectionScreen.GClass3629) },
                null
            );

        [PatchPrefix]
        static bool Prefix()
        {
            if (!SettingsModel.Instance.EnableSendData.Value && PlayerHelper.HasRaidStarted())
                return true;
 
            LeaderboardPlugin.logger.LogWarning("Player opened select side screen");
            return true;
        }
    }
}
