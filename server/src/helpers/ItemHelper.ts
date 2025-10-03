import { inject, injectable } from "tsyringe";
import { RagfairHelper } from "./RagfairHelper";

@injectable()
export class ItemHelper {
    constructor(@inject("RagfairHelper") protected ragfairHelper: RagfairHelper) { }

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
}
