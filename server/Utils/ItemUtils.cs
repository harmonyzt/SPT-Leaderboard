using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTLeaderboard.Utils;
[Injectable(InjectionType.Singleton)]
public class ItemUtils(ItemHelper itemHelper, RagfairUtils ragfairUtils, HashUtil hashUtil, ISptLogger<ItemUtils> logger)
{
    public double GetTotalFleaPrice(MongoId[] templateIds)
    {
        if (templateIds.Length == 0)
        {
            return 0;
        }

        return templateIds.Sum(ragfairUtils.GetLowestItemPrice);
    }

    public IEnumerable<Item> GetItemInstancesAsFiR(MongoId[] templateIds)
    { 
        var items = templateIds.Select(item => new Item() { Template = item, Id = hashUtil.GenerateHashForData(HashingAlgorithm.SHA1, DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()) }).ToList();
        itemHelper.SetFoundInRaid(items);
        return items;
    }
}