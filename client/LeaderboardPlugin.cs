using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT.UI;
using Newtonsoft.Json;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Patches;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard
{
    [BepInPlugin("harmonyzt.SPTLeaderboard", "SPTLeaderboard", "3.1.0")]
    public class LeaderboardPlugin : BaseUnityPlugin
    {
        public static LeaderboardPlugin Instance { get; private set; }
        
        private SettingsModel _settings;
        private LocalizationModel _localization;
        private EncryptionModel _encrypt;
        
        private Timer _inRaidHeartbeatTimer;

        public static ManualLogSource logger;

        private void Awake()
        {
            logger = Logger;
            logger.LogInfo("[SPT Leaderboard] Loading...");
            
            _settings = SettingsModel.Create(Config);
            _encrypt = EncryptionModel.Create();
            _localization = LocalizationModel.Create();
            
            new OpenMainMenuScreenPatch().Enable();
            new OpenInventoryScreenPatch().Enable();
            new OnStartRaidPatch().Enable();
            new OnEndRaidPatch().Enable();
            new HideoutAwakePatch().Enable();
            
            Instance = this;
            logger.LogInfo("[SPT Leaderboard] successful loaded!");
        }

        public static void SendHeartbeat(PlayerState playerState)
        {
            if (SettingsModel.Instance.PublicProfile.Value)
            {
                if (Singleton<PreloaderUI>.Instantiated)
                {
                    var session = DataUtils.GetSession();
                    if (session.Profile != null)
                    {
                        var request = NetworkApiRequestModel.Create(GlobalData.HeartbeatUrl);

                        request.OnSuccess = (response, code) =>
                        {
                            if (SettingsModel.Instance.Debug.Value)
                            {
                                logger.LogWarning($"Request OnSuccess {response}");
                            }
                        };

                        request.OnFail = (error, code) =>
                        {
                            if (SettingsModel.Instance.Debug.Value)
                            {
                                logger.LogError($"Request OnFail {error}");
                            }
                        };

                        var data = new PlayerHeartbeatData
                        {
                            Type = DataUtils.GetPlayerState(playerState),
                            Timestamp = DataUtils.CurrentTimestamp,
                            Version = GlobalData.Version,
                            SessionId = session.Profile.Id
                        };

                        string jsonBody = JsonConvert.SerializeObject(data);
                        if (SettingsModel.Instance.Debug.Value)
                        {
                            logger.LogWarning($"Request Data {jsonBody}");
                        }

                        request.SetData(jsonBody);
                        request.Send();
                    }
                }
            }
        }
        
        public static void SendProfileData(object data)
        {
            var request = NetworkApiRequestModel.Create(GlobalData.ProfileUrl);

            request.OnSuccess = (response, code) =>
            {
                logger.LogWarning($"Request OnSuccess {response}");
            };

            request.OnFail = (error, code) =>
            {
                logger.LogError($"Request OnFail {error}");
            };

            string jsonBody = JsonConvert.SerializeObject(data);

            if (SettingsModel.Instance.Debug.Value)
            {
                logger.LogWarning($"Request Data {jsonBody}");
            }

            request.SetData(jsonBody);
            request.Send();
        }
        
        public void StartInRaidHeartbeat()
        {
            StopInRaidHeartbeat();
            SendHeartbeat(PlayerState.IN_RAID);
        
            _inRaidHeartbeatTimer = new Timer(_settings.SupportInRaidConnectionTimer.Value * 1000);
            _inRaidHeartbeatTimer.Elapsed += (_, __) =>
            {
                if (DataUtils.HasRaidStarted())
                {
                    SendHeartbeat(PlayerState.IN_RAID);
                }
                else
                {
                    StopInRaidHeartbeat();
                }
            };
            _inRaidHeartbeatTimer.AutoReset = true;
            _inRaidHeartbeatTimer.Start();
        }

        public void StopInRaidHeartbeat()
        {
            if (_inRaidHeartbeatTimer == null) return;
            
            _inRaidHeartbeatTimer.Stop();
            _inRaidHeartbeatTimer.Dispose();
            _inRaidHeartbeatTimer = null;
        }
    }
}
