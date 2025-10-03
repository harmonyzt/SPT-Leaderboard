import { inject, injectable } from "tsyringe";
import { LeaderboardItemCallbacks } from "../callbacks/LeaderboardItemCallbacks";
import { RouteAction, StaticRouter } from "@spt/di/Router";
import { ILeaderboardGetItemPricesRequest } from "../models/routes/items/ILeaderboardGetItemPricesRequest";

@injectable()
export class LeaderboardItemRoute extends StaticRouter {
    constructor(@inject("LeaderboardItemCallbacks") protected leaderboardItemCallbacks: LeaderboardItemCallbacks) {
        super([
            new RouteAction("/SPTLB/GetItemPrices", async (url: string, info: ILeaderboardGetItemPricesRequest, sessionId: string, output: string): Promise<number> => {
                return this.leaderboardItemCallbacks.handleItemPrices(url, info, sessionId);
            }),
        ]);
    }
}
