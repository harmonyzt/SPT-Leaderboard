using System.Collections.Generic;
using Newtonsoft.Json;

namespace SPTLeaderboard.Data;

public class ItemsData
{
    [JsonProperty("templateIds")]
    public List<string> Items = new List<string>();
}