using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    public class PlayerOnDeadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "OnDead");
        }
        
        [PatchPostfix]
        public static void PatchPostfix(Player __instance)
        {
            if (__instance != null)
            {
                if (PlayerHelper.Instance.Player == __instance)
                {
                    LeaderboardPlugin.logger.LogWarning(
                        "PlayerOnDeadPatch: Player is already dead. Set position dead");
                    PlayerHelper.Instance.LastDeathPosition =
                        PlayerHelper.ConvertToMapPosition(__instance.PlayerBones.BodyTransform.position);
                }
            }
        }
    }
}