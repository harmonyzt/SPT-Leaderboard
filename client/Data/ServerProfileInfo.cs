using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPTLeaderboard.Data
{
    public class ServerProfileInfo
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("currlvl")]
        public int CurrentLVL { get; set; }

        [JsonProperty("currexp")]
        public int CurrentXP { get; set; }

        [JsonProperty("prevexp")]
        public int PrevEXP { get; set; }

        [JsonProperty("nextlvl")]
        public int NextLVL { get; set; }

        [JsonProperty("maxlvl")]
        public int MaxLVL { get; set; }

        [JsonProperty("edition")]
        public string Edition { get; set; }

        [JsonProperty("profileId")]
        public string ProfileID { get; set; }

        [JsonProperty("sptData")]
        public SPTModsData SptModsData { get; set; }
    }

    public class SPTModsData
    {
        [JsonProperty("mods")]
        public List<ModItem> Mods { get; set; }
    }

    public class ModItem
    {
        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }
}