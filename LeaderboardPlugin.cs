using System;
using System.Collections;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI;
using Newtonsoft.Json;
using SPT.Common.Http;
using SPT.Common.Utils;
using SPT.Custom.Models;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using UnityEngine;

namespace SPTLeaderboard
{
    [BepInPlugin("harmonyzt.SPTLeaderboard", "SPTLeaderboard", "1.0.0")]
    public class LeaderboardPlugin : BaseUnityPlugin
    {
        private SettingsModel _settings;
        private LocalizationModel _localization;
        private string _sessionID;
        private string _cachedVersion;
        private bool isWork = false;

        public static ManualLogSource logger;
        
        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        
        public static ISession GetSession(bool throwIfNull = false)
        {
            var session = ClientAppUtils.GetClientApp().Session;

            if (throwIfNull && session is null)
            {
                logger.LogWarning("Trying to access the Session when it's null");
            }

            return session;
        }
        
        public static Profile GetProfile(bool throwIfNull = false)
        {
            var profile = GetSession()?.Profile;

            if (throwIfNull && profile is null)
            {
                logger.LogWarning("Trying to access the Profile when it's null");
            }
        
            return GetSession()?.Profile;
        }
        
        public static long CurrentTimestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static bool HasRaidStarted()
        {
            bool? inRaid = Singleton<AbstractGame>.Instance?.InRaid;
            return inRaid.HasValue && inRaid.Value;
        }

        private void Awake()
        {
            _sessionID = Guid.NewGuid().ToString("N");
            _settings = SettingsModel.Create(Config);
            _localization = LocalizationModel.Create();
            
            logger = Logger;
            logger.LogInfo("[SPT Leaderboard] successful loaded!");
        }

        private void Update()
        {
            if (_settings.KeyBind.Value.IsDown())
            {
                SendHeartbeat(PlayerState.ONLINE);
            }
            
            if (_settings.KeyBindTwo.Value.IsDown())
            {
                if (Singleton<PreloaderUI>.Instantiated)
                {
                    var session = GetSession();
                    if (session.Profile != null)
                    {
                        var exampleData = new BaseData
                        {
                            AccountType = "edge_of_darkness",
                            Health = 440,
                            Id = session.Profile.Id,
                            IsScav = false,
                            LastPlayed = CurrentTimestamp,
                            ModInt = "fb75631b7a153b1b95cdaa7dfdc297b4a7c40f105584561f78e5353e7e925c6f",
                            Mods = ["IhanaMies-LootValueBackend", "SpecialSlots"],
                            Name = session.Profile.Nickname,
                            PmcHealth = 440,
                            PmcLevel = session.Profile.Info.Level,
                            RaidKills = 3,
                            RaidResult = "Survived",
                            RaidTime = 221,
                            SptVersion = ParseVersion(PlayerPrefs.GetString("SPT_Version")),
                            Token = "20eb4274c9b66efca21d622d45680aeedcc19762e7d7b898f9cf0bf88c9e4518",
                            DBinInv = false,
                            IsCasual = false
                        };
                
                        SendProfileData(exampleData);
                    }
                }
            }
        }

        public string ParseVersion(string rawString)
        {
            var match = Regex.Match(rawString, @"SPT\s+([0-9\.]+)\s+-");
            if (match.Success)
            {
                string version = match.Groups[1].Value;
                return version;
            }

            return GlobalData.BaseSPTVersion;
        }
        
        private void SendHeartbeat(PlayerState playerState)
        {
            var request = NetworkApiRequestModel.Create(GlobalData.HeartbeatUrl);

            request.OnSuccess = (response, code) =>
            {
                logger.LogWarning($"[SPT Leaderboard] Request OnSuccess {response}:{code}");
            };

            request.OnFail = (error, code) =>
            {
                logger.LogError($"[SPT Leaderboard] Request OnFail {error}:{code}");
            };
            
            var data = new PlayerHeartbeatData
            {
                Type = GetPlayerState(playerState),
                Timestamp = CurrentTimestamp,
                Version = GlobalData.Version,
                SessionId = _sessionID
            };

            string jsonBody = JsonConvert.SerializeObject(data);
            logger.LogWarning($"[SPT Leaderboard] Request Data {jsonBody}");
            
            request.SetData(jsonBody);
            request.Start();
        }
        
        private void SendProfileData(BaseData data)
        {
            var request = NetworkApiRequestModel.Create(GlobalData.ProfileUrl);

            request.OnSuccess = (response, code) =>
            {
                logger.LogWarning($"[SPT Leaderboard] Request OnSuccess {response}:{code}");
            };

            request.OnFail = (error, code) =>
            {
                logger.LogError($"[SPT Leaderboard] Request OnFail {error}:{code}");
            };

            string jsonBody = JsonConvert.SerializeObject(data);
            logger.LogWarning($"[SPT Leaderboard] Request Data {jsonBody}");
            
            request.SetData(jsonBody);
            request.Start();
        }
        
        public string GetPlayerState(PlayerState state)
        {
            return Enum.GetName(typeof(PlayerState), state)?.ToLower();
        }
    }
}
