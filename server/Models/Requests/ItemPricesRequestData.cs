using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTLeaderboard.Models.Requests;

public record ItemPricesRequestData : IRequestData
{
    public MongoId[] TemplateIds { get; init; }

    public ItemPricesRequestData(MongoId[] templateIds)
    {
        TemplateIds = templateIds;
    }
}