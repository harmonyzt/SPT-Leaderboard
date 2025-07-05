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
using SPTLeaderboard.Utils;
using UnityEngine;

namespace SPTLeaderboard
{
    [BepInPlugin("harmonyzt.SPTLeaderboard", "SPTLeaderboard", "1.0.0")]
    public class LeaderboardPlugin : BaseUnityPlugin
    {
        private SettingsModel _settings;
        public static string _sessionID;

        public static ManualLogSource logger;
        
        public static Profile GetProfile(bool throwIfNull = false)
        {
            var profile = DataUtils.GetSession()?.Profile;

            if (throwIfNull && profile is null)
            {
                logger.LogWarning("Trying to access the Profile when it's null");
            }
        
            return DataUtils.GetSession()?.Profile;
        }

        private void Awake()
        {
            _settings = SettingsModel.Create(Config);
            
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
                var session = DataUtils.GetSession();
                if (session.Profile != null)
                {
                    var exampleData = new BaseData
                    {
                        AccountType = session.Profile.Info.GameVersion,
                        Health = 440,
                        Id = session.Profile.Id,
                        IsScav = false,
                        LastPlayed = DataUtils.CurrentTimestamp,
                        ModInt = "fb75631b7a153b1b95cdaa7dfdc297b4a7c40f105584561f78e5353e7e925c6f",
                        Mods = ["IhanaMies-LootValueBackend", "SpecialSlots"],
                        Name = session.Profile.Nickname,
                        PmcHealth = 440,
                        PmcLevel = session.Profile.Info.Level,
                        RaidKills = 3,
                        RaidResult = "Survived",
                        RaidTime = 621,
                        SptVersion = DataUtils.ParseVersion(PlayerPrefs.GetString("SPT_Version")),
                        Token = "20eb4274c9b66efca21d622d45680aeedcc19762e7d7b898f9cf0bf88c9e4518",
                        DBinInv = false,
                        IsCasual = _settings.ModCasualMode.Value
                    };
                
                    SendProfileData(exampleData);
                }
            }
        }
        
        public static void SendHeartbeat(PlayerState playerState)
        {
            if (Singleton<PreloaderUI>.Instantiated)
            {
                var session = DataUtils.GetSession();
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
                        Type = DataUtils.GetPlayerState(playerState),
                        Timestamp = DataUtils.CurrentTimestamp,
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
    }
}
