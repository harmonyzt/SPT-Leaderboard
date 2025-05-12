module.exports = {
    // If you want to share more Profile SPT stats with anyone and the leaderboard - set to true
    public_profile: false,

    // If you want to display other name rather than your SPT profile name, put it here, or leave blank
    // NOTE: Once you change your name and submit your stats - you won't be able to change it for 10 days!
    profile_customName: "",

    // Your about me to show for public profile. Maximum - 80 characters
    profile_aboutMe: "I didn't read configuration and enabled public profile!",

    // Profile picture
    // Optimal aspect ratio: 1:1 (500x500, 250x250 etc)
    // Supports: .png .gif
    // Allowed domains: i.imgur.com | tenor.com | media1.tenor.com
    profile_profilePicture: "https://i.imgur.com/z15FrrK.png",

    // Profile theme
    // Will change the background of your profile if it's public
    // Default | Dark | Light | Gradient
    profile_profileTheme: "Default",

    // Prestige Styling (Public Profile only)
    // If you have prestige unlocked - this can allow your profile name to change its style
    profile_usePrestigeStyling: true,

    // Maximum raids to retry to connect to the leaderboard before pausing the mod
    connectionRetries: 1,

    // Debug
    DEBUG: true,

    // DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.
    // Domain (or both subdomain + domain) used for PHP requests
    PHP_ENDPOINT: "visuals.nullcore.net",

    // Path to PHP file that proccess data incoming from SPT mod
    // Example - domain.com/spt_profile_server.php <-- then put "/spt_profile_server.php"
    PHP_PATH: "/hidden/SPT_Profiles_Backend.php"
};