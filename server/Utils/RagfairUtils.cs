using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace SPTLeaderboard.Utils;
[Injectable(InjectionType.Singleton)]
public class RagfairUtils(RagfairOfferService ragfairOfferService, ItemHelper itemHelper)
{
    public double GetLowestItemPrice(MongoId templateId)
    {
        var offers = ragfairOfferService.GetOffersOfType(templateId);
        if (offers == null)
        {
            return 0;
        }

        offers = offers.ToArray();
        if (!offers.Any())
        {
            return 0;
        }

        offers = offers.Where(o => o.User?.MemberType != MemberCategory.Trader
                                && o.Requirements?.First().TemplateId == Money.ROUBLES
                                && itemHelper.GetItemQualityModifier(o.Items?.First()!) == 1.0).ToArray();

        return offers.Any() ? 0 : offers.Min(o => o.SummaryCost) ?? 0;
    }
}