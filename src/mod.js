"use strict";
const { connect } = require('http2');
const https = require('https');
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

class SPTLeaderboard {
    constructor() {
        this.retriesCount = 0;
        this.connectivity = 1;
        this.allMods = [];
        this.key_size = 0;
        this.TOKEN_FILE = path.join(__dirname, 'secret.token');
        this.uniqueToken = this.loadOrCreateToken();
        this.CFG = require("../config/config");
        this.PHP_ENDPOINT = "visuals.nullcore.net";
        this.PHP_PATH = "/hidden/SPT_Profiles_Backend.php";
        this.raidResult = "Died";
    }

    loadOrCreateToken() {
        try {
            // If token exists
            if (fs.existsSync(this.TOKEN_FILE)) {
                console.log(`[SPT Leaderboard] Your secret token was initialized. Remember to never show it to anyone!`);
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
            const mods = [
                ...getDirectories(userModsPath),
                ...getDirectories(bepinexPluginsPath),
                ...getDllFiles(bepinexPluginsPath)
            ];
            return { mods };
        }

        const modData = collectModData();

        // Define SPT version
        var configServer = container.resolve("ConfigServer");
        var coreConfig = configServer.getConfig("spt-core");
        var sptVersion = coreConfig.sptVersion;

        RouterService.registerStaticRouter("SPTLBProfileRaidEnd", [{
            url: "/client/match/local/end",
            action: async (url, info, sessionId, output) => {

                const staticProfile = profileHelper.getFullProfile(sessionId);

                await gatherProfileInfo(info, logger, sptVersion);

                return output;
            }
        }], "aki");

        const gatherProfileInfo = async (data, logger, version) => {
            const config = this.CFG;

            const jsonData = JSON.parse(JSON.stringify(data));
            const fullProfile = jsonData.results.profile;

            // Get the result of a raid (Died/Survived/Runner)
            raidResult = jsonData.results.result;
            playTime = jsonData.results.playTime;

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
                logger.log(`[SPT Leaderboard] Error: ${e.message}`, "red");
            }
        }

        // Calculate stats from profile
        const processAndSendProfile = async (profile, logger, version) => {
            const profileData = await processProfile(profile, version);
            const config = this.CFG;

            if (config.debug)
                logger.log(`[SPT Leaderboard] Data ready! ${profileData}`, "green");

            try {
                await sendProfileData(profileData);

                if (config.debug)
                    logger.log("[SPT Leaderboard] Data sent successfully!", "green");
            } catch (e) {
                logger.log(`[SPT Leaderboard] Could not send data to leaderboard: ${e.message}`, "red");
            }
        }

        const processProfile = async (profile, versionSPT) => {
            const getStatValue = (keys) => {
                const item = profile.Stats.Eft.SessionCounters.Items?.find(item =>
                    item.Key && keys.every((k, i) => item.Key[i] === k)
                );
                return item?.Value || 0;
            };

            const config = this.CFG;

            // If this was a SCAV raid, handle differently
            if(profile.Info.Side === "Savage") {
                // We want to keep actual player name so it can't be changed by accident to SCAVs name
                const pmcProfileName = profile.Info.MainProfileNickname;
                const scavLevel = profile.Info.Level;
            } else {
                const pmcProfileName = staticProfile.Info.id;
                const pmcProfileLevel = staticProfile.characters.pmc.Level;
            }

            // Initial Profile Stats that are always used
            const kills = getStatValue(['KilledPmc']);
            // This determines deaths, too
            const raidEndResult = raidResult;

            // If profile is public we send more profile data
            const damage = getStatValue(['CauseBodyDamage']);
            const curWinStreak = getStatValue(['CurrentWinStreak'], ['Pmc']);
            const longestShot = getStatValue(['LongestShot']);
            // Perform this abomination to get damage without FLOATING GHOST NUMBERS (thanks BSG)
            const modDamage = damage.toString();
            const modLongestShot = longestShot.toString();
            const totalLongestShot = parseInt(modLongestShot.slice(0, -2), 10);
            const totalDamage = parseInt(modDamage.slice(0, -2), 10);

            // If profile is set to public we send more data to PHP so more stats will be avalivable (updates every end of the raid)
            if (config.public_profile) {
                if(profile.Info.Side === "Savage"){
                    return {
                        // Never changed by the Side
                        token: this.uniqueToken,
                        id: staticProfile.Info.id,
                        modINT: this.key_size,
                        mods: modData,
                        
                        name: pmcProfileName,
                        lastPlayed: profile.Stats.Eft.LastSessionDate,
                        accountType: profile.Info.GameVersion,
                        sptVer: versionSPT,
                        disqualified: false,
                        raidEndResult: raidEndResult,
                        kills: kills,
    
                        // Public Profile Only
                        publicProfile: true,
                        profilePfp: config.profile_profilePicture,
                        profileAbout: config.profile_aboutMe,
                        registrationDate: profile.Info.RegistrationDate,
                        faction: staticProfile.characters.pmc.Info.Side,
                        damage: totalDamage,
                        currentWinstreak: curWinStreak,
                        longestShot: totalLongestShot,

                        // For SCAV
                        scavLevel: profile.Info.Level,
                        scavRaids: 1
                    };
                } else {
                    return {
                        // Never changed by the Side
                        token: this.uniqueToken,
                        id: staticProfile.Info.id,
                        modINT: this.key_size,
                        mods: modData,

                        name: profileName,
                        lastPlayed: profile.Stats.Eft.LastSessionDate,
                        pmcLevel: profile.Info.Level,
                        totalRaids: totalRaids,
                        survivedToDiedRatio: surviveRate,
                        killToDeathRatio: killToDeathRatio,
                        averageLifeTime: avgLifeTime,
                        accountType: profile.Info.GameVersion,
                        sptVer: versionSPT,
                        disqualified: false,
    
                        // Public Profile Only
                        publicProfile: true,
                        profilePfp: config.profile_profilePicture,
                        profileAbout: config.profile_aboutMe,
                        registrationDate: profile.Info.RegistrationDate,
                        faction: profile.Info.Side,
                        damage: totalDamage,
                        currentWinstreak: curWinStreak,
                        longestShot: totalLongestShot,
                    };
                }
            } else {
                return {
                    token: this.uniqueToken,
                    id: staticProfile.Info.id,
                    modINT: this.key_size,
                    fullSPTProfile: fullProfile,
                    mods: modData,

                    name: profileName,
                    lastPlayed: profile.Stats.Eft.LastSessionDate,
                    pmcLevel: profile.Info.Level,
                    totalRaids: totalRaids,
                    survivedToDiedRatio: surviveRate,
                    killToDeathRatio: killToDeathRatio,
                    averageLifeTime: avgLifeTime,
                    accountType: profile.Info.GameVersion,
                    sptVer: versionSPT,
                    disqualified: false,

                    publicProfile: true,
                    registrationDate: profile.Info.RegistrationDate,
                    faction: profile.Info.Side,
                    damage: totalDamage,
                    currentWinstreak: curWinStreak,
                    longestShot: totalLongestShot,
                };
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
                logger.log("[SPT Leaderboard] Invalid profile structure.", "yellow");
                return false;
            }

            return true;
        }

        const formatTime = (minutes) => {
            const mins = Math.floor(minutes);
            const secs = Math.floor((minutes - mins) * 60);
            return `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
        }
    }
}

module.exports = { mod: new SPTLeaderboard() };