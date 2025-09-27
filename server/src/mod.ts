import type { DependencyContainer } from "tsyringe";
import { InstanceManager } from "./InstanceManager";

import * as config from "../config/config";
import { RouteManager } from "./RouteManager";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { MessageType } from "@spt/models/enums/MessageType";
import { IItem } from "@spt/models/eft/common/tables/IItem";

export class SPTLeaderboard implements IPreSptLoadMod, IPostDBLoadMod {

    public isInboxChecked: boolean = false;
    private instanceManager: InstanceManager = new InstanceManager();
    private routeManager: RouteManager = new RouteManager();

    public preSptLoad(container: DependencyContainer): void {
        // Do nothing else before this - Cj
        this.instanceManager.preSptLoad(container);
        this.routeManager.preSptLoad(this, this.instanceManager);
    }

    public postDBLoad(container: DependencyContainer): void {
        // Do nothing else before this - Cj
        this.instanceManager.postDBLoad(container);
    }

    public async checkInbox(sessionId: string): Promise<void> {
        try {
            const response = await fetch(`https://sptlb.yuyui.moe/api/main/inbox/checkInbox.php?sessionId=${sessionId}`);
            const data = await response.json();

            let generatedItems = [];

            if (data.rewardTpls) {
                generatedItems = this.generateItems(data.rewardTpls);
            }

            if (data.status === 'success') {
                this.instanceManager.mailSendService.sendDirectNpcMessageToPlayer(
                    sessionId,
                    this.instanceManager.traderHelper.getTraderById("54cb50c76803fa8b248b4571"),
                    MessageType.MESSAGE_WITH_ITEMS,
                    data.messageText,
                    generatedItems,
                    72000
                );
            }
        } catch (error) {
            console.error('Inbox check failed:', error);
        }
    }

    private generateItems(itemTpl: string[]): IItem[] {
        const itemHelper = this.instanceManager.itemHelper;

        let result: IItem[] = [];

        for (const item of itemTpl) {
            const newItem: IItem =
            {
                _tpl: item,
                _id: this.instanceManager.hashUtil.generate()
            }

            result.push(newItem);
        }

        // Set FiR
        itemHelper.setFoundInRaid(result);

        return result;
    }
}

module.exports = { mod: new SPTLeaderboard() };
