using Newtonsoft.Json;

namespace SPTLeaderboard.Data;

public class ImageData
{
    [JsonProperty("image")]
    public string EncodedImage;

    [JsonProperty("player_id")]
    public string PlayerId;
}