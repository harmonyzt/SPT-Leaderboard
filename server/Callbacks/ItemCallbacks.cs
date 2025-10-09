using Microsoft.AspNetCore.Components;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;
using SPTLeaderboard.Models.Requests;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Callbacks;

[Injectable]
public class ItemCallbacks(HttpResponseUtil httpResponseUtil, ItemUtils itemUtils)
{
    public ValueTask<string> HandleItemPrices(ItemPricesRequestData requestData)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(itemUtils.GetTotalFleaPrice(requestData.TemplateIds)));
    }
}