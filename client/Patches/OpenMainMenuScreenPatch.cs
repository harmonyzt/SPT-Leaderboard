using System.Reflection;
using EFT;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    internal class OpenMainMenuScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(MenuScreen).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[]
                {
                    typeof(Profile), 
                    typeof(MatchmakerPlayerControllerClass),
                    typeof(ESessionMode)
                },
                null
            );

        [PatchPrefix]
        static bool Prefix()
        {
            if (!LeaderboardPlugin.Instance.cachedPlayerModelPreview)
            {
                LeaderboardPlugin.Instance.CacheFullBodyPlayerModelView();
            }
            
            if (!LeaderboardPlugin.Instance.engLocaleLoaded)
            { 
                bool hasEnLocale = LocaleManagerClass.LocaleManagerClass.dictionary_4.TryGetValue("en", out _);
                if (!hasEnLocale)
                {
                    _ = LocalizationModel.Instance.LoadEnglishLocaleAsync();
                }
                else
                {
                    LeaderboardPlugin.Instance.engLocaleLoaded = true;
                }
            }
            
            if (!SettingsModel.Instance.EnableSendData.Value)
                return true;
            
            if (!PlayerHelper.HasRaidStarted())
            {
                HeartbeatSender.Send(PlayerState.IN_MENU);
                LeaderboardPlugin.logger.LogWarning("[State] Player opened MainMenu screen");
            }
            
            return true;
        }
    }
}