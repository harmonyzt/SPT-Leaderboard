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
    [BepInPlugin("harmonyzt.SPTLeaderboard", "SPTLeaderboard", "4.0.0")]
    public class LeaderboardPlugin : BaseUnityPlugin
    {
        public static LeaderboardPlugin Instance { get; private set; }
        
        private SettingsModel _settings;
        private LocalizationModel _localization;
        private EncryptionModel _encrypt;
        private IconSaver _iconSaver;
        
        private Timer _inRaidHeartbeatTimer;
        private Timer _preRaidCheckTimer;
        
        public bool canPreRaidCheck = true;
        public bool cachedPlayerModelPreview = false;
        public bool engLocaleLoaded = false;

        public static ManualLogSource logger;

        private void Awake()
        {
            logger = Logger;
            logger.LogInfo("[SPT Leaderboard] Loading...");
            
            _settings = SettingsModel.Create(Config);
            _encrypt = EncryptionModel.Create();
            _localization = LocalizationModel.Create();
            
            new LeaderboardVersionLabelPatch().Enable();
            new OpenMainMenuScreenPatch().Enable();
            new OpenInventoryScreenPatch().Enable();
            new OpenSelectSideScreenPatch().Enable();
            new OpenLoadingRaidScreenPatch().Enable();
            new OnStartRaidPatch().Enable();
            new OnEndRaidPatch().Enable();
            new HideoutAwakePatch().Enable();
            new OnApplyDamageInfoPatch().Enable();
            new OnInitPlayerPatch().Enable();
            
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

        #region Icons
        
        public void CreateIconFullBodyPlayer()
        {
            if (!_iconSaver)
            {
                _iconSaver = gameObject.AddComponent<IconSaver>();
            }

            if (!_iconSaver.clonePlayerModelViewObj)
            {
                _iconSaver.clonePlayerModelViewObj = _iconSaver.CreateClonedPlayerModelView();
                _iconSaver.HidePlayerModelExtraElements();
            }
                
            _iconSaver.CreateFullBodyIcon();
        }
        
        public void CacheFullBodyPlayerModelView()
        {
            if (!_iconSaver)
            {
                _iconSaver = gameObject.AddComponent<IconSaver>();
            }

            _iconSaver.CachePlayerModelView();
        }

        public void CreateIconPlayer()
        {
#if BETA || DEBUG
            logger.LogWarning("Start create icon");
#endif
            if (!_iconSaver)
            {
                _iconSaver = gameObject.AddComponent<IconSaver>();
            }
            
            _iconSaver.CreateIcon();
        }
        
        #endregion

        #region Network
        
        public static void SendProfileIcon(Texture2D texture, bool isFullBody)
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
                    
            byte[] imageData = texture.EncodeToPNG();
            var encodedImage = Convert.ToBase64String(imageData);
            var data = new ImageData
            {
                EncodedImage = encodedImage,
                PlayerId = session.Profile.Id,
                IsFullBody = isFullBody
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
                logger.LogInfo($"Request OnSuccess {response}");
                try
                {
                    var responseData =  JsonConvert.DeserializeObject<ResponseRaidData>(response.ToString());

                    if (responseData.Response == "success")
                    {
                        if (responseData.AddedToBalance > 0)
                        {
                            LocalizationModel.Notification(LocalizationModel.Instance.GetLocaleCoin(responseData.AddedToBalance));
                        }
                    }
                }
                catch (Exception ex)
                {
                   //
                }
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
        
        #endregion

        #region Timers
        
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
        
        #endregion
    }
}
