"use strict";
const https = require('https');
const fs = require('fs');
const { connect } = require('http2');

class SPTLeaderboard {
    CFG = require("../config/config.json");
    PHP_ENDPOINT = "visuals.nullcore.net";
    PHP_PATH = "/hidden/SPTprofileRecorder.php";

    preSptLoad(container) {
        logger = container.resolve("WinstonLogger");
        this.profileHelper = container.resolve("ProfileHelper");
        const RouterService = container.resolve("StaticRouterModService");

        // Right now we try only once
        let connectivity = 1;

        RouterService.registerStaticRouter("SPTLBProfileLogin", [{
            url: "/launcher/profile/login",
            action: async (url, info, sessionId, output) => {
                if (!sessionId) return output;
                
                try {
                    const fullProfile = this.profileHelper.getFullProfile(sessionId);
                    if (this.isProfileValid(fullProfile) && this.isOnline() && connectivity == 1) {
                        await this.processAndSendProfile(fullProfile);
                    } else {
                        logger.log(`[SPT Leaderboard] Could not establish internet connection. Pausing mod...`, "red");
                        connectivity = 0
                    }
                } catch (e) {
                    logger.log(`[SPTLeaderboard] Error: ${e.message}`, "red");
                }

                return output;
            }
        }], "aki");
    }

    // Calculate stats from profile
    async processAndSendProfile(profile) {
        const profileData = this.processProfile(profile);
        logger.log(`[SPT Leaderboard] Data ready: ${JSON.stringify(profileData)}`, "green");
        
        try {
            await this.sendProfileData(profileData);
            
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

        // Initial Profile Stats
        const kills = getStatValue(['Kills']);
        const deaths = getStatValue(['Deaths']);
        const totalRaids = getStatValue(['Sessions', 'Pmc']);
        const totalLifetime = getStatValue(['LifeTime', 'Pmc']);

        // Calculations
        const surviveRate = totalRaids > 0 ? ((totalRaids - deaths) / totalRaids * 100).toFixed(2) : 0;
        const killToDeathRatio = deaths !== 0 ? (kills / deaths).toFixed(2) : kills.toFixed(2);
        const avgLifeTime = totalRaids > 0 ? this.formatTime((totalLifetime / 60) / totalRaids) : "00:00";

        return {
            id: profile.info.id,
            name: profile.characters.pmc.Info.Nickname,
            lastPlayed: this.formatTimeToDate(profile.characters.pmc.Stats.Eft.LastSessionDate),
            pmcLevel: profile.characters.pmc.Info.Level,
            totalRaids: totalRaids,
            survivedToDiedRatio: surviveRate,
            killToDeathRatio: killToDeathRatio,
            averageLifeTime: avgLifeTime,
            accountType: profile.characters.pmc.Info.GameVersion,
            sptVer: profile.spt.version
        };
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

    // Online?
    isOnline() {
        return navigator.onLine;
    }

    // Let's see if you are ready to enter the battle
    isProfileValid(profile) {
        if (!profile?.characters?.pmc?.Info) {
            logger.log("[SPT Leaderboard] Invalid profile structure", "yellow");
            return false;
        }
        
        if (profile.characters.pmc.Info.Level <= 5) {
            logger.log("[SPT Leaderboard] PMC level too low to enter Leaderboard (<=5)", "yellow");
            return false;
        }

        if (getStatValue(['Sessions', 'Pmc']) <= 5) {
            logger.log("[SPT Leaderboard] Not enough raids to enter Leaderboard (<=5)", "yellow");
            return false;
        }
        
        return true;
    }

    formatTime(minutes) {
        const mins = Math.floor(minutes);
        const secs = Math.floor((minutes - mins) * 60);
        return `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`;
    }
    
    formatTimeToDate(unixTimestamp) {
        const date = new Date(unixTimestamp * 1000);
        return `${String(date.getDate()).padStart(2, '0')}.${String(date.getMonth() + 1).padStart(2, '0')}.${date.getFullYear()}`;
    }
}

module.exports = { mod: new SPTLeaderboard() };