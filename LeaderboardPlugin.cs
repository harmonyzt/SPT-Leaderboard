using System.Timers;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Newtonsoft.Json;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Patches;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard
{
    [BepInPlugin("harmonyzt.SPTLeaderboard", "SPTLeaderboard", "1.0.0")]
    public class LeaderboardPlugin : BaseUnityPlugin
    {
        public static LeaderboardPlugin Instance { get; private set; }
        
        private SettingsModel _settings;
        private EncryptionModel _encrypt;
        private Timer _inRaidHeartbeatTimer;

        public static ManualLogSource logger;

        private void Awake()
        {
            logger = Logger;
            logger.LogInfo("[SPT Leaderboard] successful loaded!");
            
            
            _settings = SettingsModel.Create(Config);
            _encrypt = EncryptionModel.Create();
            
            new OpenMainMenuScreenPatch().Enable();
            new OpenInventoryScreenPatch().Enable();
            new OnStartRaidPatch().Enable();
            new OnEndRaidPatch().Enable();
            new HideoutAwakePatch().Enable();
            
            Instance = this;
        }

        private void Update()
        {
            if (_settings.KeyBind.Value.IsDown())
            {
                
            }
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
                            logger.LogWarning($"Request OnSuccess {response}:{code}");
                        };

                        request.OnFail = (error, code) =>
                        {
                            logger.LogError($"Request OnFail {error}:{code}");
                        };

                        var data = new PlayerHeartbeatData
                        {
                            Type = DataUtils.GetPlayerState(playerState),
                            Timestamp = DataUtils.CurrentTimestamp,
                            Version = GlobalData.Version,
                            SessionId = session.Profile.Id
                        };

                        string jsonBody = JsonConvert.SerializeObject(data);
                        logger.LogWarning($"Request Data {jsonBody}");

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
                logger.LogWarning($"Request OnSuccess {response}:{code}");
            };

            request.OnFail = (error, code) =>
            {
                logger.LogError($"Request OnFail {error}:{code}");
            };

            string jsonBody = JsonConvert.SerializeObject(data);
            logger.LogWarning($"Request Data {jsonBody}");
            
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
        
            logger.LogWarning("InRaid timer started");
        }

        public void StopInRaidHeartbeat()
        {
            if (_inRaidHeartbeatTimer != null)
            {
                _inRaidHeartbeatTimer.Stop();
                _inRaidHeartbeatTimer.Dispose();
                _inRaidHeartbeatTimer = null;
            
                logger.LogWarning("InRaid timer stopped");
            }
        }
    }
}
