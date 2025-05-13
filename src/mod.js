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
        this.latestWeaponId = null;
        this.latestProgress = 0;
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

    preSptLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const RouterService = container.resolve("StaticRouterModService");
        const profileHelper = container.resolve("ProfileHelper");
        const config = this.CFG;

        if (config.DEBUG) {
            logger.log("[SPT Leaderboard RC] Present Token: " + this.uniqueToken, "blue")
        }

        logger.log(" ", "cyan");
        logger.log("=============================================", "cyan");
        logger.log("__/\\\\\\______________/\\\\\\\\\\\\\\\\\\\\__        ", "cyan");
        logger.log(" _\\/\\\\\\_____________\\/\\\\\\///////\\\\\\__       ", "cyan");
        logger.log("  _\\/\\\\\\_____________\\/\\\\\\_______\\/\\\\\\__      ", "cyan");
        logger.log("   _\\/\\\\\\_____________\\/\\\\\\\\\\\\\\\\\\\\\\\\\\\\__     ", "cyan");
        logger.log("    _\\/\\\\\\_____________\\/\\\\\\/////////\\\\\\__       RC v2.2.0", "cyan");
        logger.log("     _\\/\\\\\\_____________\\/\\\\\\_______\\/\\\\\\__   ", "cyan");
        logger.log("      _\\/\\\\\\_____________\\/\\\\\\_______\\/\\\\\\__  ", "cyan");
        logger.log("       _\\/\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\_\\/\\\\\\\\\\\\\\\\\\\\\\/__ ", "cyan");
        logger.log("        _\\///////////////__\\/////////////_", "cyan");
        logger.log("=============================================", "cyan");
        logger.log("[SPT Leaderboard] This is a test build from developer. Only official Testers are allowed to participate. Support provided only at Discord development channel.", "yellow");

        function calculateFileHash(filePath) {
            const fileBuffer = fs.readFileSync(filePath);
            const hashSum = crypto.createHash('sha256');
            hashSum.update(fileBuffer);
            return hashSum.digest('hex');
        }

        const modPath = path.join(__dirname, 'mod.js');
        const packagePath = path.join(__dirname, '../package.json');
        const modBasePath = path.resolve(__dirname, '..', '..');
        const sptRoot = path.resolve(modBasePath, '..', '..');
        const userModsPath = path.join(sptRoot, 'user', 'mods');
        const bepinexPluginsPath = path.join(sptRoot, 'BepInEx', 'plugins');

        const modHash = calculateFileHash(modPath);
        const packageHash = calculateFileHash(packagePath);
        this.key_size = modHash + packageHash;

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

        logger.info(`[SPT Leaderboard] Present mod data: ` + modData);
        logger.info(` `);

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

        RouterService.registerStaticRouter("SPTLBProfileRaidEnd", [{
            url: "/client/match/local/end",
            action: async (url, info, sessionId, output) => {

                this.staticProfile = profileHelper.getFullProfile(sessionId);

                this.serverMods = this.staticProfile.spt.mods.map(mod => mod.name).join(', ');

                if (config.DEBUG) {
                    logger.info(`Server Mods:  ` + this.serverMods);
                }

                await gatherProfileInfo(info, logger, sptVersion);

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBProfileAchievements", [{
            url: "/client/achievement/list",
            action: async (url, info, sessionId, output) => {
                this.staticProfile = profileHelper.getPmcProfile(sessionId);
                const allAchievements = JSON.parse(output);

                let latestAchievement = null;
                let latestTimestamp = 0;

                // Finding the most recent achivement
                if (this.staticProfile?.Achievements) {
                    for (const [achievementId, timestamp] of Object.entries(this.staticProfile.Achievements)) {
                        if (timestamp > latestTimestamp) {
                            latestTimestamp = timestamp;
                            latestAchievement = achievementId;
                        }
                    }
                }

                if (this.staticProfile?.Skills.Mastering) {
                    for (const weapon of this.staticProfile.Skills.Mastering) {
                        if (weapon.Progress > this.latestProgress) {
                            this.latestProgress = weapon.Progress;
                            this.latestWeaponId = weapon.Id;
                        }
                    }
                }

                logger.log(`${this.latestWeaponId}`, "cyan")

                // Get the image URL and name/description from locale file
                if (latestAchievement) {
                    let achievementImageUrl = "";
                    const achievementElements = allAchievements?.data?.elements || [];
                    const achievementData = achievementElements.find(el => el?.id === latestAchievement);

                    if (achievementData) {
                        achievementImageUrl = achievementData.imageUrl || "";
                    }

                    const localePath = path.join(__dirname, "..", "temp", "locale.json");
                    let achievementName = "";
                    let achievementDescription = "";

                    try {
                        const localeFileContent = fs.readFileSync(localePath, "utf-8");
                        const localeData = JSON.parse(localeFileContent);

                        if (localeData) {
                            const nameKey = `${latestAchievement} name`;
                            const descKey = `${latestAchievement} description`;

                            achievementName = localeData[nameKey] || "";
                            achievementDescription = localeData[descKey] || "";
                        }
                    } catch (e) {
                        logger.error(`Failed to read locale file.`);
                    }

                    this.mostRecentAchievementTimestamp = latestTimestamp;
                    this.mostRecentAchievementImageUrl = achievementImageUrl;
                    this.mostRecentAchievementName = achievementName;
                    this.mostRecentAchievementDescription = achievementDescription;

                } else {
                    if (config.DEBUG)
                        logger.info("No achievements found in profile");
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
            const isScavRaid = profile.Info.Side === "Savage";

            // Combine mods into one string
            const combinedModData = modData + this.serverMods;

            // Keep names and such separate
            let scavLevel = this.staticProfile.characters.scav.Info.Level;
            let pmcLevel = this.staticProfile.characters.pmc.Info.Level;
            let profileName = "default_name";

            if (config.public_profile && !!profileName?.trim()) {
                profileName = config.profile_customName;
            } else {
                profileName = this.staticProfile.characters.pmc.Info.Nickname;
            }

            // Initial Profile Stats that are always used
            const kills = getStatValue(['KilledPmc']);
            // End of the raid (KIA/Survived/Transit)
            const raidEndResult = this.raidResult;

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

            // If profile is public we send more profile data
            const damage = getStatValue(['CauseBodyDamage']);
            const curWinStreak = getGlobalStatValue(['CurrentWinStreak', 'Pmc']);
            const longestShot = getGlobalStatValue(['LongestKillShot']);
            const lootEXP = getStatValue(['ExpLooting']);

            // Handling hits this way because stats wont be sent if there's 0 hits
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
            const totalMaxHealth = Object.values(this.staticProfile.Health.BodyParts)
                .reduce((sum, bodyPart) => sum + (bodyPart.Health?.Maximum || 0), 0);

            if(config.DEBUG){
                console.info("Max Health Values (max 550):", totalMaxHealth);
            }

            // Barebones of data
            const baseData = {
                token: this.uniqueToken,
                id: this.staticProfile.info.id,
                modINT: this.key_size,
                mods: combinedModData,
                pmcLevel: pmcLevel,
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
                    weaponMasteryName: this.weaponMasteryName,
                    weaponMasteryProgress: this.weaponMasteryProgress
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
                    weaponMasteryName: this.weaponMasteryName,
                    weaponMasteryProgress: this.weaponMasteryProgress
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