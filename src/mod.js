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

                await gatherProfileInfo(info, logger, sptVersion);

                return output;
            }
        }], "aki");

        RouterService.registerStaticRouter("SPTLBProfileLogin", [{
            url: "/launcher/profile/login",
            action: async (url, info, sessionId, output) => {
                if (!sessionId) return;

                const staticProfile = profileHelper.getFullProfile(sessionId);

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
            const isScavRaid = profile.Info.Side === "Savage";

            // If this was a SCAV raid, handle differently
            if (isScavRaid && config.public_profile) {
                // We want to keep actual player name so it can't be changed by accident to SCAVs name
                const profileName = profile.Info.MainProfileNickname;
                const scavLevel = profile.Info.Level;
            } else {
                const profileName = staticProfile.Info.id;
                const pmcLevel = staticProfile.characters.pmc.Level;
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

            // Barebones of data
            const baseData = {
                token: this.uniqueToken,
                id: staticProfile.Info.id,
                modINT: this.key_size,
                mods: modData,
                name: profileName,
                raidKills: kills,
                disqualified: false,
                sptVer: versionSPT,
                raidResult: raidEndResult,
                raidTime: playTime
            }

            // Public SCAV raid (can't be otherwise)
            if (config.public_profile && isScavRaid) {
                return {
                    ...baseData,
                    publicProfile: true,
                    raidDamage: totalDamage,
                    longestShot: totalLongestShot,
                    playedAs: "SCAV",
                    pmcLevel: pmcLevel,
                    scavLevel: scavLevel,
                    winRaidStreak: curWinStreak
                }
            // PMC Raid with public profile on
            } else if(config.public_profile && !isScavRaid) {
                return {
                    ...baseData,
                    raidDamage: totalDamage,
                    longestShot: totalLongestShot,
                    playedAs: profile.Info.Side,
                    pmcLevel: pmcLevel,
                    scavLevel: scavLevel,
                    winRaidStreak: curWinStreak
                }
            // Private profile raid
            } else {
                return {
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
                logger.log("[SPT Leaderboard] Invalid profile structure.", "yellow");
                return false;
            }

            return true;
        }
    }
}

module.exports = { mod: new SPTLeaderboard() };