using System;
using System.Timers;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Patches;
using SPTLeaderboard.Utils;
using UnityEngine;

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
        private Timer _preRaidCheckTimer;
        public bool canPreRaidCheck = true;

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
            new OnApplyDamageInfoPatch().Enable();
            new OnInitPlayerPatch().Enable();
            new OpenSelectSideScreenPatch().Enable();
            
            if (!DataUtils.IsLoaded)
            {
                DataUtils.Load(callback=>
                {
                    if (!callback) return;
                    
                    new OnCoopApplyShotFourPatch().Enable();
                    logger.LogInfo("FIKA is found. Enable patch for hit hook");
                });
            }
#if DEBUG
            new OnGameWorldStartPatch().Enable();
            new OnGameWorldDisposePatch().Enable();
#endif
            Instance = this;
            logger.LogInfo("[SPT Leaderboard] successful loaded!");
        }


        private void Update()
        {
            if (_settings.KeyBind.Value.IsDown())
            {
                PlayerHelper.GetEquipmentData();
            }
        }

        public static void SendProfileIcon(GClass907 presetIcon)
        {
            var request = NetworkApiRequestModel.Create(GlobalData.IconUrl);
            var session = PlayerHelper.GetSession();
            request.OnSuccess = (response, code) =>
            {
                logger.LogWarning($"Request OnSuccess {response}");
            };

            request.OnFail = (error, code) =>
            {
                ServerErrorHandler.HandleError(error, code);
            };
                    
            byte[] imageData = presetIcon.Sprite.texture.EncodeToPNG();
            var encodedImage = Convert.ToBase64String(imageData);
            var data = new ImageData
            {
                EncodedImage = encodedImage,
                PlayerId = session.Profile.Id
            };
            string jsonBody = JsonConvert.SerializeObject(data);
                    
#if DEBUG
            if (SettingsModel.Instance.Debug.Value)
            {
                logger.LogWarning($"Request Image Data {jsonBody}");
            }
#endif
                    
            request.SetData(jsonBody);
            request.Send();
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
                ServerErrorHandler.HandleError(error, code);
            };

            string jsonBody = JsonConvert.SerializeObject(data);
            
#if DEBUG
            if (SettingsModel.Instance.Debug.Value)
            {
                logger.LogWarning($"Request Data {jsonBody}");
            }
#endif
            
            request.SetData(jsonBody);
            request.Send();
        }
        
        public static void SendPreRaidData(object data)
        {
            var request = NetworkApiRequestModel.Create(GlobalData.PreRaidUrl);

            request.OnSuccess = (response, code) =>
            {
                logger.LogWarning($"Request OnSuccess {response}");
            };

            request.OnFail = (error, code) =>
            {
                ServerErrorHandler.HandleError(error, code);
            };

            string jsonBody = JsonConvert.SerializeObject(data);
            
#if DEBUG
            if (SettingsModel.Instance.Debug.Value)
            {
                logger.LogWarning($"Request Data {jsonBody}");
            }
#endif
            
            request.SetData(jsonBody);
            request.Send();
        }
        
        public void StartInRaidHeartbeat()
        {
            StopInRaidHeartbeat();
            HeartbeatSender.Send(PlayerState.IN_RAID);
        
            _inRaidHeartbeatTimer = new Timer(_settings.SupportInRaidConnectionTimer.Value * 1000);
            _inRaidHeartbeatTimer.Elapsed += (_, __) =>
            {
                if (PlayerHelper.HasRaidStarted())
                {
                    HeartbeatSender.SendInRaid();
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

        public void StartPreRaidCheckTimer()
        {
            StopPreRaidCheckTimer();
            
            canPreRaidCheck = false;
            _preRaidCheckTimer = new Timer(10 * 60 * 1000);
            _preRaidCheckTimer.Elapsed += (sender, args) =>
            {
                canPreRaidCheck = true;
                _preRaidCheckTimer.Stop();
                _preRaidCheckTimer.Dispose();
                _preRaidCheckTimer = null;
                LeaderboardPlugin.logger.LogWarning("Таймер PreRaidData завершился, можно отправлять снова");
            };
            _preRaidCheckTimer.AutoReset = false;
            _preRaidCheckTimer.Start();
        }
        
        public void StopPreRaidCheckTimer()
        {
            if (_preRaidCheckTimer == null) return;
            
            _preRaidCheckTimer.Stop();
            _preRaidCheckTimer.Dispose();
            _preRaidCheckTimer = null;
        }
    }
}
