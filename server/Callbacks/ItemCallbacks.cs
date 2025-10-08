using Microsoft.AspNetCore.Components;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTLeaderboard.Models.Requests;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Callbacks;

[Injectable(InjectionType.Singleton)]
public class ItemCallbacks(ItemUtils itemUtils)
{
    public ValueTask<double> HandleItemPrices(string url, ItemPricesRequestData requestData, MongoId sessionId, string? output)
    {
        return new ValueTask<double>(itemUtils.GetTotalFleaPrice(requestData.TemplateIds));
    }
}