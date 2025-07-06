using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPTLeaderboard.Data
{
    public class BaseData
    {
        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("health")]
        public float Health { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isScav")]
        public bool IsScav { get; set; }

        [JsonProperty("modINT")]
        public string ModInt { get; set; }

        [JsonProperty("mods")]
        public List<string> Mods { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pmcHealth")]
        public float PmcHealth { get; set; }

        [JsonProperty("pmcLevel")]
        public int PmcLevel { get; set; }

        [JsonProperty("raidKills")]
        public int RaidKills { get; set; }

        [JsonProperty("raidResult")]
        public string RaidResult { get; set; }

        [JsonProperty("raidTime")]
        public float RaidTime { get; set; }

        [JsonProperty("sptVer")]
        public string SptVersion { get; set; }

        // [JsonProperty("teamTag")]
        // public string TeamTag { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("DBinINV")]
        public bool DBinInv { get; set; }

        [JsonProperty("isCasual")]
        public bool IsCasual { get; set; }
    }
}