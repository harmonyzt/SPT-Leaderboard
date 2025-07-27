using System.Reflection;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    internal class OpenInventoryScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(InventoryScreen).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                [
                    typeof(IHealthController),
                    typeof(InventoryController),
                    typeof(AbstractQuestControllerClass),
                    typeof(AbstractAchievementControllerClass),
                    typeof(GClass3695),
                    typeof(CompoundItem),
                    typeof(EInventoryTab),
                    typeof(ISession),
                    typeof(ItemContextAbstractClass),
                    typeof(bool)
                ],
                null
            );

        [PatchPrefix]
        static bool Prefix()
        {
            if (!SettingsModel.Instance.EnableSendData.Value)
                return true;
            
            if (!PlayerHelper.HasRaidStarted())
            {
                HeartbeatSender.Send(PlayerState.IN_STASH);
                LeaderboardPlugin.logger.LogWarning("[State] Player opened Inventory screen");
            }

            return true;
        }
    }
}
