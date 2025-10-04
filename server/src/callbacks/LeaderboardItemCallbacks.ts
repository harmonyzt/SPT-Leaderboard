import { inject, injectable } from "tsyringe";
import { LeaderboardItemHelper } from "../helpers/LeaderboardItemHelper";
import { ILeaderboardGetItemPricesRequest } from "../models/routes/items/ILeaderboardGetItemPricesRequest";
import { HttpResponseUtil } from "@spt/utils/HttpResponseUtil";

@injectable()
export class LeaderboardItemCallbacks {
    constructor(
        @inject("LeaderboardItemHelper") protected itemHelper: LeaderboardItemHelper,
        @inject("HttpResponseUtil") protected httpResponseUtil: HttpResponseUtil
    ) { }

    public handleItemPrices(_url: string, _info: ILeaderboardGetItemPricesRequest, _sessionId: string): string {
        return this.httpResponseUtil.noBody(this.itemHelper.getTotalFleaPrice(_info.templateIds));
    }
}
