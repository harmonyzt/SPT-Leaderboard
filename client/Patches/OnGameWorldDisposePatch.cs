using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    public class OnGameWorldDisposePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GameWorld).GetMethod(
                "Dispose",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static void Prefix()
        {
            OverlayDebug.Instance.Disable();
            LeaderboardPlugin.logger.LogWarning("Player dispose world");
        }
    }
}