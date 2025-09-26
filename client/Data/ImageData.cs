using Newtonsoft.Json;

namespace SPTLeaderboard.Data;

public class ImageData
{
    [JsonProperty("image")]
    public string EncodedImage;

    [JsonProperty("player_id")]
    public string PlayerId;
    
    [JsonProperty("isFullBody")]
    public bool IsFullBody;
    
    [JsonProperty("token")]
    public string Token;
}