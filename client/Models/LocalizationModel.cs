using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Communications;
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

        public string GetLocaleCoin(int value = 0)
        {
            if (LocalizationData.AddCoin.TryGetValue(CurrentLanguage(), out var textAddCoin))
            {
                var formatedText = string.Format(textAddCoin, value);
                return formatedText;
            }
            
            var addCoin = LocalizationData.AddCoin["en"]; 
            var localeCoin = string.Format(addCoin, value);
            return localeCoin;
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
        /// <param name="throwUnknown"></param>
        /// <returns></returns>
        public static string GetLocaleName(string id, bool throwUnknown = true)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return GetLocaleString(id, "en");
            }

            return throwUnknown ? "Unknown" : id;
        }

        /// <summary>
        /// Get localization by id and custom locale lang
        /// </summary>
        /// <param name="id"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        public static string GetLocaleString(string id, string locale)
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

        
        /// <summary>
        /// Returns the localized error message dictionary for the specified <see cref="ErrorType"/>.
        /// </summary>
        /// <param name="errorType">The type of error to retrieve localization for.</param>
        /// <returns>
        /// A dictionary mapping locale keys to localized error message strings.
        /// </returns>
        /// <remarks>
        /// The returned dictionary comes from <see cref="LocalizationData"/> and contains localized
        /// strings for different languages keyed by locale codes.
        /// </remarks>
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
                ErrorType.API_BANNED => LocalizationData.Error_API_BANNED,
                ErrorType.API_TOO_MANY_REQUESTS => LocalizationData.Error_API_TOO_MUCH_REQUESTS,
                _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null)
            };
        }
        
        public static string GetCorrectedNickname(GInterface214 profileData)
        {
            return profileData.Side == EPlayerSide.Savage ? Transliterate(profileData.Nickname) : profileData.Nickname;
        }

        /// <summary>
        /// Transliteration of Cyrillic
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string Transliterate(string text)
        {
            return GClass953.Dictionary_0.Aggregate(text, (current, key) => current.Replace(key.Key, key.Value));
        }

        /// <summary>
        /// Request eng lcoalization for non-eng users
        /// </summary>
        public async Task LoadEnglishLocaleAsync()
        {
            try
            {
                LeaderboardPlugin.logger.LogInfo("Request to load english locale");
                var session = PlayerHelper.GetSession();
                Dictionary<string, string> result = await session.GetLocalization("en");
                LocaleManagerClass.LocaleManagerClass.UpdateLocales("en", result);
            }
            catch (Exception e)
            {
                LeaderboardPlugin.logger.LogError($"Fail Request load english locale: {e}");
            }
        }

        
        /// <summary>
        /// Displays a message notification to the player.
        /// </summary>
        /// <param name="text">The message to display.</param>
        /// <param name="durationType">
        /// The duration for which the notification should be displayed.  
        /// Defaults to <see cref="ENotificationDurationType.Default"/>.
        /// </param>
        /// <remarks>
        /// Internally calls <see cref="NotificationManagerClass.DisplayMessageNotification"/> to show the message.
        /// </remarks>
        public static void Notification(string text, ENotificationDurationType durationType = ENotificationDurationType.Default)
        {
            NotificationManagerClass.DisplayMessageNotification(
                text, 
                durationType);
        }
        
        /// <summary>
        /// Displays a warning notification to the player.
        /// </summary>
        /// <param name="text">The warning message to display.</param>
        /// <param name="durationType">
        /// The duration for which the notification should be displayed.  
        /// Defaults to <see cref="ENotificationDurationType.Long"/>.
        /// </param>
        /// <remarks>
        /// Internally calls <see cref="NotificationManagerClass.DisplayWarningNotification"/> to show the message.
        /// </remarks>
        public static void NotificationWarning(string text, ENotificationDurationType durationType = ENotificationDurationType.Long)
        {
            NotificationManagerClass.DisplayWarningNotification(
                text, 
                durationType);
        }
    }
}