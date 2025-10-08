using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;
using SPTLeaderboard.Callbacks;
using SPTLeaderboard.Models.Requests;

namespace SPTLeaderboard.Routers;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class ItemRouter : StaticRouter
{
   private static ItemCallbacks _callbacks;

   public ItemRouter(JsonUtil jsonUtil, ItemCallbacks callbacks) : base(jsonUtil, GetRoutes())
   {
      _callbacks = callbacks;
   }

   private static List<RouteAction> GetRoutes()
      => [
         new("/SPTLB/GetItemPrices", async (url, data, sessionId, output) => await _callbacks.HandleItemPrices(url, (data as ItemPricesRequestData)!, sessionId, output))
      ];
}