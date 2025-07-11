using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPTLeaderboard.Data
{
    public class ProfileData
    {
        [JsonProperty("_id")] public string _ID { get; set; }

        [JsonProperty("aid")] public string aID { get; set; }

        [JsonProperty("savage")] public string Savage { get; set; }

        [JsonProperty("Info")] public Info Info { get; set; }
        
        [JsonProperty("Inventory")] public Inventory Inventory { get; set; }
    }
    
    public class Info
    {
        [JsonProperty("Side")] public string Side { get; set; }
    }
    
    public class Inventory
    {
        [JsonProperty("items")] public List<InventoryItem> Items { get; set; }
    }

    public class InventoryItem
    {
        [JsonProperty("_id")] public string ID { get; set; }

        [JsonProperty("_tpl")] public string Tpl { get; set; }
    }
    
    public class TraderData
    {
        [JsonProperty("id")] public string ID { get; set; }
        
        [JsonProperty("unlocked")] public bool Unlocked { get; set; }

        [JsonProperty("loyaltyLevel")] public int LoyaltyLevel { get; set; }

        [JsonProperty("salesSum")] public long SalesSum { get; set; }

        [JsonProperty("standing")] public double Standing { get; set; }

        [JsonProperty("disabled")] public bool Disabled { get; set; }
        
        [JsonProperty("notFound", NullValueHandling = NullValueHandling.Ignore)] public bool? NotFound { get; set; }
    }
}