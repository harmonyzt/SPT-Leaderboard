using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    public class OnGameWorldStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GameWorld).GetMethod(
                "OnGameStarted",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        static void PostFix()
        {
#if DEBUG
            OverlayDebug.Instance.Enable();
#endif
            LeaderboardPlugin.logger.LogWarning("Player started world");
        }
    }
}