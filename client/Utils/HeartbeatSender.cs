using System;
using Comfort.Common;
using EFT.UI;
using Newtonsoft.Json;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Utils
{
    public static class HeartbeatSender
    {
        private static DateTime _lastSendTime = DateTime.MinValue;
        private static PlayerState _lastSentState;

        public static void Send(PlayerState playerState)
        {
            if (SettingsModel.Instance.PublicProfile.Value)
            {
                if (Singleton<PreloaderUI>.Instantiated)
                {
                    var now = DateTime.UtcNow;
                    var timeSinceLastSend = (now - _lastSendTime).TotalSeconds;

                    if (timeSinceLastSend < GlobalData.HeartbeatCooldownSeconds && _lastSentState.Equals(playerState))
                    {
                        return;
                    }

                    var session = PlayerHelper.GetSession();
                    if (session?.Profile == null)
                        return;

                    var request = NetworkApiRequestModel.Create(GlobalData.HeartbeatUrl);

                    request.OnSuccess = (response, code) =>
                    {
                        LeaderboardPlugin.logger.LogWarning($"Request OnSuccess {response}");
                    };

                    request.OnFail = (error, code) => { ServerErrorHandler.HandleError(error, code); };

                    var data = new PlayerHeartbeatData
                    {
                        Type = DataUtils.GetPlayerState(playerState),
                        Timestamp = DataUtils.CurrentTimestamp,
                        Version = GlobalData.Version,
                        SessionId = session.Profile.Id,
                        Token = EncryptionModel.Instance.Token
                    };

                    string jsonBody = JsonConvert.SerializeObject(data);

#if DEBUG
                    if (SettingsModel.Instance.Debug.Value)
                    {
                        LeaderboardPlugin.logger.LogWarning($"Request Data {jsonBody}");
                    }
#endif

                    request.SetData(jsonBody);
                    request.Send();

                    // Кэшируем отправку
                    _lastSendTime = now;
                    _lastSentState = playerState;
                }
            }
        }
        
        public static void SendInRaid(PlayerState playerState = PlayerState.IN_RAID)
        {
            if (SettingsModel.Instance.PublicProfile.Value)
            {
                if (Singleton<PreloaderUI>.Instantiated)
                {

                    var session = PlayerHelper.GetSession();
                    if (session?.Profile == null)
                        return;

                    var request = NetworkApiRequestModel.Create(GlobalData.HeartbeatUrl);

                    request.OnSuccess = (response, code) =>
                    {
                        LeaderboardPlugin.logger.LogWarning($"Request OnSuccess {response}");
                    };

                    request.OnFail = (error, code) => { ServerErrorHandler.HandleError(error, code); };

                    var data = new PlayerHeartbeatRaidData
                    {
                        Type = DataUtils.GetPlayerState(playerState),
                        Timestamp = DataUtils.CurrentTimestamp,
                        Version = GlobalData.Version,
                        SessionId = session.Profile.Id,
                        Map = DataUtils.GetRaidRawMap(),
                        Side = DataUtils.GetRaidPlayerSide(),
                        GameTime = DataUtils.GetRaidGameTime(),
                        Token = EncryptionModel.Instance.Token
                    };

                    string jsonBody = JsonConvert.SerializeObject(data);

#if DEBUG
                    if (SettingsModel.Instance.Debug.Value)
                    {
                        LeaderboardPlugin.logger.LogWarning($"Request Data {jsonBody}");
                    }
#endif

                    request.SetData(jsonBody);
                    request.Send();

                    _lastSentState = playerState;
                }
            }
        }
    }
}
