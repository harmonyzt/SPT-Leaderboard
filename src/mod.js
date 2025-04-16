"use strict";
const https = require('https');

class SPTLeaderboard {
    CFG = require("../config/config.json");
    PHP_ENDPOINT = "visuals.nullcore.net";
    PHP_PATH = "/hidden/SPTprofileRecorder.php";

    preSptLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const config = this.CFG;

        this.profileHelper = container.resolve("ProfileHelper");
        const RouterService = container.resolve("StaticRouterModService");

        let retriesCount = 0;
        let connectivity = 1;

        if (!config.fika) {
            RouterService.registerStaticRouter("SPTLBProfileLogin", [{
                url: "/client/match/local/end",
                action: async (url, info, sessionId, output) => {
                    if (!sessionId) return output;

                    logger.log(`[SPT Leaderboard] Ready to update statistics...`, "cyan");

                    try {
                        const fullProfile = this.profileHelper.getFullProfile(sessionId);
                        if (this.isProfileValid(fullProfile, logger)) {
                            await this.processAndSendProfile(fullProfile, logger);
                        } else {
                            // Time out and retry next raid
                            if (retriesCount >= config.connectionRetries) {
                                retriesCount += 1;
                            } else {
                                logger.log(`[SPT Leaderboard] Could not establish internet connection with PHP. Mod will be paused until next SPT Server start.`, "red");
                                connectivity = 0;

                                return;
                            }
                        }
                    } catch (e) {
                        logger.log(`[SPT Leaderboard] Error: ${e.message}`, "red");
                    }

                    return output;
                }
            }], "aki");
        } else {
            RouterService.registerStaticRouter("SPTLBProfileLogin", [{
                url: "/fika/raid/leave",
                action: async (url, info, sessionId, output) => {
                    if (!sessionId) return output;

                    try {
                        const fullProfile = this.profileHelper.getFullProfile(sessionId);
                        if (this.isProfileValid(fullProfile, logger) && this.isOnline() && connectivity == 1) {
                            await this.processAndSendProfile(fullProfile, logger);
                        } else {
                            // Time out and retry next raid
                            if (connectionRetries < config.connectionRetries) {
                                connectivity = 1;
                            } else {
                                logger.log(`[SPT Leaderboard] Could not establish internet connection with PHP. Mod will be paused until next SPT Server start.`, "red");
                                connectivity = 0;

                                return;
                            }
                        }
                    } catch (e) {
                        logger.log(`[SPT Leaderboard] Error: ${e.message}`, "red");
                    }

                    return output;
                }
            }], "aki");
        }
    }

    // Calculate stats from profile
    async processAndSendProfile(profile, logger) {
        const profileData = this.processProfile(profile);

        if(config.debug)
            logger.log(`[SPT Leaderboard] Data ready: ${JSON.stringify(profileData)}`, "green");

        try {
            await this.sendProfileData(profileData);

            if(config.debug)
                logger.log("[SPT Leaderboard] Data sent successfully!", "green");
        } catch (e) {
            logger.log(`[SPT Leaderboard] Send error: ${e.message}`, "red");
        }
    }

    processProfile(profile) {
        const getStatValue = (keys) => {
            const item = profile.characters.pmc.Stats.Eft.OverallCounters.Items?.find(item =>
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
                id: profile.info.id,
                name: profile.characters.pmc.Info.Nickname,
                lastPlayed: profile.characters.pmc.Stats.Eft.LastSessionDate,
                pmcLevel: profile.characters.pmc.Info.Level,
                totalRaids: totalRaids,
                survivedToDiedRatio: surviveRate,
                killToDeathRatio: killToDeathRatio,
                averageLifeTime: avgLifeTime,
                accountType: profile.characters.pmc.Info.GameVersion,
                sptVer: profile.spt.version,
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
                id: profile.info.id,
                name: profile.characters.pmc.Info.Nickname,
                lastPlayed: profile.characters.pmc.Stats.Eft.LastSessionDate,
                pmcLevel: profile.characters.pmc.Info.Level,
                totalRaids: totalRaids,
                survivedToDiedRatio: surviveRate,
                killToDeathRatio: killToDeathRatio,
                averageLifeTime: avgLifeTime,
                accountType: profile.characters.pmc.Info.GameVersion,
                sptVer: profile.spt.version,
                fika: config.fika,
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
        if (!profile?.characters?.pmc?.Info) {
            logger.log("[SPT Leaderboard] Invalid profile structure", "yellow");
            return false;
        }

        if (profile.characters.pmc.Info.Level <= 5) {
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