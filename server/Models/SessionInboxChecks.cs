using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace SPTLeaderboard.Models;

[Injectable(InjectionType.Singleton)]
public class SessionInboxChecks
{
    private Dictionary<MongoId, bool> _inboxChecks = new Dictionary<MongoId, bool>();
    
    public SessionInboxChecks()
    { }

    public void AddSessionInboxState(MongoId sessionId, bool inboxState)
    {
       _inboxChecks.Add(sessionId, inboxState); 
    } 
    
    
    public bool TrySetSessionInboxState(MongoId sessionId, bool newChecked)
    {
        if (!_inboxChecks.ContainsKey(sessionId))
        {
            return false;
        }

        _inboxChecks[sessionId] = newChecked;
        return true;
    }

    public bool TryGetSessionInboxState(MongoId sessionId)
    {
        return _inboxChecks.ContainsKey(sessionId) && _inboxChecks[sessionId];
    }
}