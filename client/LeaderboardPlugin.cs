using System;
using System.Timers;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
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
        private GClass907 presetIcon;

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


        private void Update()
        {
            if (_settings.KeyBind.Value.IsDown())
            {
                var profile = PlayerHelper.GetProfile();
                
                logger.LogWarning("1");
                presetIcon = Singleton<GClass905>.Instance.GetIcon(new GClass910(profile.Inventory.Equipment.CloneVisibleItem<InventoryEquipment>(), profile.Customization));
                logger.LogWarning("2");
                if (presetIcon.Sprite == null)
                {
                    presetIcon.Changed.Bind(ChangedBlyat);
                }
                else
                {
                    var saver = new SpriteSaver();
                    saver.SaveSpriteAsPNG(GlobalData.LeaderboardIconPath, presetIcon.Sprite);
                }
            }
        }

        private void ChangedBlyat()
        {
            bool flag = presetIcon.Sprite != null;
            logger.LogWarning($"Is loaded icon? {flag}");

            if (flag)
            {
                var saver = new SpriteSaver();
                saver.SaveSpriteAsPNG(GlobalData.LeaderboardIconPath, presetIcon.Sprite);
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
    }
}
