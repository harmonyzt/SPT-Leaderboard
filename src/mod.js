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
        this.PHP_PATH = this.CFG.PHP_PATH || "/hidden/SPT_Profiles_Backend.php";
        this.localeData;
        this.raidResult = "Died";
        this.playTime = 0;
        this.staticProfile;
        this.serverMods;
        this.transitionMap;
        this.lastRaidMap;
        this.lastRaidMapRaw;
        this.mostRecentAchievementTimestamp = 0;
        this.mostRecentAchievementImageUrl = null;
        this.mostRecentAchievementName = null;
        this.mostRecentAchievementDescription = null;
        this.masteryWeaponId = 0;
        this.masteryWeaponProgress;
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
        const logger = container.resolve("WinstonLogger");
        const RouterService = container.resolve("StaticRouterModService");
        const profileHelper = container.resolve("ProfileHelper");

        // Cache for heartbeats + 5 sec time out (sessionId: timestamp)
        const heartbeatCache = new Map();
        const HEARTBEAT_THROTTLE_MS = 5 * 1000;

        const config = this.CFG;

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
            return [
                ...getDirectories(userModsPath),
                ...getDirectories(bepinexPluginsPath),
                ...getDllFiles(bepinexPluginsPath)
            ];
        }

        async function sendHeartbeat(type, extraData = {}) {
            const { sessionId } = extraData;

            if (sessionId && heartbeatCache.has(sessionId)) {
                const cachedData = heartbeatCache.get(sessionId);
                const timeSinceLast = Date.now() - cachedData.lastSentTime;

                // Throttle
                if (timeSinceLast < HEARTBEAT_THROTTLE_MS) {
                    console.log(`[SPT Leaderboard] Skipping Heartbeat (${type}): heartbeat for sessionId ${sessionId} was already sent ${timeSinceLast} ms ago`);
                    return null;
                }

                // Skipping online heartbeat
                if (type === 'online' && cachedData.lastRaidState) {
                    console.log(`[SPT Leaderboard] Skipping Online Heartbeat: player ${sessionId} is in raid`);
                    return null;
                }
            }

            // Clear cache
            if (sessionId) {
                const cacheData = {
                    lastSentTime: Date.now(),
                    lastRaidState: type === 'in_raid' || type === 'raid_start'
                        ? true
                        : type === 'raid_end'
                            ? false
                            : heartbeatCache.get(sessionId)?.lastRaidState || false
                };
                heartbeatCache.set(sessionId, cacheData);
            }

            try {
                const response = await fetch('https://visuals.nullcore.net/SPT/api/heartbeat/v1.php', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        type,
                        timestamp: Date.now(),
                        ...extraData
                    })
                });

                const result = await response.json();

                if (config.DEBUG)
                    console.log(`[SPT Leaderboard] Sent heartbeat ${type} to the API:`, result);

            } catch (error) {
                console.error(`[SPT Leaderboard] Error sending heartbeat ${type} to API:`, error.message);
            }
        }

        const modData = collectModData();

        // Define SPT version
        var configServer = container.resolve("ConfigServer");
        var coreConfig = configServer.getConfig("spt-core");
        var sptVersion = coreConfig.sptVersion;

        // Find map name from serverId
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

        // STATTRACK mod support
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
            // No mod(s) detected - set to 0 
            this.modWeaponStats = 0;
            this.isUsingStattrack = false;
        }

        RouterService.registerStaticRouter("SPTLBProfileRaidEnd", [{
            url: "/client/match/local/end",
            action: async (url, info, sessionId, output) => {

                sendHeartbeat('raid_end', { sessionId: sessionId });

                this.staticProfile = profileHelper.getFullProfile(sessionId);
                this.serverMods = this.staticProfile.spt.mods.map(mod => mod.name).join(', ');

                await gatherProfileInfo(info, logger, sptVersion);

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBHeartBeatOnline", [{
            url: "/launcher/profile/info",
            action: async (url, info, sessionId, output) => {

                if (!sessionId)
                    return output

                sendHeartbeat('online', { sessionId: sessionId });

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBHeartBeatInMenu", [{
            url: "/singleplayer/log",
            action: async (url, info, sessionId, output) => {
                if (!sessionId)
                    return output

                sendHeartbeat('in_menu', { sessionId: sessionId });

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBHeartBeatRaidStart", [{
            url: "/client/match/local/start",
            action: async (url, info, sessionId, output) => {
                if (!sessionId)
                    return output

                sendHeartbeat('raid_start', { sessionId: sessionId });

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBProfileAchievements", [{
            url: "/client/achievement/list",
            action: async (url, info, sessionId, output) => {
                this.staticProfile = profileHelper.getFullProfile(sessionId);

                const allAchievements = JSON.parse(output);

                let latestAchievement = null;
                let latestTimestamp = 0;

                // Finding the most recent achivement
                if (this.staticProfile?.characters.pmc.Achievements) {
                    const kappaId = "664f1f8768508d74604bf556";

                    for (const [achievementId, timestamp] of Object.entries(this.staticProfile.characters.pmc.Achievements)) {
                        // Check for kappa achievement id
                        if (achievementId === kappaId) {
                            this.hasKappa = true;
                        }

                        if (timestamp > latestTimestamp) {
                            latestTimestamp = timestamp;
                            latestAchievement = achievementId;
                        }
                    }
                }

                // Get the image URL and name/description from locale file
                if (latestAchievement) {
                    let achievementImageUrl = "";
                    const achievementElements = allAchievements?.data?.elements || [];
                    const achievementData = achievementElements.find(el => el?.id === latestAchievement);

                    if (achievementData) {
                        achievementImageUrl = achievementData.imageUrl || "";
                    }

                    let achievementName = "";
                    let achievementDescription = "";

                    try {
                        achievementName = this.getLocaleName(latestAchievement, 'name') || "";
                        achievementDescription = this.getLocaleName(latestAchievement, 'description') || "";
                    } catch (e) {
                        logger.error(`[SPT Leaderboard] Failed to assign achievement: ${e.message}`);
                    }

                    this.mostRecentAchievementTimestamp = latestTimestamp;
                    this.mostRecentAchievementImageUrl = achievementImageUrl;
                    this.mostRecentAchievementName = achievementName;
                    this.mostRecentAchievementDescription = achievementDescription;

                } else {
                    if (config.DEBUG)
                        logger.info("[SPT Leaderboard] No achievements found in profile. Skipping...");
                }

                // Trader Info
                if (!this.staticProfile?.characters.pmc.TradersInfo) {
                    logger.info("[SPT Leaderboard] TradersInfo not found in profile!");
                    return;
                }

                const tradersData = this.staticProfile.characters.pmc.TradersInfo;

                // create new object for traders to easily navigate on frontend
                for (const [traderId, traderName] of Object.entries(this.traderMap)) {
                    if (tradersData[traderId]) {
                        this.tradersInfo[traderName] = {
                            id: traderId,  // saving id in case we need it for later
                            salesSum: tradersData[traderId].salesSum,
                            unlocked: tradersData[traderId].unlocked,
                            standing: tradersData[traderId].standing,
                            loyaltyLevel: tradersData[traderId].loyaltyLevel,
                            disabled: tradersData[traderId].disabled
                        };
                    } else {
                        this.tradersInfo[traderName] = {
                            id: traderId,
                            salesSum: 0,
                            unlocked: false,
                            standing: 0,
                            loyaltyLevel: 0,
                            disabled: true,
                            notFound: true
                        };
                    }
                }

                this.DBinINV = this.staticProfile.characters.pmc.Inventory.items.some(item => item._tpl === "58ac60eb86f77401897560ff");

                return output;
            }
        }], "aki");

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
            const curWinStreak = getGlobalStatValue(['CurrentWinStreak', 'Pmc']);
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
                teamTag: config.profile_teamTag,
                token: this.uniqueToken,
                DBinINV: this.DBinINV,
                isCasual: config.mod_casualMode
            }

            // Public SCAV raid
            if (config.public_profile && isScavRaid) {
                return {
                    ...baseData,
                    bp_cardbg: config.bp_backgroundReward,
                    bp_cat: config.bp_catReward,
                    bp_decal: config.bp_decal,
                    bp_mainbg: config.bp_mainBackgroundReward,
                    bp_pfpbordercolor: config.bp_pfpBorder,
                    bp_pfpstyle: config.bp_pfpStyle,
                    bp_prestigebg: config.bp_usePrestigeBackground,
                    discFromRaid: discFromRaid,
                    hasKappa: this.hasKappa,
                    isTransition: isTransition,
                    isUsingStattrack: this.isUsingStattrack,
                    lastRaidEXP: lootEXP,
                    lastRaidHits: lastHits,
                    lastRaidMap: this.lastRaidMap,
                    lastRaidMapRaw: this.lastRaidMapRaw,
                    lastRaidTransitionTo: lastRaidTransitionTo,
                    allAchievements: this.staticProfile.characters.pmc.Achievements,
                    latestAchievementDescription: this.mostRecentAchievementDescription,
                    latestAchievementImageUrl: this.mostRecentAchievementImageUrl,
                    latestAchievementName: this.mostRecentAchievementName,
                    latestAchievementTimestamp: this.mostRecentAchievementTimestamp,
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
                    traderInfo: this.tradersInfo,
                    usePrestigeStyling: config.profile_usePrestigeStyling,
                    weaponMasteryId: this.masteryWeaponId,
                    weaponMasteryProgress: this.masteryWeaponProgress,
                    winRaidStreak: curWinStreak
                }
                // Public PMC Raid
            } else if (config.public_profile && !isScavRaid) {
                return {
                    ...baseData,
                    bp_cardbg: config.bp_backgroundReward,
                    bp_cat: config.bp_catReward,
                    bp_decal: config.bp_decal,
                    bp_mainbg: config.bp_mainBackgroundReward,
                    bp_pfpbordercolor: config.bp_pfpBorder,
                    bp_pfpstyle: config.bp_pfpStyle,
                    bp_prestigebg: config.bp_usePrestigeBackground,
                    discFromRaid: discFromRaid,
                    hasKappa: this.hasKappa,
                    isTransition: isTransition,
                    isUsingStattrack: this.isUsingStattrack,
                    lastRaidEXP: lootEXP,
                    lastRaidHits: lastHits,
                    lastRaidMap: this.lastRaidMap,
                    lastRaidMapRaw: this.lastRaidMapRaw,
                    lastRaidTransitionTo: lastRaidTransitionTo,
                    allAchievements: this.staticProfile.characters.pmc.Achievements,
                    latestAchievementDescription: this.mostRecentAchievementDescription,
                    latestAchievementImageUrl: this.mostRecentAchievementImageUrl,
                    latestAchievementName: this.mostRecentAchievementName,
                    latestAchievementTimestamp: this.mostRecentAchievementTimestamp,
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
                    traderInfo: this.tradersInfo,
                    usePrestigeStyling: config.profile_usePrestigeStyling,
                    weaponMasteryId: this.masteryWeaponId,
                    weaponMasteryProgress: this.masteryWeaponProgress,
                    winRaidStreak: curWinStreak
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