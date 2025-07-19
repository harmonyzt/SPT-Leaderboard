using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPTLeaderboard.Data
{
    public class AdditiveProfileData(BaseData baseData)
    {
        #region BaseData
        
        [JsonProperty("accountType")]
        public string AccountType { get; set; } = baseData.AccountType;

        [JsonProperty("health")]
        public float Health { get; set; } = baseData.Health;

        [JsonProperty("id")]
        public string Id { get; set; } = baseData.Id;

        [JsonProperty("isScav")]
        public bool IsScav { get; set; } = baseData.IsScav;

        [JsonProperty("lastPlayed")]
        public long LastPlayed { get; set; } = baseData.LastPlayed;

        [JsonProperty("modINT")]
        public string ModInt { get; set; } = baseData.ModInt;

        [JsonProperty("mods")]
        public List<string> Mods { get; set; } = baseData.Mods;

        [JsonProperty("name")]
        public string Name { get; set; } = baseData.Name;

        [JsonProperty("pmcHealth")]
        public float PmcHealth { get; set; } = baseData.PmcHealth;

        [JsonProperty("pmcLevel")]
        public int PmcLevel { get; set; } = baseData.PmcLevel;

        [JsonProperty("raidKills")]
        public int RaidKills { get; set; } = baseData.RaidKills;

        [JsonProperty("raidResult")]
        public string RaidResult { get; set; } = baseData.RaidResult;

        [JsonProperty("raidTime")]
        public float RaidTime { get; set; } = baseData.RaidTime;

        [JsonProperty("sptVer")]
        public string SptVersion { get; set; } = baseData.SptVersion;

        // [JsonProperty("teamTag")]
        // public string TeamTag { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; } = baseData.Token;

        [JsonProperty("DBinINV")]
        public bool DBinInv { get; set; } = baseData.DBinInv;

        [JsonProperty("isCasual")]
        public bool IsCasual { get; set; } = baseData.IsCasual;
        
        #endregion
        
        [JsonProperty("discFromRaid")]
        public bool DiscFromRaid { get; set; }
        
        [JsonProperty("isTransition")]
        public bool IsTransition { get; set; }
        
        [JsonProperty("isUsingStattrack")]
        public bool IsUsingStattrack { get; set; }
        
        [JsonProperty("lastRaidEXP")]
        public int LastRaidEXP { get; set; }
        
        [JsonProperty("lastRaidHits")]
        public int LastRaidHits { get; set; }
        
        [JsonProperty("lastRaidMap")]
        public string LastRaidMap { get; set; }
        
        [JsonProperty("lastRaidMapRaw")]
        public string LastRaidMapRaw { get; set; }
        
        [JsonProperty("lastRaidTransitionTo")]
        public string LastRaidTransitionTo { get; set; }
        
        [JsonProperty("raidHits")]
        public HitsData RaidHits { get; set; }
        
        [JsonProperty("allAchievements")]
        public Dictionary<string, int> AllAchievements { get; set; }
        
        [JsonProperty("longestShot")]
        public int LongestShot { get; set; }
        
        [JsonProperty("savageKills")]
        public int SavageKills { get; set; }
        
        [JsonProperty("bossKills")]
        public int BossKills { get; set; }

        [JsonProperty("modWeaponStats")]
        public Dictionary<string, Dictionary<string, WeaponInfo>> ModWeaponStats { get; set; } = null;
        
        [JsonProperty("playedAs")]
        public string PlayedAs { get; set; }
        
        [JsonProperty("pmcSide")]
        public string PmcSide { get; set; }
        
        [JsonProperty("prestige")]
        public int Prestige { get; set; }

        [JsonProperty("publicProfile")]
        public bool PublicProfile { get; set; } = true;
        
        [JsonProperty("hasKappa")]
        public bool HasKappa { get; set; } = false;
        
        [JsonProperty("raidDamage")]
        public int RaidDamage { get; set; }
        
        [JsonProperty("registrationDate")]
        public long RegistrationDate { get; set; }
        
        [JsonProperty("scavLevel")]
        public int ScavLevel { get; set; }

        [JsonProperty("traderInfo")]
        public Dictionary<string, TraderData> TraderInfo { get; set; } = null;
    }
}