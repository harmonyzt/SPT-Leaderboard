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
        this.mostRecentAchievementTimestamp = 0;
        this.mostRecentAchievementImageUrl = null;
        this.mostRecentAchievementName = null;
        this.mostRecentAchievementDescription = null;
        this.masteryWeaponId = 0;
        this.masteryWeaponProgress;
        this.isUsingStattrack = false;
        this.modWeaponStats = 0;
        this.hasKappa = false;

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
                console.log(`[SPT Leaderboard] Generated your secret token, see mod directory. WARNING: DO NOT SHARE IT WITH ANYONE! If you lose it, you will lose access to the Leaderboard!`);
                return newToken;
            }
        } catch (e) {
            console.error(`[SPT Leaderboard] Error handling token file: ${e.message}`);
            // Generating new token in case
            return crypto.randomBytes(32).toString('hex');
        }
    }

    loadLocales() {
        const localePath = path.join(__dirname, "..", "temp", "locale.json");
        const localeFileContent = fs.readFileSync(localePath, "utf-8");
        this.localeData = JSON.parse(localeFileContent);

        if (this.CFG.DEBUG) {
            console.info("[SPT Leaderboard] Loaded locale file successfully!");
        }
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

    getBestWeapon(sessionId, info) {
        if (!info[sessionId]) {
            return null;
        }

        // Get all weapons sorted by kills (descending)
        const weapons = Object.entries(info[sessionId])
            .sort((a, b) => b[1].kills - a[1].kills);

        if (weapons.length === 0) {
            return { bestWeapon: null };
        }

        // Find first weapon with existing locale name
        for (const [weaponId, weaponStats] of weapons) {
            const weaponName = this.getLocaleName(weaponId, "ShortName");

            // If we found weapon with valid name that exist in locales
            if (weaponName !== "Unknown") {
                return {
                    bestWeapon: {
                        name: weaponName,
                        stats: weaponStats,
                        originalId: weaponId
                    }
                };
            }
        }

        // If no weapons with valid names found, return the first one with "Unknown" name
        return {
            bestWeapon: {
                name: "Unknown Weapon",
                stats: weapons[0][1],
                originalId: weapons[0][0]
            }
        };
    }

    preSptLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const RouterService = container.resolve("StaticRouterModService");
        const profileHelper = container.resolve("ProfileHelper");
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
                "rezervbase": "Reserve",
                "shoreline": "Shoreline",
                "woods": "Woods",
                "lighthouse": "Lighthouse",
                "tarkovstreets": "Streets of Tarkov",
                "sandbox": "Ground Zero - Low",
                "sandbox_high": "Ground Zero - High"
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

                    this.modWeaponStats = this.getBestWeapon(sessionId, info);

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

                this.staticProfile = profileHelper.getFullProfile(sessionId);
                this.serverMods = this.staticProfile.spt.mods.map(mod => mod.name).join(', ');
                
                await gatherProfileInfo(info, logger, sptVersion);

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
                        // Check for kappa ach id
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
                        logger.error(`Failed to assign achievement: ${e.message}`);
                    }

                    this.mostRecentAchievementTimestamp = latestTimestamp;
                    this.mostRecentAchievementImageUrl = achievementImageUrl;
                    this.mostRecentAchievementName = achievementName;
                    this.mostRecentAchievementDescription = achievementDescription;

                } else {
                    if (config.DEBUG)
                        logger.info("No achievements found in profile. Skipping...");
                }


                // Trader Info
                if (!this.staticProfile?.characters.pmc.TradersInfo) {
                    console.info("TradersInfo not found in profile!");
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

                return output;
            }
        }], "aki");

        const gatherProfileInfo = async (data, logger, version) => {

            const jsonData = JSON.parse(JSON.stringify(data));
            const fullProfile = jsonData.results.profile;

            if (config.DEBUG) {
                logger.info(JSON.stringify(jsonData, null, 2));
                logger.log("Data above was saved in server log file.", "green");
            }

            this.lastRaidMap = getPrettyMapName(jsonData.serverId);

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

                if (config.DEBUG)
                    logger.info("[SPT Leaderboard] Data sent to the server successfully!");
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
                token: this.uniqueToken,
                id: this.staticProfile.info.id,
                modINT: this.key_size,
                mods: combinedModData,
                pmcLevel: pmcLevel,
                pmcHealth: totalMaxHealth,
                name: profileName,
                health: totalMaxHealth,
                raidKills: kills,
                disqualified: false,
                sptVer: versionSPT,
                raidResult: raidEndResult,
                raidTime: this.playTime,
                lastPlayed: profile.Stats.Eft.LastSessionDate,
                isScav: isScavRaid,
                accountType: profile.Info.GameVersion
            }

            // Public SCAV raid (can't be otherwise)
            if (config.public_profile && isScavRaid) {
                return {
                    ...baseData,
                    publicProfile: true,
                    raidDamage: totalDamage,
                    longestShot: totalLongestShot,
                    playedAs: "SCAV",
                    pmcSide: this.staticProfile.characters.pmc.Info.Side,
                    scavLevel: scavLevel,
                    winRaidStreak: curWinStreak,
                    profileAboutMe: config.profile_aboutMe,
                    profilePicture: config.profile_profilePicture,
                    profileTheme: config.profile_profileTheme,
                    registrationDate: profile.Info.RegistrationDate,
                    lastRaidMap: this.lastRaidMap,
                    lastRaidEXP: lootEXP,
                    lastRaidHits: lastHits,
                    isTransition: isTransition,
                    lastRaidTransitionTo: lastRaidTransitionTo,
                    discFromRaid: discFromRaid,
                    prestige: this.staticProfile.characters.pmc.Info.PrestigeLevel,
                    usePrestigeStyling: config.profile_usePrestigeStyling,
                    latestAchievementName: this.mostRecentAchievementName,
                    latestAchievementDescription: this.mostRecentAchievementDescription,
                    latestAchievementImageUrl: this.mostRecentAchievementImageUrl,
                    latestAchievementTimestamp: this.mostRecentAchievementTimestamp,
                    hasKappa: this.hasKappa,
                    weaponMasteryId: this.masteryWeaponId,
                    weaponMasteryProgress: this.masteryWeaponProgress,
                    isUsingStattrack: this.isUsingStattrack,
                    modWeaponStats: this.modWeaponStats,
                    traderInfo: this.tradersInfo,

                    bp_prestigebg: config.bp_usePrestigeBackground,
                    bp_cardbg: config.bp_backgroundReward,
                    bp_mainbg: config.bp_mainBackgroundReward,
                    bp_cat: config.bp_catReward,
                    bp_pfpstyle: config.bp_pfpStyle,
                    bp_pfpbordercolor: config.bp_pfpBorder
                }
                // PMC Raid with public profile on
            } else if (config.public_profile && !isScavRaid) {
                return {
                    ...baseData,
                    publicProfile: true,
                    raidDamage: totalDamage,
                    longestShot: totalLongestShot,
                    playedAs: "PMC",
                    pmcSide: this.staticProfile.characters.pmc.Info.Side,
                    scavLevel: scavLevel,
                    winRaidStreak: curWinStreak,
                    profileAboutMe: config.profile_aboutMe,
                    profilePicture: config.profile_profilePicture,
                    profileTheme: config.profile_profileTheme,
                    registrationDate: profile.Info.RegistrationDate,
                    lastRaidMap: this.lastRaidMap,
                    lastRaidEXP: lootEXP,
                    lastRaidHits: lastHits,
                    isTransition: isTransition,
                    lastRaidTransitionTo: lastRaidTransitionTo,
                    discFromRaid: discFromRaid,
                    prestige: this.staticProfile.characters.pmc.Info.PrestigeLevel,
                    usePrestigeStyling: config.profile_usePrestigeStyling,
                    latestAchievementName: this.mostRecentAchievementName,
                    latestAchievementDescription: this.mostRecentAchievementDescription,
                    latestAchievementImageUrl: this.mostRecentAchievementImageUrl,
                    latestAchievementTimestamp: this.mostRecentAchievementTimestamp,
                    hasKappa: this.hasKappa,
                    weaponMasteryId: this.masteryWeaponId,
                    weaponMasteryProgress: this.masteryWeaponProgress,
                    isUsingStattrack: this.isUsingStattrack,
                    modWeaponStats: this.modWeaponStats,
                    traderInfo: this.tradersInfo,

                    bp_prestigebg: config.bp_usePrestigeBackground,
                    bp_cardbg: config.bp_backgroundReward,
                    bp_mainbg: config.bp_mainBackgroundReward,
                    bp_cat: config.bp_catReward,
                    bp_pfpstyle: config.bp_pfpStyle,
                    bp_pfpbordercolor: config.bp_pfpBorder
                }
                // Private profile raid
            } else {
                return {
                    publicProfile: false,
                    ...baseData
                }
            }
        }

        // Send the data
        const sendProfileData = async (data) => {
            // 10 seconds time-out
            const controller = new AbortController();
            const timeout = setTimeout(() => controller.abort(), 10000);

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

        // UTILS
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