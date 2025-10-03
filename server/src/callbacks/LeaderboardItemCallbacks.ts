import { inject, injectable } from "tsyringe";
import { LeaderboardItemHelper } from "../helpers/ItemHelper";
import { ILeaderboardGetItemPricesRequest } from "../models/routes/items/ILeaderboardGetItemPricesRequest";

@injectable()
export class LeaderboardItemCallbacks {
    constructor(@inject("LeaderboardItemHelper") protected itemHelper: LeaderboardItemHelper) { }

    public handleItemPrices(_url: string, _info: ILeaderboardGetItemPricesRequest, _sessionId: string): number {
        return this.itemHelper.getTotalFleaPrice(_info.templateIds);
    }
}
