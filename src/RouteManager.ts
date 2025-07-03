import path from "node:path";
import fs from "node:fs";

import type { InstanceManager } from "./InstanceManager";
import { SPTLeaderboard } from "./mod";
import { PlayerState } from "./enums/PlayerState";

import * as config from "../config/config"
import { ICoreConfig } from "@spt/models/spt/config/ICoreConfig";
import { ConfigTypes } from "@spt/models/enums/ConfigTypes";

export class RouteManager {
    private sptLeaderboard: SPTLeaderboard;
    private InstanceManager: InstanceManager;

    private coreConfig: ICoreConfig;

    private PHP_ENDPOINT: string = config.PHP_ENDPOINT || "visuals.nullcore.net";
    private PHP_PATH: string = config.PHP_PATH || "/SPT/api/v1/main.php";

    private retriesCount: number = 0;
    private connectivity: number = 1;

    // Cache for heartbeats + 10 sec time out (sessionId: timestamp)
    private stateCache: Map<string, any> = new Map();

    // -------------------------- Public members --------------------------

    public preSptLoad(
        sptLeaderboard: SPTLeaderboard,
        instanceManager: InstanceManager
    ): void {
        this.sptLeaderboard = sptLeaderboard;
        this.InstanceManager = instanceManager;

        this.coreConfig = instanceManager.configServer.getConfig<ICoreConfig>(ConfigTypes.CORE);

        this.registerRoutes();
    }

    // -------------------------- Private members --------------------------

    private registerRoutes(): void {

        // I broke out route registration into their own methods for readability - Cj

        this.registerStackTrackRoute();
        this.registerMatchEndRoute();
        this.registerProfileInfoRoute();
        this.registerClientGlobalsRoute();
        this.registerClientMatchStartRoute();
        this.registerProfileItemsMovingRoute();
        this.registerGameLogoutRoute();
    }

    private registerStackTrackRoute(): void {
        const sptRoot = this.sptLeaderboard.sptRoot;
        const statTrackPath = path.join(sptRoot, 'user', 'mods/acidphantasm-stattrack');

        // Early return, reduces nesting - Cj
        if (!config.enable_mod_support && !fs.existsSync(statTrackPath)) {
            this.sptLeaderboard.isUsingStattrack = false;
            this.sptLeaderboard.modWeaponStats = null;
            return;
        }

        this.sptLeaderboard.isUsingStattrack = true;

        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBStattrackSupport",
            [
                {
                    url: "/stattrack/save",
                    // eslint-disable-next-line @typescript-eslint/no-unused-vars
                    action: async (url, info, sessionId, output) => {
                        this.sptLeaderboard.modWeaponStats = this.sptLeaderboard.getAllValidWeapons(sessionId, info);
                        return output;
                    }
                }
            ],
            "aki"
        );
    }

    private registerMatchEndRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBProfileRaidEnd",
            [
                {
                    url: "/client/match/local/end",
                    action: async (url, info, sessionId, output) => {

                        if (!sessionId) {
                            return output
                        }

                        if (config.public_profile) {
                            // Removed `raid_end` here, not sure what that was even doing - CJ
                            this.sendHeartbeat(sessionId, output);
                        }

                        const profileHelper = this.InstanceManager.profileHelper;
                        const profile = profileHelper.getFullProfile(sessionId);
                        this.sptLeaderboard.staticProfile = profile;
                        this.sptLeaderboard.serverMods = profile.spt.mods.map(mod => mod.name).join(', ');

                        await this.gatherProfileInfo(info);

                        return output;
                    }
                }
            ],
            "aki"
        );
    }

    private registerProfileInfoRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBHeartBeatOnline",
            [
                {
                    url: "/launcher/profile/info",
                    action: async (url, info, sessionId, output) => {
                        if (!sessionId || !config.public_profile) return output;

                        const currentState = this.stateCache.get(sessionId);

                        // No state for profile at all - send ONLINE
                        if (!currentState) {
                            this.stateCache.set(sessionId, {
                                state: PlayerState.ONLINE,
                                lastSentTime: 0
                            });
                        }

                        // Always send the heartbeat
                        this.sendHeartbeat(sessionId, output);

                        return output;
                    }
                }
            ],
            "aki"
        );
    }

    private registerClientGlobalsRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBHeartBeatInMenu",
            [
                {
                    url: "/client/globals",
                    action: async (url, info, sessionId, output) => {
                        if (sessionId && config.public_profile) {
                            this.stateCache.set(sessionId, {
                                state: PlayerState.IN_MENU,
                                lastSentTime: this.stateCache.get(sessionId)?.lastSentTime || 0
                            });
                        }

                        if (sessionId) {
                            this.sptLeaderboard.checkInbox(sessionId);
                        }

                        return output;
                    }
                }
            ],
            "aki"
        );
    }

    private registerClientMatchStartRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBHeartBeatRaidStart",
            [
                {
                    url: "/client/match/local/start",
                    action: async (url, info, sessionId, output) => {
                        if (sessionId && config.public_profile) {
                            this.stateCache.set(sessionId, {
                                state: PlayerState.IN_RAID,
                                lastSentTime: this.stateCache.get(sessionId)?.lastSentTime || 0
                            });
                            this.sendHeartbeat(sessionId, output); // Send instantly when entering raid
                        }
                        return output;
                    }
                }
            ],
            "aki"
        );
    }

    private registerProfileItemsMovingRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter("SPTLBHeartBeatInStash",
            [
                {
                    url: "/client/game/profile/items/moving",
                    action: async (url, info, sessionId, output) => {
                        if (sessionId && config.public_profile) {
                            this.stateCache.set(sessionId, {
                                state: PlayerState.IN_STASH,
                                lastSentTime: this.stateCache.get(sessionId)?.lastSentTime || 0
                            });
                        }
                        return output;
                    }
                }
            ],
            "aki"
        );
    }

    private registerGameLogoutRoute(): void {
        const staticRouter = this.InstanceManager.staticRouter;
        staticRouter.registerStaticRouter(
            "SPTLBHeartBeatLogout",
            [
                {
                    url: "/client/game/logout",
                    action: async (url, info, sessionId, output) => {
                        if (sessionId && config.public_profile) {
                            this.stateCache.set(sessionId, {
                                state: PlayerState.IN_STASH,
                                lastSentTime: this.stateCache.get(sessionId)?.lastSentTime || 0
                            });
                        }
                        return output;
                    }
                }
            ], 
            "aki"
        );
    }

    private async sendHeartbeat(sessionId, output) {

        if (!this.stateCache.has(sessionId)) {
            return;
        }

        const cachedData = this.stateCache.get(sessionId);
        const timeSinceLast = Date.now() - cachedData.lastSentTime;

        const HEARTBEAT_THROTTLE_MS = 10 * 1000;

        // Throttle for heartbeats
        if (timeSinceLast < HEARTBEAT_THROTTLE_MS) {
            if (config.DEBUG) {
                this.InstanceManager.logger.info(`[SPT Leaderboard] Skipping Heartbeat: session ${sessionId} was updated ${timeSinceLast} ms ago`);
            }
            return;
        }

        try {
            const response = await fetch('https://visuals.nullcore.net/SPT/api/heartbeat/v1.php', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    type: cachedData.state,
                    timestamp: Date.now(),
                    ver: '2.6.0',
                    sessionId
                })
            });

            const result = await response.json();

            if (config.DEBUG) {
                this.InstanceManager.logger.info(`[SPT Leaderboard] Sent ${cachedData.state} heartbeat: ${result}`);
            }

            // Update last time sent
            this.stateCache.set(sessionId, {
                ...cachedData,
                lastSentTime: Date.now()
            });

        } catch (error) {
            this.InstanceManager.logger.error(`[SPT Leaderboard] Error sending heartbeat: ${error.message}`);
            return output;
        }
    }

    private async gatherProfileInfo(data: any): Promise<void> {
        const jsonData = JSON.parse(JSON.stringify(data));
        const fullProfile = jsonData.results.profile;

        const logger = this.InstanceManager.logger;

        if (config.DEBUG) {
            logger.info(JSON.stringify(jsonData, null, 2));
            logger.log("[SPT Leaderboard] Data above was saved in server log file.", "green");
        }

        this.sptLeaderboard.lastRaidMap = this.sptLeaderboard.getPrettyMapName(jsonData.serverId);
        this.sptLeaderboard.lastRaidMapRaw = jsonData.serverId.split('.')[0];

        // Get the result of a raid (Died/Survived/Runner)
        this.sptLeaderboard.raidResult = jsonData.results.result;
        this.sptLeaderboard.playTime = jsonData.results.playTime;
        this.sptLeaderboard.transitionMap = jsonData?.locationTransit?.location ?? "None";

        try {
            if (this.connectivity == 0) return;

            logger.log(`[SPT Leaderboard] Getting ready to send statistics...`, "cyan");

            if (this.sptLeaderboard.isProfileValid(fullProfile)) {
                await this.processAndSendProfile(fullProfile);
            } else {
                // Time out and retry next raid
                if (this.retriesCount <= config.connectionRetries) {
                    this.retriesCount += 1;
                } else {
                    logger.error(`[SPT Leaderboard] Could not establish internet connection with PHP or your profile does not match requirements. Pausing mod until next SPT Server start...`);
                    this.connectivity = 0;

                    return;
                }
            }
        } catch (e) {
            logger.info(`[SPT Leaderboard] Error: ${e.message}`);
        }
    }

    // Calculate stats from profile
    private async processAndSendProfile(profile: any): Promise<void> {
        const profileData = await this.processProfile(profile);

        if (config.DEBUG)
            this.InstanceManager.logger.info(`[SPT Leaderboard] Data ready!`);

        try {
            await this.sendProfileData(profileData);

            this.InstanceManager.logger.info("[SPT Leaderboard] Data sent to the leaderboard successfully!");
        } catch (e) {
            this.InstanceManager.logger.error(`[SPT Leaderboard] Could not send data to leaderboard: ${e.message}`);
        }
    }

    // Send the data
    private async sendProfileData(data: any): Promise<string> {
        // 20 seconds time-out
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), config.connectionTimeout);

        try {
            const response = await fetch(`https://${this.PHP_ENDPOINT}${this.PHP_PATH}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-SPT-Mod': 'SPTLeaderboard'
                },
                body: JSON.stringify(data),
                signal: controller.signal
            });

            clearTimeout(timeout);

            if (!response.ok) {
                const text = await response.text();
                throw new Error(`HTTPS ${response.status}: ${text}`);
            }

            return await response.text();
        } catch (error) {
            throw error;
        }
    }

    private async processProfile(profile: any): Promise<any> {

        // Would recommend turning this into a method - CJ
        const getStatValue = (keys) => {
            const item = profile.Stats.Eft.SessionCounters.Items?.find(item =>
                item.Key && keys.every((k, i) => item.Key[i] === k)
            );
            return item?.Value || 0;
        };

        // Would recommend turning this into a method - CJ
        const getGlobalStatValue = (keys) => {
            const item = profile.Stats.Eft.OverallCounters.Items?.find(item =>
                item.Key && keys.every((k, i) => item.Key[i] === k)
            );
            return item?.Value || 0;
        };

        const staticProfile = this.sptLeaderboard.staticProfile;

        // MAIN
        let scavLevel = staticProfile.characters.scav.Info.Level;
        let pmcLevel = staticProfile.characters.pmc.Info.Level;
        let profileName = staticProfile.characters.pmc.Info.Nickname;
        const kills = getStatValue(['KilledPmc']);
        const raidEndResult = this.sptLeaderboard.raidResult;
        const combinedModData = this.sptLeaderboard.collectModData() + this.sptLeaderboard.serverMods;
        const isScavRaid = profile.Info.Side === "Savage";

        // If left the raid
        let discFromRaid = false;
        if (raidEndResult === "Left") {
            discFromRaid = true;
        }

        // For transit
        let isTransition = false;
        let lastRaidTransitionTo = "None";

        if (raidEndResult === "Transit" && this.sptLeaderboard.transitionMap !== "None") {
            isTransition = true;
            lastRaidTransitionTo = this.sptLeaderboard.transitionMap;
        } else {
            isTransition = false;
            lastRaidTransitionTo = "None";
        }

        // Secondary stats
        const damage = getStatValue(['CauseBodyDamage']);
        const longestShot = getGlobalStatValue(['LongestKillShot']);
        const lootEXP = getStatValue(['ExpLooting']);

        // If there's 0 hits it wouldn't appear in SessionCounters
        let lastHits = getStatValue(['HitCount']);
        if (!lastHits || lastHits <= 0) {
            lastHits = 0;
        }

        // Perform this abomination to get damage without FLOATING GHOST NUMBERS (thanks BSG)
        const modDamage = damage.toString();
        const modLongestShot = longestShot.toString();
        const totalLongestShot = parseInt(modLongestShot.slice(0, -2), 10);
        const totalDamage = parseInt(modDamage.slice(0, -2), 10);

        // Get max PMC health
        const totalMaxHealth = Object.values(staticProfile.characters.pmc.Health.BodyParts)
            .reduce((sum, bodyPart) => sum + (bodyPart.Health?.Maximum || 0), 0);

        // Barebones of data
        // Would recommend creating an interface for this so its not anyonmous - CJ
        // See `ISptProfile` for an example of an interface
        const baseData = {
            accountType: profile.Info.GameVersion,
            health: totalMaxHealth,
            id: staticProfile.info.id,
            isScav: isScavRaid,
            lastPlayed: profile.Stats.Eft.LastSessionDate,
            modINT: this.sptLeaderboard.key_size,
            mods: combinedModData,
            name: profileName,
            pmcHealth: totalMaxHealth,
            pmcLevel: pmcLevel,
            raidKills: kills,
            raidResult: raidEndResult,
            raidTime: this.sptLeaderboard.playTime,
            sptVer: this.coreConfig.sptVersion,
            teamTag: config.profile_teamTag,
            token: this.sptLeaderboard.uniqueToken,
            DBinINV: this.sptLeaderboard.DBinINV,
            isCasual: config.mod_casualMode
        }

        // Public SCAV raid
        // Would recommend creating an interface for this so its not anyonmous - CJ
        if (config.public_profile && isScavRaid) {
            return {
                ...baseData,
                discFromRaid: discFromRaid,
                isTransition: isTransition,
                isUsingStattrack: this.sptLeaderboard.isUsingStattrack,
                lastRaidEXP: lootEXP,
                lastRaidHits: lastHits,
                lastRaidMap: this.sptLeaderboard.lastRaidMap,
                lastRaidMapRaw: this.sptLeaderboard.lastRaidMapRaw,
                lastRaidTransitionTo: lastRaidTransitionTo,
                allAchievements: staticProfile.characters.pmc.Achievements,
                longestShot: totalLongestShot,
                modWeaponStats: this.sptLeaderboard.modWeaponStats,
                playedAs: "SCAV",
                pmcSide: staticProfile.characters.pmc.Info.Side,
                prestige: staticProfile.characters.pmc.Info.PrestigeLevel,
                profileAboutMe: config.profile_aboutMe,
                profilePicture: config.profile_profilePicture,
                profileTheme: config.profile_profileTheme,
                publicProfile: true,
                raidDamage: totalDamage,
                registrationDate: profile.Info.RegistrationDate,
                scavLevel: scavLevel,
                traderInfo: this.sptLeaderboard.tradersInfo
            }
            // Public PMC Raid
        } else if (config.public_profile && !isScavRaid) {
            return {
                ...baseData,
                discFromRaid: discFromRaid,
                isTransition: isTransition,
                isUsingStattrack: this.sptLeaderboard.isUsingStattrack,
                lastRaidEXP: lootEXP,
                lastRaidHits: lastHits,
                lastRaidMap: this.sptLeaderboard.lastRaidMap,
                lastRaidMapRaw: this.sptLeaderboard.lastRaidMapRaw,
                lastRaidTransitionTo: lastRaidTransitionTo,
                allAchievements: staticProfile.characters.pmc.Achievements,
                longestShot: totalLongestShot,
                modWeaponStats: this.sptLeaderboard.modWeaponStats,
                playedAs: "PMC",
                pmcSide: staticProfile.characters.pmc.Info.Side,
                prestige: staticProfile.characters.pmc.Info.PrestigeLevel,
                profileAboutMe: config.profile_aboutMe,
                profilePicture: config.profile_profilePicture,
                profileTheme: config.profile_profileTheme,
                publicProfile: true,
                raidDamage: totalDamage,
                registrationDate: profile.Info.RegistrationDate,
                scavLevel: scavLevel,
                traderInfo: this.sptLeaderboard.tradersInfo
            }
        } else {
            // Private profile raid
            return {
                ...baseData,
                publicProfile: false
            }
        }
    }
}