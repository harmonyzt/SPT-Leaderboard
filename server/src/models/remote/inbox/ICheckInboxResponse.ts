export interface ICheckInboxResponse {
    status: string,
    sessionId: string,
    messageText: string | undefined,
    rewardTpls: string[] | undefined,
}
