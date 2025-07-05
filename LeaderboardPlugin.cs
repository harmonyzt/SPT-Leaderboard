using System;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json.Linq;
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
        private NetworkApiRequestModel _network;

        public static ManualLogSource ManualLogger;
        
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
            _network = NetworkApiRequestModel.Create();
            
            ManualLogger = Logger;
            ManualLogger.LogInfo("[SPT Leaderboard] successful loaded!");
        }

        private void Update()
        {
            if (_settings.KeyBind.Value.IsDown())
            {
                StartCoroutine(NetworkApiRequestModel.Instance.SendPostRequest());
            }
        }
    }
}
