using Newtonsoft.Json;

namespace SPTLeaderboard.Data;

public class HitsData
{
    [JsonProperty("head")]
    public int Head { get; set; } = 0;
    
    [JsonProperty("chest")]
    public int Chest { get; set; } = 0;
    
    [JsonProperty("stomach")]
    public int Stomach { get; set; } = 0;
    
    [JsonProperty("leftArm")]
    public int LeftArm { get; set; } = 0;
    
    [JsonProperty("rightArm")]
    public int RightArm { get; set; } = 0;
    
    [JsonProperty("leftLeg")]
    public int LeftLeg { get; set; } = 0;
    
    [JsonProperty("rightLeg")]
    public int RightLeg { get; set; } = 0;
}