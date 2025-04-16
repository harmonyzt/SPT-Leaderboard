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
        this.TOKEN_FILE = path.join(__dirname, 'secret.token');
        this.uniqueToken = this.loadOrCreateToken();
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
            // Generating token in case
            return crypto.randomBytes(32).toString('hex');
        }
    }

    CFG = require("../config/config.json");
    PHP_ENDPOINT = "visuals.nullcore.net";
    PHP_PATH = "/hidden/SPTprofileRecorder.php";

    preSptLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const RouterService = container.resolve("StaticRouterModService");

        // Define SPT version
        var configServer = container.resolve("ConfigServer");
        var coreConfig = configServer.getConfig("spt-core");
        var sptVersion = coreConfig.sptVersion;

        RouterService.registerStaticRouter("SPTLBProfileRaidEnd", [{
            url: "/client/match/local/end",
            action: async (url, info, sessionId, output) => {
                await this.gatherProfileInfo(info, logger, sptVersion);

                return output;
            }
        }], "aki");

    }

    async gatherProfileInfo(data, logger, version) {
        const config = this.CFG;

        try {
            const jsonData = JSON.parse(JSON.stringify(data));
            const fullProfile = jsonData.results.profile;

            if (this.connectivity == 0) return;

            logger.log(`[SPT Leaderboard] Ready to update statistics...`, "cyan");

            if (this.isProfileValid(fullProfile, logger)) {
                await this.processAndSendProfile(fullProfile, logger, version);
            } else {
                // Time out and retry next raid
                if (this.retriesCount <= config.connectionRetries) {
                    this.retriesCount += 1;
                } else {
                    logger.log(`[SPT Leaderboard] Could not establish internet connection with PHP. Mod will be paused until next SPT Server start.`, "red");
                    this.connectivity = 0;

                    return;
                }
            }
        } catch (e) {
            logger.log(`[SPT Leaderboard] Error: ${e.message}`, "red");
        }
    }

    // Calculate stats from profile
    async processAndSendProfile(profile, logger, version) {
        const profileData = this.processProfile(profile, version);
        const config = this.CFG;

        if (config.debug)
            logger.log(`[SPT Leaderboard] Data ready!`, "green");

        try {
            await this.sendProfileData(profileData);

            if (config.debug)
                logger.log("[SPT Leaderboard] Data sent successfully!", "green");
        } catch (e) {
            logger.log(`[SPT Leaderboard] Send error: ${e.message}`, "red");
        }
    }

    processProfile(profile, version) {
        const getStatValue = (keys) => {
            const item = profile.Stats.Eft.OverallCounters.Items?.find(item =>
                item.Key && keys.every((k, i) => item.Key[i] === k)
            );
            return item?.Value || 0;
        };

        const config = this.CFG;

        // Initial Profile Stats (Public, will always be sent)
        const kills = getStatValue(['Kills']);
        const deaths = getStatValue(['Deaths']);
        const totalRaids = getStatValue(['Sessions', 'Pmc']);
        const totalLifetime = getStatValue(['LifeTime', 'Pmc']);
        const surviveRate = totalRaids > 0 ? ((totalRaids - deaths) / totalRaids * 100).toFixed(2) : 0;
        const killToDeathRatio = deaths !== 0 ? (kills / deaths).toFixed(2) : kills.toFixed(2);
        const avgLifeTime = totalRaids > 0 ? this.formatTime((totalLifetime / 60) / totalRaids) : "00:00";

        // If profile is public we send more profile data
        // this is a FLOAT without floating point
        const damage = getStatValue(['CauseBodyDamage']);
        const curWinStreak = getStatValue(['CurrentWinStreak'], ['Pmc']);
        const longestShot = getStatValue(['LongestShot']);
        // Perform this abomination to get damage without FLOATING GHOST NUMBERS (thanks BSG)
        const modDamage = damage.toString();
        const modLongestShot = longestShot.toString();

        const totalLongestShot = parseInt(modLongestShot.slice(0, -2), 10);
        const totalDamage = parseInt(modDamage.slice(0, -2), 10);

        // If profile is set to public we send more data to PHP so more stats will be avalivable (updates every end of the raid)
        if (!config.publicProfile) {
            return {
                token: this.uniqueToken,
                id: profile._id,

                name: profile.Info.Nickname,
                lastPlayed: profile.Stats.Eft.LastSessionDate,
                pmcLevel: profile.Info.Level,
                totalRaids: totalRaids,
                survivedToDiedRatio: surviveRate,
                killToDeathRatio: killToDeathRatio,
                averageLifeTime: avgLifeTime,
                accountType: profile.Info.GameVersion,
                sptVer: version,
                fika: "false",
                publicProfile: "false",
                registrationDate: 0,
                faction: 0,
                damage: 0,
                currentWinstreak: 0,
                longestShot: 0
            };
        } else {
            return {
                token: this.uniqueToken,
                id: profile.info.id,

                name: profile.characters.pmc.Info.Nickname,
                lastPlayed: profile.characters.pmc.Stats.Eft.LastSessionDate,
                pmcLevel: profile.characters.pmc.Info.Level,
                totalRaids: totalRaids,
                survivedToDiedRatio: surviveRate,
                killToDeathRatio: killToDeathRatio,
                averageLifeTime: avgLifeTime,
                accountType: profile.characters.pmc.Info.GameVersion,
                sptVer: version,
                fika: "false",
                publicProfile: "true",
                registrationDate: profile.characters.pmc.Info.RegistrationDate,
                faction: profile.characters.pmc.Info.Side,
                damage: totalDamage,
                currentWinstreak: curWinStreak,
                longestShot: totalLongestShot
            };
        }
    }

    // Send the data
    async sendProfileData(data) {
        return new Promise((resolve, reject) => {
            const postData = JSON.stringify(data);

            const options = {
                hostname: this.PHP_ENDPOINT,
                path: this.PHP_PATH,
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-SPT-Mod': 'SPTLeaderboard',
                    'Content-Length': Buffer.byteLength(postData)
                },
                timeout: 10000
            };

            const req = https.request(options, (res) => {
                let responseData = '';

                res.on('data', (chunk) => {
                    responseData += chunk;
                });

                res.on('end', () => {
                    if (res.statusCode === 200) {
                        resolve(responseData);
                    } else {
                        reject(new Error(`HTTP ${res.statusCode}: ${responseData}`));
                    }
                });
            });

            req.on('error', (e) => reject(e));
            req.on('timeout', () => {
                req.destroy();
                reject(new Error('Request timeout after 10 seconds'));
            });

            req.write(postData);
            req.end();
        });
    }

    // UTILS

    // Online? (doesnt work this way)
    isOnline() {
        return navigator.onLine;
    }

    // Let's see if you are ready to enter the battle
    isProfileValid(profile, logger) {
        if (!profile?.Info) {
            logger.log("[SPT Leaderboard] Invalid profile structure", "yellow");
            return false;
        }

        if (profile.Info.Level <= 4) {
            logger.log("[SPT Leaderboard] PMC level too low to enter Leaderboard (<=5)", "yellow");
            return false;
        }

        //if (getStatValue(['Sessions', 'Pmc']) <= 5) {
        //    logger.log("[SPT Leaderboard] Not enough raids to enter Leaderboard (<=5)", "yellow");
        //    return false;
        //}

        return true;
    }

    formatTime(minutes) {
        const mins = Math.floor(minutes);
        const secs = Math.floor((minutes - mins) * 60);
        return `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
    }
}

module.exports = { mod: new SPTLeaderboard() };