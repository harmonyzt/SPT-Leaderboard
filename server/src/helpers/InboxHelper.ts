import { inject } from "tsyringe";
import { LeaderboardItemHelper } from "./LeaderboardItemHelper";
import { MailSendService } from "@spt/services/MailSendService";
import { TraderHelper } from "@spt/helpers/TraderHelper";
import { ICheckInboxResponse } from "../models/remote/inbox/ICheckInboxResponse";
import { MessageType } from "@spt/models/enums/MessageType";
import { SPTLeaderboard } from "../mod";

export class InboxHelper {
    constructor(
        @inject("LeaderboardItemHelper") protected leaderboardItemHelper: LeaderboardItemHelper,
        @inject("MailSendService") protected mailSendService: MailSendService,
        @inject("TraderHelper") protected traderHelper: TraderHelper
    ) { }

    public async checkInbox(sessionId: string): Promise<boolean> {
        try {
            const url = new URL("/api/main/checkInbox.php", SPTLeaderboard.apiUrl);
            const params = new URLSearchParams(`sessionId=${sessionId}`);

            let response = await fetch(url + params.toString());
            if (!response.ok) {
                return false;
            }

            let data: ICheckInboxResponse = await response.json() as ICheckInboxResponse;
            if (data.status !== 'success') {
                return false;
            }

            let generatedItems = this.leaderboardItemHelper.getItemInstancesAsFiR(data.rewardTpls);

            this.mailSendService.sendDirectNpcMessageToPlayer(
                sessionId,
                this.traderHelper.getTraderById("54cb50c76803fa8b248b4571"),
                MessageType.MESSAGE_WITH_ITEMS,
                data.messageText,
                generatedItems,
                72000);

            return true;
        } catch (error) {
            console.error('Inbox check failed: ', error);
            return false;
        }
    }
}
