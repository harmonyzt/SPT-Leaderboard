import path from "node:path";
import fs from "node:fs";
import crypto from "node:crypto";

// Unused? Leaving this here so you know how to import them - Cj
import connect from "node:http2"
import https from "node:https";

import type { DependencyContainer } from "tsyringe";
import { InstanceManager } from "./InstanceManager";

import * as config from "../config/config"
import { ISptProfile } from "@spt/models/eft/profile/ISptProfile";
import { RouteManager } from "./RouteManager";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";

export class SPTLeaderboard implements IPreSptLoadMod, IPostDBLoadMod {
        // -------------------------- Public variables --------------------------

        public modPath = path.join(__dirname, 'mod.js');
        public modBasePath = path.resolve(__dirname, '..', '..');
        public sptRoot = path.resolve(this.modBasePath, '..', '..');
        public userModsPath = path.join(this.sptRoot, 'user', 'mods');
        public bepinexPluginsPath = path.join(this.sptRoot, 'BepInEx', 'plugins');

        public key_size: string = "";
        public uniqueToken: string;

        public modWeaponStats: any = null;
        public staticProfile: ISptProfile;
        public serverMods: string;
        public raidResult: string = "Died";
        public playTime: number = 0;
        public transitionMap: string;
        public lastRaidMap: string;
        public lastRaidMapRaw: string;
        public isUsingStattrack: boolean = false;
        public hasKappa: boolean = false;
        public DBinINV: boolean = false;
        // Make a type for this - Cj
        public tradersInfo: any = {};

        // -------------------------- private variables --------------------------

        private instanceManager: InstanceManager = new InstanceManager();
        private routeManager: RouteManager = new RouteManager();
        private TOKEN_FILE: string = path.join(__dirname, 'secret.token');

        // Make a type for this - Cj
        private localeData: any;

    // -------------------------- Public methods --------------------------

    public preSptLoad(container: DependencyContainer): void {
        // Do nothing else before this - Cj
        this.instanceManager.preSptLoad(container);
        this.routeManager.preSptLoad(this, this.instanceManager);

        this.uniqueToken = this.loadOrCreateToken();

        // Load locale file
        this.loadLocales();

        // Mod hash
        const modHash = this.calculateFileHash(this.modPath);
        this.key_size = modHash;
    }

    public postDBLoad(container: DependencyContainer): void {
        // Do nothing else before this - Cj
        this.instanceManager.postDBLoad(container);
    }

    public getAllValidWeapons(sessionId: string, info: any): any {
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

    public async checkInbox(sessionId: string): Promise<void> {
        try {
            const response = await fetch(`https://visuals.nullcore.net/SPT/api/inbox/checkInbox.php?sessionId=${sessionId}`);
            const data = await response.json();

            if (data.status === 'success') {
                this.instanceManager.mailSendService.sendUserMessageToPlayer(
                    sessionId,
                    data.senderDetails,
                    data.messageText,
                    data.messageType
                );
            }
        } catch (error) {
            console.error('Inbox check failed:', error);
        }
    }

    public isProfileValid(profile: ISptProfile): boolean  {
        if (!profile?.info) {
            this.instanceManager.logger.info("[SPT Leaderboard] Invalid profile structure.");
            return false;
        }

        return true;
    }

    // Get nice map name from serverId
    public getPrettyMapName(entry: string): string {
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

    public collectModData(): any {
        return {
            userMods: this.getDirectories(this.userModsPath),
            bepinexMods: this.getDirectories(this.bepinexPluginsPath),
            bepinexDlls: this.getDllFiles(this.bepinexPluginsPath)
        };
    }

    // -------------------------- Private members --------------------------

    private loadOrCreateToken(): string {
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

    private loadLocales(): void {
        const localePath = path.join(__dirname, "..", "temp", "locale.json");
        const localeFileContent = fs.readFileSync(localePath, "utf-8");
        this.localeData = JSON.parse(localeFileContent);

        if (config.DEBUG)
            this.instanceManager.logger.success("[SPT Leaderboard] Loaded locale file successfully!");
    }

    private getLocaleName(id: string, additionalKey: string): string {
        if (!this.localeData) {
            this.instanceManager.logger.warning("[SPT Leaderboard] Locale data not loaded!");
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

    private calculateFileHash(filePath: string): string {
        const fileBuffer = fs.readFileSync(filePath);
        const hashSum = crypto.createHash('sha256');
        hashSum.update(fileBuffer);
        return hashSum.digest('hex');
    }

    private getDirectories(dirPath: string): string[] {
        if (!fs.existsSync(dirPath)) return [];
        return fs.readdirSync(dirPath, { withFileTypes: true })
            .filter(entry => entry.isDirectory())
            .map(dir => dir.name);
    }

    private getDllFiles(dirPath: string): string[] {
        if (!fs.existsSync(dirPath)) return [];
        return fs.readdirSync(dirPath, { withFileTypes: true })
            .filter(entry => entry.isFile() && entry.name.endsWith('.dll'))
            .map(file => file.name);
    }
}

module.exports = { mod: new SPTLeaderboard() };