using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;
using SPTLeaderboard.Callbacks;

namespace SPTLeaderboard.Routers;
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class InboxRouter : StaticRouter
{
    private static InboxCallbacks _callbacks;

    public InboxRouter(JsonUtil jsonUtil, InboxCallbacks callbacks) : base(jsonUtil, GetRoutes())
    {
        _callbacks = callbacks;
    }

    private static List<RouteAction> GetRoutes()
    {
        return
        [
            new RouteAction("/client/match/local/end", async (url, data, sessionId, output) => await _callbacks.HandleInboxNotChecked(sessionId, output)),
            new RouteAction("/client/game/logout", async (url, data, sessionId, output) => await _callbacks.HandleInboxNotChecked(sessionId, output)),
            new RouteAction("/client/game/profile/items/moving", async (url, data, sessionId, output) => await _callbacks.HandleInboxChecked(sessionId, output)),
        ];
    }
    
}