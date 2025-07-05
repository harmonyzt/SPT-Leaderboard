using System.Reflection;
using EFT;
using EFT.UI;
using SPT.Reflection.Patching;

namespace SPTLeaderboard.Patches
{
    internal class TriggersPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(MenuScreen).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[] { typeof(Profile), typeof(MatchmakerPlayerControllerClass), typeof(ESessionMode) },
                null
            );

        [PatchPrefix]
        static bool Prefix()
        {
            // LeaderboardPlugin.logger.LogWarning("Player opened menu screen");
            return true;
        }
    }
}