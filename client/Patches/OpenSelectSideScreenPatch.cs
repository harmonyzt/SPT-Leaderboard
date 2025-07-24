using System;
using System.Linq;
using System.Reflection;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.Matchmaker;
using SPT.Reflection.Patching;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    internal class OpenSelectSideScreenPatch : ModulePatch
    {
        private static DateTime? _lastSendTime = null;
        private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(10);
        
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
            LeaderboardPlugin.logger.LogWarning("Player opened select side screen");
            if (!SettingsModel.Instance.EnableSendData.Value && PlayerHelper.HasRaidStarted())
                return true;

            PlayerHelper.GetLimitViolations(PlayerHelper.GetEquipmentData());
            
            // If it has not yet been 10 minutes since the last call - we do nothing
            if (!LeaderboardPlugin.Instance.canPreRaidCheck)
            {
                return true;
            }

            var modsPlayer = DataUtils.GetServerMods()
                .Concat(DataUtils.GetDirectories(GlobalData.UserModsPath))
                .Concat(DataUtils.GetDirectories(BepInEx.Paths.PluginPath))
                .Concat(DataUtils.GetDirectories(BepInEx.Paths.PluginPath))
                .ToList();
            
            var preraidData = new PreRaidData
            {
                VersionMod = "3.1.0",//TODO: Replace by GlobalData.Version before prod
                IsCasual = SettingsModel.Instance.ModCasualMode.Value,
#if DEBUG
                Mods = SettingsModel.Instance.Debug.Value ? ["IhanaMies-LootValueBackend", "SpecialSlots"] : modsPlayer,
                Hash = SettingsModel.Instance.Debug.Value
                    ? "fb75631b7a153b1b95cdaa7dfdc297b4a7c40f105584561f78e5353e7e925c6f"
                    : EncryptionModel.Instance.GetHashMod()
#else
                Mods = modsPlayer,
                Hash = EncryptionModel.Instance.GetHashMod()
#endif
            };
            
            LeaderboardPlugin.SendPreRaidData(preraidData);
            LeaderboardPlugin.Instance.StartPreRaidCheckTimer();
            return true;
        }
    }
}
