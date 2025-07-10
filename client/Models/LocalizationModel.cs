using System;
using System.Collections.Generic;
using System.IO;
using Comfort.Common;
using Newtonsoft.Json;
using SPTLeaderboard.Data;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Models
{
    public class LocalizationModel
    {
        public Dictionary<string, string> LocaleStatTrack = new Dictionary<string, string>();
        public static LocalizationModel Instance { get; private set; }
        
        /// <summary>
        /// Get current language EFT
        /// </summary>
        /// <returns></returns>
        private string CurrentLanguage()
        {
            if (Singleton<SharedGameSettingsClass>.Instance.Game.Settings.Language == null)
            {
                return "en";
            }
            return Singleton<SharedGameSettingsClass>.Instance.Game.Settings.Language;
        }
    
        public static LocalizationModel Create()
        {
            if (Instance != null)
            {
                return Instance;
            }
            return Instance = new LocalizationModel();
        }

        /// <summary>
        /// Get locale text for needly type. See lang in game settings.
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public string GetLocaleErrorText(ErrorType errorType)
        {
            if (GetLocaleTypeError(errorType).TryGetValue(CurrentLanguage(), out var errorTextExplain))
            {
                return errorTextExplain;
            }
        
            return GetLocaleTypeError(errorType)["en"]; // fallback to English
        }

        public LocalizationModel()
        {
            if (File.Exists(GlobalData.LocalePath))
            {
                try
                {
                    var tempLocale = File.ReadAllText(GlobalData.LocalePath);
                    LocaleStatTrack = JsonConvert.DeserializeObject<Dictionary<string, string>>(tempLocale);
                    LeaderboardPlugin.logger.LogWarning("[LocalizationModel] Locale loaded");
                }
                catch (Exception ex)
                {
                    LeaderboardPlugin.logger.LogError("[LocalizationModel] Failed to load locale StatTrack: " + ex.ToString());
                }
                
            }
        }

        public string GetLocaleName(string id)
        {
            if (LocaleStatTrack.Count > 0)
            { 
                LeaderboardPlugin.logger.LogWarning($"[StatTrackLocale] Get name for {id}");
                if (LocaleStatTrack.TryGetValue(id, out var name)) {
                    LeaderboardPlugin.logger.LogWarning($"[StatTrackLocale] Name {id} id {name}");
                    return name;
                }
            }

            return "Unknown";
        }

        public Dictionary<string, string> GetLocaleTypeError(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.VIOLATION_LA_TOS => LocalizationData.Error_LA_TOS,
                ErrorType.TOKEN_MISMATCH => LocalizationData.Error_TokenMismatch,
                ErrorType.TOKEN_NOT_SAFE => LocalizationData.Error_TokenNotSafe,
                ErrorType.UPDATE_MOD => LocalizationData.Error_UpdateMod,
                ErrorType.SCAV_ONLY_PUBLIC => LocalizationData.Error_ScavOnlyPublic,
                ErrorType.CHAR_LIMIT => LocalizationData.Error_CharLimit,
                ErrorType.NSFW_NAME => LocalizationData.Error_NsfwName,
                ErrorType.BALACLAVA => LocalizationData.Error_Balaclava,
                _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null)
            };
        }
    }
}