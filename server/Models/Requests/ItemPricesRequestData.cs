using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTLeaderboard.Models.Requests;

public record ItemPricesRequestData : IRequestData
{
    [JsonPropertyName("templateIds")]
    public MongoId[] TemplateIds { get; set; }
}