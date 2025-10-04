import { inject, injectable } from "tsyringe";
import { InboxHelper } from "../helpers/InboxHelper";
import { SPTLeaderboard } from "../mod";

@injectable()
export class LeaderboardInboxCallbacks {
    constructor(@inject("InboxHelper") protected inboxHelper: InboxHelper) { }

    public handleInboxNotChecked(_url: string, _info: any, _sessionId: string): void {
        SPTLeaderboard.sessionsInboxChecks.set(_sessionId, false);
    }

    public handleInboxChecked(_url: string, _info: any, _sessionId: string): void {
        let sessionInboxCheck = SPTLeaderboard.sessionsInboxChecks.get(_sessionId);
        if (sessionInboxCheck === false) {
            SPTLeaderboard.sessionsInboxChecks.set(_sessionId, true);
            this.inboxHelper.checkInbox(_sessionId);
        }
    }
}
