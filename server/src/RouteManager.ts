import type { InstanceManager } from "./InstanceManager";
import { SPTLeaderboard } from "./mod";


import * as config from "../config/config"
import { ICoreConfig } from "@spt/models/spt/config/ICoreConfig";
import { ConfigTypes } from "@spt/models/enums/ConfigTypes";

export class RouteManager {
    private sptLeaderboard: SPTLeaderboard;
    private InstanceManager: InstanceManager;
    private coreConfig: ICoreConfig;

    // Individual checks for inbox
    private inboxChecked: Map<string, any> = new Map();

    // -------------------------- Public members --------------------------

    public preSptLoad(
        sptLeaderboard: SPTLeaderboard,
        instanceManager: InstanceManager
    ): void {
        this.sptLeaderboard = sptLeaderboard;
        this.InstanceManager = instanceManager;

        this.coreConfig = instanceManager.configServer.getConfig<ICoreConfig>(ConfigTypes.CORE);

        // Register inbox checking if it's enabled - otherwise do nothing
        if (config.external_inbox_check) {
            this.registerRoutes();
        }
    }

    // -------------------------- Private members --------------------------

    private registerRoutes(): void {
        this.registerMatchEndRoute();
        this.registerProfileItemsMovingRoute();
        this.registerGameLogoutRoute();
    }

    private registerMatchEndRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBProfileRaidEndCheckInbox",
            [
                {
                    url: "/client/match/local/end",
                    action: async (url, info, sessionId, output) => {
                        if (!sessionId) {
                            return output
                        }

                        // Player can check for inbox once again if left the game or in raid
                        this.inboxChecked.set(sessionId, {
                            state: false
                        });

                        return output;
                    }
                }
            ],
            "aki"
        );
    }

    private registerProfileItemsMovingRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter("SPTLBHeartBeatInStash",
            [
                {
                    url: "/client/game/profile/items/moving",
                    action: async (url, info, sessionId, output) => {
                        if (!sessionId) {
                            return output
                        }

                        // We want to check it only once
                        const wasInboxChecked = this.inboxChecked.get(sessionId);
                        if (sessionId && !wasInboxChecked?.state || true) {
                            this.inboxChecked.set(sessionId, {
                                state: true
                            });

                            this.sptLeaderboard.checkInbox(sessionId);
                        }

                        return output;
                    }
                }
            ],
            "aki"
        );
    }

    private registerGameLogoutRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBHeartBeatLogout",
            [
                {
                    url: "/client/game/logout",
                    action: async (url, info, sessionId, output) => {
                        // Player can check for inbox once again if left the game or in raid
                        this.inboxChecked.set(sessionId, {
                            state: false
                        });
                        return output;
                    }
                }
            ],
            "aki"
        );
    }
}