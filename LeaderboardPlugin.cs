using System;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI;
using Newtonsoft.Json;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;
using SPTLeaderboard.Enums;
using SPTLeaderboard.Models;
using SPTLeaderboard.Patches;
using UnityEngine;

namespace SPTLeaderboard
{
    [BepInPlugin("harmonyzt.SPTLeaderboard", "SPTLeaderboard", "1.0.0")]
    public class LeaderboardPlugin : BaseUnityPlugin
    {
        private SettingsModel _settings;
        private LocalizationModel _localization;
        public static string _sessionID;

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
            _settings = SettingsModel.Create(Config);
            _localization = LocalizationModel.Create();
            
            new OpenMainMenuScreenPatch().Enable();
            new OpenInventoryScreenPatch().Enable();
            new OnStartRaidPatch().Enable();
            new OnEndRaidPatch().Enable();
            
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
                SendExampleProfileData();
            }
        }

        public void SendExampleProfileData()
        {
            if (Singleton<PreloaderUI>.Instantiated)
            {
                var session = GetSession();
                if (session.Profile != null)
                {
                    var exampleData = new BaseData
                    {
                        AccountType = session.Profile.Info.GameVersion,
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
                        RaidTime = 621,
                        SptVersion = ParseVersion(PlayerPrefs.GetString("SPT_Version")),
                        Token = "",
                        DBinInv = false,
                        IsCasual = _settings.ModCasualMode.Value
                    };
                
                    SendProfileData(exampleData);
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
        
        public static void SendHeartbeat(PlayerState playerState)
        {
            if (Singleton<PreloaderUI>.Instantiated)
            {
                var session = GetSession();
                if (session.Profile != null)
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
                        SessionId = session.Profile.Id
                    };

                    string jsonBody = JsonConvert.SerializeObject(data);
                    logger.LogWarning($"[SPT Leaderboard] Request Data {jsonBody}");

                    request.SetData(jsonBody);
                    request.Start();
                }
            }
        }
        
        public void SendProfileData(BaseData data)
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
        
        public static string GetPlayerState(PlayerState state)
        {
            return Enum.GetName(typeof(PlayerState), state)?.ToLower();
        }
    }
}
