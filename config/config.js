module.exports = {
    // If you want to share more Profile SPT stats with anyone and the leaderboard - set to true
    public_profile: true,

    // If you want to display other name rather than your SPT profile name, put it here, or leave blank
    // Maximum - 15 characters
    profile_customName: "",

    // Your about me to show for public profile. Maximum - 80 characters
    profile_aboutMe: "1 like = 1 meow",

    // Profile picture
    // Optimal aspect ratio: 1:1 (500x500, 250x250 etc)
    // Supports: .png .gif
    // Allowed domains: i.imgur.com | tenor.com | media1.tenor.com
    profile_profilePicture: "https://i.imgur.com/z15FrrK.png",

    // Profile theme
    // Will change the background of your profile if it's public
    // Default | Dark | Light | Gradient
    profile_profileTheme: "Gradient",

    // Prestige Styling (Public Profile only)
    // If you have prestige unlocked - this allows your profile name to change its style
    profile_usePrestigeStyling: true,

    // Maximum raids to retry to connect to the leaderboard if it failed for first time
    connectionRetries: 1,

    // Display debug messages in console and log them inside SPT server .log file
    DEBUG: true,

    // DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.
    // Domain (or both subdomain + domain) used for PHP requests
    PHP_ENDPOINT: "visuals.nullcore.net",

    // Path to PHP file that process incoming data from this mod
    // Example - domain.com/spt_profile_server.php <-- in this case you put "/spt_profile_server.php"
    PHP_PATH: "/hidden/SPT_Profiles_Backend.php"
};