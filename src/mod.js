"use strict";
const fs = require('fs');

class SPTLeaderboard {
    CFG = require("../config/config.json");

    preSptLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const profileHelper = container.resolve("ProfileHelper");
        const RouterService = container.resolve("StaticRouterModService");
        const config = this.CFG;

        
        RouterService.registerStaticRouter("SPTLBProfileLogin", [{
            url: "/launcher/profile/login",
            action: async (url, info, sessionId, output) => {
                const fullProfile = this.profileHelper.getFullProfile(sessionId);

                logger.log(fullProfile, "cyan")

                return output;
            }
        }], "aki");

    }

}

module.exports = { mod: new SPTLeaderboard() };
