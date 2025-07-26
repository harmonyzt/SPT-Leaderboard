using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    internal class OnApplyDamageInfoPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Player).GetMethod(
                "ApplyDamageInfo",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix(DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType)
        {
            if (!SettingsModel.Instance.EnableSendData.Value)
                return true;
            
#if DEBUG
            LeaderboardPlugin.logger.LogWarning("[ProcessShot Local] Hit");
#endif
            IPlayerOwner player = damageInfo.Player;
#if DEBUG
            LeaderboardPlugin.logger.LogWarning($"[ProcessShot Local] Nick -> {player?.Nickname}");
#endif
            if ((Player)((player != null) ? player.iPlayer : null) != PlayerHelper.Instance.Player)
            {
                return true;
            }

            HitsTracker.Instance.IncreaseHit(bodyPartType);
#if DEBUG
            OverlayDebug.Instance.UpdateOverlay();
            LeaderboardPlugin.logger.LogWarning($"[ProcessShot Local] Hit BodyType {bodyPartType.ToString()}");
            LeaderboardPlugin.logger.LogWarning($"[ProcessShot Local] Hit EBodyPartColliderType {colliderType.ToString()}");
#endif
            
            return true;
        }
    }
}