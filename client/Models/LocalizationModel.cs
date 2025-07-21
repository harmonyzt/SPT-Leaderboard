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
        public string GetLocaleErrorText(ErrorType errorType, string localeKey = "")
        {
            if (errorType == ErrorType.CAPACITY)
            {
                var localeTypeEquipment = GetLocale(localeKey);
                if (LocalizationData.Error_Capacity.TryGetValue(CurrentLanguage(), out var errorTextExplain))
                {
                    return string.Format(errorTextExplain, localeTypeEquipment);
                }
                
                return string.Format(LocalizationData.Error_Capacity["en"], localeTypeEquipment); // fallback to English
            }
            else
            {
                if (GetLocaleTypeError(errorType).TryGetValue(CurrentLanguage(), out var errorTextExplain))
                {
                   return errorTextExplain;
                }

                return GetLocaleTypeError(errorType)["en"]; // fallback to English 
            }
            
        }
        
        /// <summary>
        /// Get localization by id with current locale in LocaleManagerClass EFT
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetLocale(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return GetLocaleString(id, CurrentLanguage());
            }

            return "Unknown";
        }

        /// <summary>
        /// Get localization by id in LocaleManagerClass EFT
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetLocaleName(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return GetLocaleString(id, "en");
            }

            return "Unknown";
        }

        /// <summary>
        /// Get localization by id and custom locale lang
        /// </summary>
        /// <param name="id"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        private string GetLocaleString(string id, string locale)
        {
            if (string.IsNullOrEmpty(id.Trim()))
            {
                return "Unknown";
            }
            string text;
            if (LocaleManagerClass.LocaleManagerClass.method_8(id, locale, out text))
            {
                return text;
            }
            return "Unknown";
        }

        private Dictionary<string, string> GetLocaleTypeError(ErrorType errorType)
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
                ErrorType.DEVITEMS => LocalizationData.Error_DevItems,
                _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null)
            };
        }
    }
}