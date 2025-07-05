using System;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SPT.Reflection.Utils;
using SPTLeaderboard.Models;
using UnityEngine;

namespace SPTLeaderboard
{
    [BepInPlugin("harmonyzt.SPTLeaderboard", "SPTLeaderboard", "1.0.0")]
    public class LeaderboardPlugin : BaseUnityPlugin
    {
        private SettingsModel _settings;
        private LocalizationModel _localization;

        public static ManualLogSource logger;
        
        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();

        public static bool HasRaidStarted()
        {
            bool? inRaid = Singleton<AbstractGame>.Instance?.InRaid;
            return inRaid.HasValue && inRaid.Value;
        }

        private void Awake()
        {
            _settings = SettingsModel.Create(Config);
            _localization = LocalizationModel.Create();
            
            logger = Logger;
            logger.LogInfo("[SPT Leaderboard] successful loaded!");
        }

        private void Update()
        {
            if (_settings.KeyBind.Value.IsDown())
            {
                TestMethod();
            }
        }

        private void TestMethod()
        {
            var request = NetworkApiRequestModel.Create("https://visuals.nullcore.net/SPT/testEnv/api/heartbeat/v1.php");

            request.OnSuccess = (response, code) =>
            {
                logger.LogWarning($"[SPT Leaderboard] Request OnSuccess {response}:{code}");
            };

            request.OnFail = (error, code) =>
            {
                logger.LogError($"[SPT Leaderboard] Request OnFail {error}:{code}");
            };
                
            request.SetData("""{"type":"online","timestamp":1751658790369,"ver":"2.6.0","sessionId":"6862c9040004a645b8febe48"}""");
            request.Start();
        }
        
        public string GetPlayerState(PlayerState state)
        {
            return Enum.GetName(typeof(PlayerState), state)?.ToLower();
        }
    }

    public enum PlayerState
    {
        ONLINE,
        IN_MENU,
        IN_RAID,
        IN_STASH
    }
}
