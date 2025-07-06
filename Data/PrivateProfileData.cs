using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPTLeaderboard.Data
{
    public class PrivateProfileData(BaseData baseData)
    {
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

        [JsonProperty("token")]
        public string Token { get; set; } = baseData.Token;

        [JsonProperty("DBinINV")]
        public bool DBinInv { get; set; } = baseData.DBinInv;

        [JsonProperty("isCasual")]
        public bool IsCasual { get; set; } = baseData.IsCasual;

        [JsonProperty("publicProfile")]
        public bool IsPublicProfile { get; set; } = false;
    }
}