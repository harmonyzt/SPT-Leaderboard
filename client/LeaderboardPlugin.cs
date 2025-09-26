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
        public bool configUpdated = false;

        public static ManualLogSource logger;

        private void Awake()
        {
            logger = Logger;
            logger.LogInfo("Loading...");
            
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
            new OnEnemyDamagePatch().Enable();
            new PlayerOnDeadPatch().Enable();
            
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
            // Enable patches for overlay with hits
            new OnGameWorldStartPatch().Enable();
            new OnGameWorldDisposePatch().Enable();
#endif
            
            Instance = this;
            logger.LogInfo("Successful loaded!");
        }
        
        #region Icons
        
        /// <summary>
        /// Capture icon preview with PMC full body
        /// </summary>
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
        
        /// <summary>
        /// Caching player model view for future actions
        /// </summary>
        public void CacheFullBodyPlayerModelView()
        {
            if (!_iconSaver)
            {
                _iconSaver = gameObject.AddComponent<IconSaver>();
            }

            _iconSaver.CachePlayerModelView();
        }

        /// <summary>
        /// Capture icon preview only face 
        /// </summary>
        public void CreateIconPlayer()
        {
            if (!_iconSaver)
            {
                _iconSaver = gameObject.AddComponent<IconSaver>();
            }
            
            _iconSaver.CreateIcon();
        }
        
        #endregion

        #region Network
        
        /// <summary>
        /// Sends the player's profile image to the server.
        /// </summary>
        /// <param name="texture">The texture containing the profile image to send.</param>
        /// <param name="isFullBody">
        /// Indicates whether the image is a full-body picture (<c>true</c>) or just an avatar/icon (<c>false</c>).
        /// </param>
        /// <remarks>
        /// The method encodes <paramref name="texture"/> to PNG, then to Base64, creates a JSON payload with 
        /// the player's ID and image type, and sends it to the server at <see cref="GlobalData.IconUrl"/>.
        /// On success, the <c>OnSuccess</c> callback is triggered; on failure, the <c>OnFail</c> callback is triggered.
        /// </remarks>
        public static void SendProfileIcon(Texture2D texture, bool isFullBody)
        {
            var request = NetworkApiRequestModel.Create(GlobalData.IconUrl);
            var session = PlayerHelper.GetSession();
            request.OnSuccess = (response, code) =>
            {
                logger.LogInfo($"Request OnSuccess {response}");
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
                IsFullBody = isFullBody,
                Token = EncryptionModel.Instance.Token
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
        
        /// <summary>
        /// Sends the raid and profile data to the server.
        /// </summary>
        /// <param name="data">An object containing the profile and raid data to be serialized and sent.</param>
        /// <remarks>
        /// The method serializes <paramref name="data"/> to JSON and sends it to the server at 
        /// <see cref="GlobalData.ProfileUrl"/>.
        /// On success, the <c>OnSuccess</c> callback is triggered; on failure, the <c>OnFail</c> callback is triggered.
        /// </remarks>
        public static void SendRaidData(object data)
        {
            var request = NetworkApiRequestModel.Create(GlobalData.ProfileUrl);

            request.OnSuccess = (response, code) =>
            {
                logger.LogInfo($"Request OnSuccess {response}");
                if (SettingsModel.Instance.ShowPointsNotification.Value)
                {
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
        
        /// <summary>
        /// Sends pre-raid data to the server.
        /// </summary>
        /// <param name="data">
        /// An object containing the pre-raid information to be serialized and sent, typically 
        /// an instance of <see cref="PreRaidData"/>.
        /// </param>
        /// <remarks>
        /// The method serializes <paramref name="data"/> to JSON and sends it to the server at 
        /// <see cref="GlobalData.PreRaidUrl"/>.
        /// On success, the <c>OnSuccess</c> callback is triggered; on failure, the <c>OnFail</c> callback is triggered.
        /// </remarks>
        public static void SendPreRaidData(object data)
        {
            var request = NetworkApiRequestModel.Create(GlobalData.PreRaidUrl);

            request.OnSuccess = (response, code) =>
            {
                logger.LogInfo($"Request OnSuccess {response}");
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
        
        /// <summary>
        /// Start the timer to update the heartbeat during the raid
        /// </summary>
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

        /// <summary>
        /// Stop the timer to update the heartbeat during the raid
        /// </summary>
        public void StopInRaidHeartbeat()
        {
            if (_inRaidHeartbeatTimer == null) return;
            
            _inRaidHeartbeatTimer.Stop();
            _inRaidHeartbeatTimer.Dispose();
            _inRaidHeartbeatTimer = null;
        }

        /// <summary>
        /// Start a delay for the preRaid check.
        /// </summary>
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
        
        /// <summary>
        /// Disable the delay for preRaid check
        /// </summary>
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
