using System;
using EFT.Communications;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Utils
{
    public static class ServerErrorHandler
    {
        public static void HandleError(string responseBody, long statusCode)
        {
            LeaderboardPlugin.logger.LogError($"Server Error: {statusCode} | Response: {responseBody}");

            var typeError = GetTypeError(statusCode);
            if (typeError != ErrorType.SILENT_ERROR)
            {
                NotificationManagerClass.DisplayWarningNotification(LocalizationModel.Instance.GetLocaleErrorText(typeError),
                    GetDurationType(typeError));
            }
        }

        public static ErrorType GetTypeError(long errorCode)
        {
            return errorCode switch
            {
                699 => ErrorType.VIOLATION_LA_TOS,
                700 => ErrorType.TOKEN_MISMATCH,
                701 => ErrorType.SILENT_ERROR,
                702 => ErrorType.TOKEN_NOT_SAFE,
                703 => ErrorType.SILENT_ERROR,
                705 => ErrorType.UPDATE_MOD,
                231 => ErrorType.SCAV_ONLY_PUBLIC,
                232 => ErrorType.CHAR_LIMIT,
                707 => ErrorType.NSFW_NAME,
                _ => ErrorType.SILENT_ERROR
            };
        }
        
        public static ENotificationDurationType GetDurationType(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.VIOLATION_LA_TOS => ENotificationDurationType.Long,
                ErrorType.TOKEN_MISMATCH => ENotificationDurationType.Long,
                ErrorType.TOKEN_NOT_SAFE => ENotificationDurationType.Long,
                ErrorType.UPDATE_MOD => ENotificationDurationType.Long,
                ErrorType.SCAV_ONLY_PUBLIC => ENotificationDurationType.Default,
                ErrorType.CHAR_LIMIT => ENotificationDurationType.Long,
                ErrorType.NSFW_NAME => ENotificationDurationType.Long,
                ErrorType.BALACLAVA => ENotificationDurationType.Long,
                _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null)
            };
        }
    }

    public enum ErrorType
    {
        VIOLATION_LA_TOS,
        TOKEN_MISMATCH,
        TOKEN_NOT_SAFE,
        UPDATE_MOD,
        SCAV_ONLY_PUBLIC,
        CHAR_LIMIT,
        NSFW_NAME,
        SILENT_ERROR,
        BALACLAVA
    }
}