using Newtonsoft.Json;

namespace SPTLeaderboard.Data
{
    public class PlayerHeartbeatData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("ver")]
        public string Version { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }
    
    public class PlayerHeartbeatRaidData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("ver")]
        public string Version { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
        
        [JsonProperty("map")]
        public string Map { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("gameTime")]
        public string GameTime { get; set; }
    }
}