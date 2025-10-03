import { inject, injectable } from "tsyringe";
import { RagfairHelper } from "./RagfairHelper";
import { ItemHelper } from "@spt/helpers/ItemHelper";
import { HashUtil } from "@spt/utils/HashUtil";
import { IItem } from "@spt/models/eft/common/tables/IItem";

@injectable()
export class LeaderboardItemHelper {
    constructor(
        @inject("RagfairHelper") protected ragfairHelper: RagfairHelper,
        @inject("ItemHelper") protected itemHelper: ItemHelper,
        @inject("HashUtil") protected hashUtil: HashUtil
    ) { }

    public getTotalFleaPrice(templateIds: string[]): number {
        if (templateIds.length == 0) {
            return 0;
        }

        let totalPrice = 0;
        for (let item of templateIds) {
            let lowestPrice = this.ragfairHelper.getLowestItemPrice(item);
            totalPrice += lowestPrice;
        }
        return totalPrice;
    }

    public getItemInstancesAsFiR(templateIds: string[]): IItem[] {
        let result: IItem[];
        for (let item of templateIds) {
            result.push({ _tpl: item, _id: this.hashUtil.generate() });
        }
        this.itemHelper.setFoundInRaid(result);
        return result;
    }
}
