import type { DependencyContainer } from "tsyringe";

import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { LeaderboardInboxRouter } from "./routers/LeaderboardInboxRouter";
import { LeaderboardInboxCallbacks } from "./callbacks/LeaderboardInboxCallbacks";
import { InboxHelper } from "./helpers/InboxHelper";
import { LeaderboardItemRouter } from "./routers/LeaderboardItemRouter";
import { LeaderboardItemCallbacks } from "./callbacks/LeaderboardItemCallbacks";
import { LeaderboardItemHelper } from "./helpers/LeaderboardItemHelper";
import { LeaderboardRagfairHelper } from "./helpers/LeaderboardRagfairHelper";
import config from "../config.json";

export class SPTLeaderboard implements IPreSptLoadMod {

    public static sessionsInboxChecks: Map<string, boolean> = new Map();
    public static apiUrl: URL = new URL(config.api_url);

    public preSptLoad(container: DependencyContainer): void {
        container.register<LeaderboardInboxRouter>("LeaderboardInboxRouter", LeaderboardInboxRouter);
        container.register<LeaderboardInboxCallbacks>("LeaderboardInboxCallbacks", LeaderboardInboxCallbacks);
        container.register<LeaderboardItemRouter>("LeaderboardItemRouter", LeaderboardItemRouter);
        container.register<LeaderboardItemCallbacks>("LeaderboardItemCallbacks", LeaderboardItemCallbacks);
        container.register<InboxHelper>("InboxHelper", InboxHelper);
        container.register<LeaderboardItemHelper>("LeaderboardItemHelper", LeaderboardItemHelper);
        container.register<LeaderboardRagfairHelper>("RagfairHelper", LeaderboardRagfairHelper);
        container.registerType("StaticRoutes", "LeaderboardInboxRouter");
        container.registerType("StaticRoutes", "LeaderboardItemRouter");
    }
}

module.exports = { mod: new SPTLeaderboard() };
