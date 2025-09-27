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
        protected override MethodBase GetTargetMethod() =>
            typeof(MatchMakerSideSelectionScreen).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                [typeof(MatchMakerSideSelectionScreen.GClass3629)],
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
            
            var preRaidData = new PreRaidData
            {
                VersionMod = GlobalData.Version,
                IsCasual = SettingsModel.Instance.ModCasualMode.Value,
#if DEBUG
                Mods = SettingsModel.Instance.Debug.Value ? ["IhanaMies-LootValueBackend", "SpecialSlots"] : modsPlayer,
#else
                Mods = modsPlayer,
#endif
                Hash = EncryptionModel.Instance.GetHashMod()
            };
            
            LeaderboardPlugin.SendPreRaidData(preRaidData);
            LeaderboardPlugin.Instance.StartPreRaidCheckTimer();
            return true;
        }
    }
}
