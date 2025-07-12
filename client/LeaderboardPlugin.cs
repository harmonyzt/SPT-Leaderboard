using System.Timers;
using BepInEx;
using BepInEx.Logging;
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
            new HookEftBattleUIScreenPatch().Enable();
            new OnGameWorldStartPatch().Enable();
            new OnGameWorldDisposePatch().Enable();
#endif
            Instance = this;
            logger.LogInfo("[SPT Leaderboard] successful loaded!");
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
        
        public void StartInRaidHeartbeat()
        {
            StopInRaidHeartbeat();
            HeartbeatSender.Send(PlayerState.IN_RAID);
        
            _inRaidHeartbeatTimer = new Timer(_settings.SupportInRaidConnectionTimer.Value * 1000);
            _inRaidHeartbeatTimer.Elapsed += (_, __) =>
            {
                if (PlayerHelper.HasRaidStarted())
                {
                    HeartbeatSender.Send(PlayerState.IN_RAID);
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
