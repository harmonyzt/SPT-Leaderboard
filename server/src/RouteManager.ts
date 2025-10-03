import type { InstanceManager } from "./InstanceManager";
import { SPTLeaderboard } from "./mod";

import * as config from "../config/config"
import { ICoreConfig } from "@spt/models/spt/config/ICoreConfig";
import { ConfigTypes } from "@spt/models/enums/ConfigTypes";
import { RagfairOfferService } from "@spt/services/RagfairOfferService";
import { IRagfairOffer } from "@spt/models/eft/ragfair/IRagfairOffer";

export class RouteManager {
    private sptLeaderboard: SPTLeaderboard;
    private InstanceManager: InstanceManager;
    private offerService: RagfairOfferService;
    private coreConfig: ICoreConfig;


    // Individual checks for inbox
    private inboxChecked: Map<string, any> = new Map();

    // -------------------------- Public members --------------------------

    constructor(sptLeaderboard: SPTLeaderboard, instanceManager: InstanceManager, ragfair: RagfairOfferService) {
        this.sptLeaderboard = sptLeaderboard;
        this.InstanceManager = instanceManager;
        this.offerService = ragfair;
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
        this.registerServerPricingRoute();
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

    private registerServerPricingRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBHeartBeatLogout",
            [
                {
                    url: "/SPTLB/GetItemPrices",
                    action: async (url, info, sessionId, output) => {
                        // get info from client mod and our template ids inside to return a total price in RUB
                        return JSON.stringify(this.calculateTotalPrice(info.templateIds));
                    }
                }
            ],
            "aki"
        );
    }

    private calculateTotalPrice(templateIds: string[]): number {
        if (!templateIds || !Array.isArray(templateIds)) {
            return 0;
        }

        let totalPrice = 0;

        // Go through every template ID
        for (const templateId of templateIds) {
            const lowestPrice = this.getLowestItemPrice(templateId);
            if (lowestPrice !== null) {
                totalPrice += lowestPrice;
            }
        }

        return totalPrice;
    }

    private getLowestItemPrice(templateId: string): number | null {
        let offers: IRagfairOffer[] = this.offerService.getOffersOfType(templateId);

        if (offers && offers.length > 0) {
            offers = offers.filter(a => a.user.memberType != 4
                && a.requirements[0]._tpl == '5449016a4bdc2d6f028b456f' // Only rubles
                && this.InstanceManager.itemHelper.getItemQualityModifier(a.items[0]) == 1 // Only items (not weapons)
            );

            if (offers.length > 0) {
                return offers.sort((a, b) => a.summaryCost - b.summaryCost)[0].summaryCost;
            }
        }

        return null;
    }
}
