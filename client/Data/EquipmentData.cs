using Newtonsoft.Json;

namespace SPTLeaderboard.Data;

public class EquipmentData
{
    [JsonProperty("rig")]
    public int TacticalVest { get; set; } = 0;
    
    [JsonProperty("pockets")]
    public int Pockets { get; set; } = 0;
    
    [JsonProperty("backpack")]
    public int Backpack { get; set; } = 0;
    
    [JsonProperty("securedContainer")]
    public int SecuredContainer { get; set; } = 0;
    
    [JsonProperty("stash")]
    public int Stash { get; set; } = 0;
}