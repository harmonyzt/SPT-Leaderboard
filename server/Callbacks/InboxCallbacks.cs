using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTLeaderboard.Models;

namespace SPTLeaderboard.Callbacks;

[Injectable(InjectionType.Singleton)]
public class InboxCallbacks(
    SessionInboxChecks inboxChecks,
    HttpResponseUtil httpResponseUtil,
    ILogger<InboxCallbacks> logger)
{
    public ValueTask<object> HandleInboxNotChecked(string url, IRequestData data, MongoId sessionId, string? output)
    {
        logger.LogDebug("{SessionId} not checked", sessionId);
        if (!inboxChecks.TrySetSessionInboxState(sessionId, false))
        {
            logger.LogDebug("added {SessionId} to inbox checks", sessionId);
            inboxChecks.AddSessionInboxState(sessionId, false);
        }

        return new ValueTask<object>(httpResponseUtil.NoBody(output));
    }

    public ValueTask<object> HandleInboxChecked(string url, IRequestData data, MongoId sessionId, string? output)
    {
        logger.LogDebug("{SessionId} is checked", sessionId);
        if (!inboxChecks.TryGetSessionInboxState(sessionId))
        {
            logger.LogDebug("added {SessionId} to inbox checks", sessionId);
            inboxChecks.AddSessionInboxState(sessionId, true);
        }
        return new ValueTask<object>(httpResponseUtil.NoBody(output));
    }
}