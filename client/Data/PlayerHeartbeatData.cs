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
}