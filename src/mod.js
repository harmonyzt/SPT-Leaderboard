"use strict";
const { connect } = require('http2');
const https = require('https');
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { debug } = require('console');

class SPTLeaderboard {
    constructor() {
        this.retriesCount = 0;
        this.connectivity = 1;
        this.key_size = 0;
        this.TOKEN_FILE = path.join(__dirname, 'secret.token');
        this.uniqueToken = this.loadOrCreateToken();
        this.CFG = require("../config/config");
        this.PHP_ENDPOINT = this.CFG.PHP_ENDPOINT || "visuals.nullcore.net";
        this.PHP_PATH = this.CFG.PHP_PATH || "/SPT/api/v1/main.php";
        this.localeData;
        this.raidResult = "Died";
        this.playTime = 0;
        this.staticProfile;
        this.serverMods;
        this.transitionMap;
        this.lastRaidMap;
        this.lastRaidMapRaw;
        this.isUsingStattrack = false;
        this.modWeaponStats = 0;
        this.hasKappa = false;
        this.DBinINV = false;
        // Traders
        this.tradersInfo = {};
        this.traderMap = {
            "6617beeaa9cfa777ca915b7c": "REF",
            "54cb50c76803fa8b248b4571": "PRAPOR",
            "54cb57776803fa99248b456e": "THERAPIST",
            "579dc571d53a0658a154fbec": "FENCE",
            "58330581ace78e27b8b10cee": "SKIER",
            "5935c25fb3acc3127c3d8cd9": "PEACEKEEPER",
            "5a7c2eca46aef81a7ca2145d": "MECHANIC",
            "5ac3b934156ae10c4430e83c": "RAGMAN",
            "638f541a29ffd1183d187f57": "LIGHTKEEPER",
            "656f0f98d80a697f855d34b1": "BTR_DRIVER",
            "5c0647fdd443bc2504c2d371": "JAEGER"
        };
    }

    loadOrCreateToken() {
        try {
            // If token exists
            if (fs.existsSync(this.TOKEN_FILE)) {
                console.log(`[SPT Leaderboard] Your secret token was initialized by the mod. Remember to never show it to anyone!`);
                return fs.readFileSync(this.TOKEN_FILE, 'utf8').trim();
            } else {
                // If it doesn't
                const newToken = crypto.randomBytes(32).toString('hex');
                fs.writeFileSync(this.TOKEN_FILE, newToken, 'utf8');
                console.log(`[SPT Leaderboard] Generated your secret token, see mod directory. WARNING: DO NOT SHARE IT WITH ANYONE! If you lose it, you will lose access to the Leaderboard until next season!`);
                return newToken;
            }
        } catch (e) {
            console.error(`[SPT Leaderboard] Error handling token file: ${e.message}`);
            // Generating new token in case of an error
            return crypto.randomBytes(32).toString('hex');
        }
    }

    loadLocales() {
        const localePath = path.join(__dirname, "..", "temp", "locale.json");
        const localeFileContent = fs.readFileSync(localePath, "utf-8");
        this.localeData = JSON.parse(localeFileContent);

        if (this.CFG.DEBUG)
            console.info("[SPT Leaderboard] Loaded locale file successfully!");
    }

    getLocaleName(id, additionalKey) {
        if (!this.localeData) {
            console.info("[SPT Leaderboard] Locale data not loaded!");
            return "Unknown";
        }

        // if given additionalKey, try to find it
        if (additionalKey) {
            const combinedKey = `${id} ${additionalKey}`;
            if (this.localeData[combinedKey]) {
                return this.localeData[combinedKey];
            }
        }

        // Return default Id if not found
        return this.localeData[id] || "Unknown";
    }

    getAllValidWeapons(sessionId, info) {
        if (!info[sessionId]) {
            return null;
        }

        const result = {
            [sessionId]: {}
        };

        // Process all weapons for this session ID
        for (const [weaponId, weaponStats] of Object.entries(info[sessionId])) {
            const weaponName = this.getLocaleName(weaponId, "ShortName");

            // Skip weapons with unknown names or tpl ids
            if (weaponName === "Unknown") {
                continue;
            }

            // Add valid weapon to the result
            result[sessionId][weaponName] = {
                stats: weaponStats,
                originalId: weaponId
            };
        }

        // If no valid weapons found, return null or empty object
        if (Object.keys(result[sessionId]).length === 0) {
            return null;
        }

        return result;
    }

    preSptLoad(container) {
        const config = this.CFG;
        const logger = container.resolve("WinstonLogger");
        const RouterService = container.resolve("StaticRouterModService");
        const profileHelper = container.resolve("ProfileHelper");
        const mailService = container.resolve("MailSendService");

        // Store cache for heartbeats + throttling (sessionId: timestamp)
        const PlayerState = {
            ONLINE: 'online',
            IN_MENU: 'in_menu',
            IN_RAID: 'in_raid',
            IN_STASH: 'in_stash'
        };
        const stateCache = new Map();
        const HEARTBEAT_THROTTLE_MS = 10 * 1000;

        // Load locale file
        this.loadLocales();

        function calculateFileHash(filePath) {
            const fileBuffer = fs.readFileSync(filePath);
            const hashSum = crypto.createHash('sha256');
            hashSum.update(fileBuffer);
            return hashSum.digest('hex');
        }

        // Paths
        const modPath = path.join(__dirname, 'mod.js');
        const modBasePath = path.resolve(__dirname, '..', '..');
        const sptRoot = path.resolve(modBasePath, '..', '..');
        const userModsPath = path.join(sptRoot, 'user', 'mods');
        const bepinexPluginsPath = path.join(sptRoot, 'BepInEx', 'plugins');

        const modHash = calculateFileHash(modPath);
        this.key_size = modHash;

        // Mod data
        function getDirectories(dirPath) {
            if (!fs.existsSync(dirPath)) return [];
            return fs.readdirSync(dirPath, { withFileTypes: true })
                .filter(entry => entry.isDirectory())
                .map(dir => dir.name);
        }

        function getDllFiles(dirPath) {
            if (!fs.existsSync(dirPath)) return [];
            return fs.readdirSync(dirPath, { withFileTypes: true })
                .filter(entry => entry.isFile() && entry.name.endsWith('.dll'))
                .map(file => file.name);
        }

        function collectModData() {
            return {
                userMods: getDirectories(userModsPath),
                bepinexMods: getDirectories(bepinexPluginsPath),
                bepinexDlls: getDllFiles(bepinexPluginsPath)
            };
        }

        async function sendHeartbeat(sessionId, output) {
            if (!stateCache.has(sessionId)) return;

            const cachedData = stateCache.get(sessionId);
            const timeSinceLast = Date.now() - cachedData.lastSentTime;

            // Throttle for heartbeats
            if (timeSinceLast < HEARTBEAT_THROTTLE_MS) {
                if (config.DEBUG) {
                    logger.info(`[SPT Leaderboard] Skipping Heartbeat: session ${sessionId} was updated ${timeSinceLast} ms ago`);
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
                    logger.info(`[SPT Leaderboard] Sent ${cachedData.state} heartbeat:`, result);
                }

                // Update last time sent
                stateCache.set(sessionId, {
                    ...cachedData,
                    lastSentTime: Date.now()
                });

            } catch (error) {
                console.error(`[SPT Leaderboard] Error sending ${cachedData.state} heartbeat:`, error.message);
                return output;
            }
        }

        const modData = collectModData();

        // Define SPT version
        var configServer = container.resolve("ConfigServer");
        var coreConfig = configServer.getConfig("spt-core");
        var sptVersion = coreConfig.sptVersion;

        // Get nice map name from serverId
        function getPrettyMapName(entry) {
            const mapAliases = {
                "bigmap": "Customs",
                "factory4_day": "Factory",
                "factory4_night": "Night Factory",
                "interchange": "Interchange",
                "laboratory": "Labs",
                "RezervBase": "Reserve",
                "shoreline": "Shoreline",
                "woods": "Woods",
                "lighthouse": "Lighthouse",
                "TarkovStreets": "Streets of Tarkov",
                "Sandbox": "Ground Zero - Low",
                "Sandbox_high": "Ground Zero - High"
            };

            const rawMapName = entry.split('.')[0];

            rawMapName.toLowerCase();

            return mapAliases[rawMapName] || rawMapName; // returning raw if not found
        }

        // Stattrack Mod Support
        const statTrackPath = path.join(sptRoot, 'user', 'mods/acidphantasm-stattrack');
        if (config.enable_mod_support && fs.existsSync(statTrackPath)) {
            this.isUsingStattrack = true;

            RouterService.registerStaticRouter("SPTLBStattrackSupport", [{
                url: "/stattrack/save",
                action: async (url, info, sessionId, output) => {

                    this.modWeaponStats = this.getAllValidWeapons(sessionId, info);

                    return output;
                }
            }], "aki");
        } else {
            // No mod detected - set to 0 
            this.modWeaponStats = 0;
            this.isUsingStattrack = false;
        }

        RouterService.registerStaticRouter("SPTLBProfileRaidEnd", [{
            url: "/client/match/local/end",
            action: async (url, info, sessionId, output) => {

                if (!sessionId)
                    return output

                if (config.public_profile) {
                    sendHeartbeat('raid_end', { sessionId: sessionId }, output);
                }

                this.staticProfile = profileHelper.getFullProfile(sessionId);
                this.serverMods = this.staticProfile.spt.mods.map(mod => mod.name).join(', ');

                await gatherProfileInfo(info, logger, sptVersion);

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBHeartBeatOnline", [{
            url: "/launcher/profile/info",
            action: (url, info, sessionId, output) => {
                if (!sessionId || !config.public_profile) return output;

                const currentState = stateCache.get(sessionId);

                // No state for profile at all - send ONLINE
                if (!currentState) {
                    stateCache.set(sessionId, {
                        state: PlayerState.ONLINE,
                        lastSentTime: 0
                    });
                }

                // Always send the heartbeat
                sendHeartbeat(sessionId, output);

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBHeartBeatInMenu", [{
            url: "/client/globals",
            action: (url, info, sessionId, output) => {
                if (sessionId && config.public_profile) {
                    stateCache.set(sessionId, {
                        state: PlayerState.IN_MENU,
                        lastSentTime: stateCache.get(sessionId)?.lastSentTime || 0
                    });
                }

                if (sessionId) {
                    checkInbox(sessionId);
                }

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBHeartBeatRaidStart", [{
            url: "/client/match/local/start",
            action: (url, info, sessionId, output) => {
                if (sessionId && config.public_profile) {
                    stateCache.set(sessionId, {
                        state: PlayerState.IN_RAID,
                        lastSentTime: stateCache.get(sessionId)?.lastSentTime || 0
                    });
                    sendHeartbeat(sessionId, output); // Send instantly when entering raid
                }
                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBHeartBeatInStash", [{
            url: "/client/game/profile/items/moving",
            action: async (url, info, sessionId, output) => {
                if (sessionId && config.public_profile) {
                    stateCache.set(sessionId, {
                        state: PlayerState.IN_STASH,
                        lastSentTime: stateCache.get(sessionId)?.lastSentTime || 0
                    });
                }
                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBHeartBeatLogout", [{
            url: "/client/game/logout",
            action: async (url, info, sessionId, output) => {
                if (sessionId && config.public_profile) {
                    stateCache.set(sessionId, {
                        state: PlayerState.IN_STASH,
                        lastSentTime: stateCache.get(sessionId)?.lastSentTime || 0
                    });
                }
                return output;
            }
        }], "aki");

        //const kappaId = "664f1f8768508d74604bf556";

        const gatherProfileInfo = async (data, logger, version) => {
            const jsonData = JSON.parse(JSON.stringify(data));
            const fullProfile = jsonData.results.profile;

            if (config.DEBUG) {
                logger.info(JSON.stringify(jsonData, null, 2));
                logger.log("[SPT Leaderboard] Data above was saved in server log file.", "green");
            }

            this.lastRaidMap = getPrettyMapName(jsonData.serverId);
            this.lastRaidMapRaw = jsonData.serverId.split('.')[0];

            // Get the result of a raid (Died/Survived/Runner)
            this.raidResult = jsonData.results.result;
            this.playTime = jsonData.results.playTime;
            this.transitionMap = jsonData?.locationTransit?.location ?? "None";

            try {
                if (this.connectivity == 0) return;

                logger.log(`[SPT Leaderboard] Getting ready to send statistics...`, "cyan");

                if (isProfileValid(fullProfile, logger)) {
                    await processAndSendProfile(fullProfile, logger, version);
                } else {
                    // Time out and retry next raid
                    if (this.retriesCount <= config.connectionRetries) {
                        this.retriesCount += 1;
                    } else {
                        logger.error(`[SPT Leaderboard] Could not establish internet connection with PHP or your profile does not match requirements. Pausing mod until next SPT Server start...`, "red");
                        this.connectivity = 0;

                        return;
                    }
                }
            } catch (e) {
                logger.info(`[SPT Leaderboard] Error: ${e.message}`);
            }
        }

        // Calculate stats from profile
        const processAndSendProfile = async (profile, logger, version) => {
            const profileData = await processProfile(profile, version);
            const config = this.CFG;

            if (config.DEBUG)
                logger.info(`[SPT Leaderboard] Data ready!`);

            try {
                await sendProfileData(profileData);

                logger.info("[SPT Leaderboard] Data sent to the leaderboard successfully!");
            } catch (e) {
                logger.info(`[SPT Leaderboard] Could not send data to leaderboard: ${e.message}`);
            }
        }

        const processProfile = async (profile, versionSPT) => {
            const getStatValue = (keys) => {
                const item = profile.Stats.Eft.SessionCounters.Items?.find(item =>
                    item.Key && keys.every((k, i) => item.Key[i] === k)
                );
                return item?.Value || 0;
            };

            const getGlobalStatValue = (keys) => {
                const item = profile.Stats.Eft.OverallCounters.Items?.find(item =>
                    item.Key && keys.every((k, i) => item.Key[i] === k)
                );
                return item?.Value || 0;
            };

            const config = this.CFG;

            // MAIN
            let scavLevel = this.staticProfile.characters.scav.Info.Level;
            let pmcLevel = this.staticProfile.characters.pmc.Info.Level;
            let profileName = "default_name";
            const kills = getStatValue(['KilledPmc']);
            const raidEndResult = this.raidResult;
            const combinedModData = modData + this.serverMods;
            const isScavRaid = profile.Info.Side === "Savage";

            if (config.public_profile && config.profile_customName?.trim()) {
                // public profile
                // profile_customName exists
                profileName = config.profile_customName;
            } else {
                profileName = this.staticProfile.characters.pmc.Info.Nickname;
            }

            // If left the raid
            let discFromRaid = false;
            if (raidEndResult === "Left") {
                discFromRaid = true;
            }

            // For transit
            let isTransition = false;
            let lastRaidTransitionTo = "None";

            if (raidEndResult === "Transit" && this.transitionMap !== "None") {
                isTransition = true;
                lastRaidTransitionTo = this.transitionMap;
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
            const totalMaxHealth = Object.values(this.staticProfile.characters.pmc.Health.BodyParts)
                .reduce((sum, bodyPart) => sum + (bodyPart.Health?.Maximum || 0), 0);

            // Barebones of data
            const baseData = {
                accountType: profile.Info.GameVersion,
                health: totalMaxHealth,
                id: this.staticProfile.info.id,
                isScav: isScavRaid,
                lastPlayed: profile.Stats.Eft.LastSessionDate,
                modINT: this.key_size,
                mods: combinedModData,
                name: profileName,
                pmcHealth: totalMaxHealth,
                pmcLevel: pmcLevel,
                raidKills: kills,
                raidResult: raidEndResult,
                raidTime: this.playTime,
                sptVer: versionSPT,
                token: this.uniqueToken,
                DBinINV: this.DBinINV,
                isCasual: config.mod_casualMode
            }

            // Public SCAV raid
            if (config.public_profile && isScavRaid) {
                return {
                    ...baseData,
                    discFromRaid: discFromRaid,
                    isTransition: isTransition,
                    isUsingStattrack: this.isUsingStattrack,
                    lastRaidEXP: lootEXP,
                    lastRaidHits: lastHits,
                    lastRaidMap: this.lastRaidMap,
                    lastRaidMapRaw: this.lastRaidMapRaw,
                    lastRaidTransitionTo: lastRaidTransitionTo,
                    allAchievements: this.staticProfile.characters.pmc.Achievements,
                    longestShot: totalLongestShot,
                    modWeaponStats: this.modWeaponStats,
                    playedAs: "SCAV",
                    pmcSide: this.staticProfile.characters.pmc.Info.Side,
                    prestige: this.staticProfile.characters.pmc.Info.PrestigeLevel,
                    profileAboutMe: config.profile_aboutMe,
                    profilePicture: config.profile_profilePicture,
                    profileTheme: config.profile_profileTheme,
                    publicProfile: true,
                    raidDamage: totalDamage,
                    registrationDate: profile.Info.RegistrationDate,
                    scavLevel: scavLevel,
                    traderInfo: this.tradersInfo
                }
                // Public PMC Raid
            } else if (config.public_profile && !isScavRaid) {
                return {
                    ...baseData,
                    discFromRaid: discFromRaid,
                    isTransition: isTransition,
                    isUsingStattrack: this.isUsingStattrack,
                    lastRaidEXP: lootEXP,
                    lastRaidHits: lastHits,
                    lastRaidMap: this.lastRaidMap,
                    lastRaidMapRaw: this.lastRaidMapRaw,
                    lastRaidTransitionTo: lastRaidTransitionTo,
                    allAchievements: this.staticProfile.characters.pmc.Achievements,
                    longestShot: totalLongestShot,
                    modWeaponStats: this.modWeaponStats,
                    playedAs: "PMC",
                    pmcSide: this.staticProfile.characters.pmc.Info.Side,
                    prestige: this.staticProfile.characters.pmc.Info.PrestigeLevel,
                    profileAboutMe: config.profile_aboutMe,
                    profilePicture: config.profile_profilePicture,
                    profileTheme: config.profile_profileTheme,
                    publicProfile: true,
                    raidDamage: totalDamage,
                    registrationDate: profile.Info.RegistrationDate,
                    scavLevel: scavLevel,
                    traderInfo: this.tradersInfo
                }
            } else {
                // Private profile raid
                return {
                    ...baseData,
                    publicProfile: false
                }
            }
        }

        // Send the data
        const sendProfileData = async (data) => {
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

        // Util
        const isProfileValid = (profile, logger) => {
            if (!profile?.Info) {
                logger.info("[SPT Leaderboard] Invalid profile structure.");
                return false;
            }

            return true;
        }
    }
}

module.exports = { mod: new SPTLeaderboard() };