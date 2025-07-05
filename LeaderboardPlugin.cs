using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT.UI;
using Newtonsoft.Json;
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
        private EncryptionModel _encrypt;
        
        public static string _sessionID;

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
        }

        private void Update()
        {
            if (_settings.KeyBind.Value.IsDown())
            {
                SendHeartbeat(PlayerState.ONLINE);
            }
            
            if (_settings.KeyBindTwo.Value.IsDown())
            {
                logger.LogWarning($"Токен {_encrypt.Token}");
                // SendExampleProfileData();
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
                        Token = _encrypt.Token,
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
