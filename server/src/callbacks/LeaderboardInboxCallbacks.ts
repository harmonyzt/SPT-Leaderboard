import { inject, injectable } from "tsyringe";
import { SPTLeaderboard } from "../mod";

@injectable()
export class LeaderboardInboxCallbacks {
    constructor(@inject("SPTLeaderboard") protected sptLeaderboard: SPTLeaderboard) { }

    public handleInboxNotChecked(_url: string, _info: any, _sessionId: string): void {
        SPTLeaderboard.sessionsInboxChecks.set(_sessionId, false);
    }

    public handleInboxChecked(_url: string, _info: any, _sessionId: string): void {
        let sessionInboxCheck = SPTLeaderboard.sessionsInboxChecks.get(_sessionId);
        if (sessionInboxCheck === false) {
            SPTLeaderboard.sessionsInboxChecks.set(_sessionId, true);
            this.sptLeaderboard.checkInbox(_sessionId);
        }
    }
}
