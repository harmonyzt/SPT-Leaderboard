import { ItemHelper } from "@spt/helpers/ItemHelper";
import { RagfairOfferHelper } from "@spt/helpers/RagfairOfferHelper";
import { IRagfairOffer } from "@spt/models/eft/ragfair/IRagfairOffer";
import { RagfairOfferService } from "@spt/services/RagfairOfferService";
import { inject, injectable } from "tsyringe";

@injectable()
export class RagfairHelper {
    constructor(
        @inject("RagfairOfferService") protected ragfairOfferService: RagfairOfferService,
        @inject("ItemHelper") protected itemHelper: ItemHelper) {
    }

    public getLowestItemPrice(templateId: string): number {
        let offers: IRagfairOffer[] = this.ragfairOfferService.getOffersOfType(templateId);

        if (offers === undefined) {
            return 0;
        }

        if (offers.length == 0) {
            return 0;
        }

        offers = offers.filter(o => o.user.memberType != 4
            && o.requirements[0]._tpl == '5449016a4bdc2d6f028b456f'
            && this.itemHelper.getItemQualityModifier(o.items[0]) == 1);

        if (offers.length == 0) { //empty after filter
            return 0;
        }

        return offers.sort((a, b) => a.summaryCost - b.summaryCost)[0].summaryCost;
    }
}
