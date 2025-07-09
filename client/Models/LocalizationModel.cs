using System;
using System.Collections.Generic;
using System.IO;
using Comfort.Common;
using Newtonsoft.Json;
using SPTLeaderboard.Data;

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
        /// <param name="type"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public string GetLocaleErrorText(TypeText type)
        {
            if (type == TypeText.ErrorRules)
            {
                if (LocalizationData.ErrorRules.TryGetValue(CurrentLanguage(), out var addOfferLocale))
                {
                    return addOfferLocale;
                }
            
                return LocalizationData.ErrorRules["en"]; // fallback to English
            }

            if (type == TypeText.ErrorBanned)
            {
                if (LocalizationData.ErrorBanned.TryGetValue(CurrentLanguage(), out var addOfferLocale))
                {
                    return addOfferLocale;
                }

                return LocalizationData.ErrorBanned["en"]; // fallback to English
            }

            return "Leaderboard error. Check logs";
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
    }

    public enum TypeText
    {
        ErrorRules,
        ErrorBanned
    }
    
    
}