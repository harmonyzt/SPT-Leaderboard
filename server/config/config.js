module.exports = {
    // Enabling this will switch you to a Casual Mode.
    // You will not be ranked in the leaderboard and your stats won't count towards its progress.
    // You'll be free off any leaderboard restrictions (except reasonable ones), have access to raid history and your profile like usual.
    // DANGER - Once you played with this ON - YOU CANT GET BACK INTO RANKING.
    mod_casualMode: false,
    
    // If you want to share more Profile SPT stats with anyone and the leaderboard - set to true
    // This also allows your server to send heartbeats to API
    public_profile: true,

    // Enable mod support to send extra data for your profile
    // Mod automatically detects mods that it supports
    // Currently supports: 
    // Stattrack by AcidPhantasm (extra weapon stats at battlepass tab and weapon mastery)
    enable_mod_support: true,

    // Maximum raids to retry to connect to the Leaderboard API if it failed for first time
    connectionRetries: 1,

    // How long mod will be waiting for the response from Leaderboard API, in MILLISECONDS
    // EXAMPLE: 10000 - 10 seconds | 20000 - 20 seconds
    connectionTimeout: 15000,

    // DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.
    // Domain (or both subdomain + domain) used for PHP requests
    PHP_ENDPOINT: "visuals.nullcore.net",

    // Path to PHP file or API that process incoming data from this mod
    // Example - domain.com/spt_profile_server.php <-- in this case you put "/spt_profile_server.php"
    PHP_PATH: "/SPT/api/v1/main.php",

    // Display debug messages in console and log them inside SPT server .log file
    DEBUG: false
};