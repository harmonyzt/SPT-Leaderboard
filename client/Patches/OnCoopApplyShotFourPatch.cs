using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches;

public class OnCoopApplyShotFourPatch: ModulePatch
{
    protected override MethodBase GetTargetMethod() => DataUtils.GetPluginType(DataUtils.FikaCore, "Fika.Core.Coop.ObservedClasses.ObservedClientBridge")
        .GetMethod("ApplyShot", BindingFlags.Instance | BindingFlags.Public);
    
    [PatchPostfix]
    static void PostFix(DamageInfoStruct damageInfo, EBodyPart bodyPart, EBodyPartColliderType bodyPartCollider)
    {
        if (!SettingsModel.Instance.EnableSendData.Value)
            return;
            
        LeaderboardPlugin.logger.LogWarning("[ApplyShot ObservedClientBridge] hit");
        IPlayerOwner player = damageInfo.Player;
        LeaderboardPlugin.logger.LogWarning($"[ApplyShot ObservedClientBridge] {player?.Nickname}");
        if ((Player)((player != null) ? player.iPlayer : null) != PlayerHelper.Instance.Player)
        {
            return;
        }
        
        HitsTracker.Instance.IncreaseHit(bodyPart);
        
#if DEBUG
        OverlayDebug.Instance.UpdateOverlay();
            
        LeaderboardPlugin.logger.LogWarning("[ApplyShot ObservedClientBridge] Player hit");
        LeaderboardPlugin.logger.LogWarning($"[ApplyShot ObservedClientBridge] Player shooted to damageInfo.BodyPartColliderType {damageInfo.BodyPartColliderType.ToString()}");
        LeaderboardPlugin.logger.LogWarning($"[ApplyShot ObservedClientBridge] Player shooted to bodyPartType {bodyPart.ToString()}");
        LeaderboardPlugin.logger.LogWarning($"[ApplyShot ObservedClientBridge] Player shooted to EBodyPartColliderType {bodyPartCollider.ToString()}");
#endif
    }
}