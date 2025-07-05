using System.Reflection;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;

namespace SPTLeaderboard.Patches
{
    internal class OpenInventoryScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(InventoryScreen).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[]
                {
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
                },
                null
            );

        [PatchPrefix]
        static bool Prefix()
        {
            if (!LeaderboardPlugin.HasRaidStarted())
            {
                LeaderboardPlugin.SendHeartbeat(PlayerState.IN_STASH);
                LeaderboardPlugin.logger.LogWarning("Player opened Inventory screen");
                return true;
            }

            return true;
        }
    }
}
