import { inject, injectable } from "tsyringe";
import { ItemHelper } from "../helpers/ItemHelper";
import { ILeaderboardGetItemPricesRequest } from "../models/routes/items/ILeaderboardGetItemPricesRequest";

@injectable()
export class LeaderboardItemCallbacks {
    constructor(@inject("ItemHelper") protected itemHelper: ItemHelper) { }

    public handleItemPrices(_url: string, _info: ILeaderboardGetItemPricesRequest, _sessionId: string): number {
        return this.itemHelper.getTotalFleaPrice(_info.templateIds);
    }
}
