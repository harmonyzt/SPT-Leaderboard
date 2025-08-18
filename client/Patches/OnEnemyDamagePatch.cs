using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using SPT.Reflection.Patching;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    public class OnEnemyDamagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetType = typeof(LocationStatisticsCollectorAbstractClass);
            return targetType?.GetMethod(
                "OnEnemyDamage",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
        }

        [PatchPostfix]
        static void PostFix(
            object __instance,
            DamageInfoStruct damage,
            EBodyPart bodyPart,
            string playerProfileId,
            EPlayerSide playerSide,
            WildSpawnType role,
            string groupId,
            float fullHealth,
            bool isHeavyDamage,
            float distance,
            int hour,
            List<string> targetEquipment,
            HealthEffects enemyEffects,
            List<string> zoneIds)
        {
            if (!SettingsModel.Instance.EnableSendData.Value)
                return;
            
            if (!(damage.Weapon is ThrowWeapItemClass))
            {
                HitsTracker.Instance.AddHit(distance);
            }
            
#if DEBUG || BETA
            LeaderboardPlugin.logger.LogWarning($"[OnEnemyDamage Postfix] side={playerSide} role={role} body={bodyPart} dist={distance:0.0} heavy={isHeavyDamage}");
#endif
        }
    }
}