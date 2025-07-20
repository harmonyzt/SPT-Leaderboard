using Newtonsoft.Json;

namespace SPTLeaderboard.Data;

public class ImageData
{
    [JsonProperty("image")]
    public string EncodedImage;
}