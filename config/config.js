module.exports = {
    // If you want to share more Profile SPT stats with anyone and the leaderboard - set to true
    public_profile: true,

    // If you want to display other name rather than your SPT profile name, put it here, or leave blank
    // Maximum - 15 characters
    profile_customName: "",

    // Your Team Tag
    // MIN: 3 characters | MAX: 6 characters
    // You'll be assigned to a team to participate in Teams and further events
    // Your team tag will always have [team tag] at both sides 
    profile_teamTag: "",

    // Your about me to show for public profile. Maximum - 80 characters
    // Your cap ends here -                                                                          ↓ Here
    profile_aboutMe: "I didn't look at the configs of this mod!",

    // Profile picture
    // Optimal aspect ratio: 1:1 (500x500, 250x250 etc)
    // Supports: .png .gif
    // Allowed domains: i.imgur.com | tenor.com | media1.tenor.com
    profile_profilePicture: "https://i.imgur.com/z15FrrK.png",

    // Profile theme
    // Will change the background of your profile if it's public
    // Shaded/Gradient themes don't support decals
    // default | dark | light | gradient | redshade | steelshade
    profile_profileTheme: "default",

    // Prestige Styling
    // Only if you have prestige unlocked
    // Also will ignore your current background set in bp_mainBackgroundReward
    profile_usePrestigeStyling: true,

    // none | killa | tagilla | both
    // Set your prestige background behind your profile!
    bp_usePrestigeBackground: "killa",

    ////////////////////////////////////////
    // BATTLEPASS FEATURES
    ////////////////////////////////////////

    // Choose a reward that'll be automatically chosen once you reach the required battlepass level (see your leaderboard profile)
    
    // None - default
    // 4 LVL - streets 
    // 7 LVL - streets2
    // 10 LVL - streets3
    // 15 LVL - purple
    // 25 LVL - labs
    bp_backgroundReward: "default",

    // Sets a background behind profile card
    // LVL 10 - usec | bear | default
    bp_mainBackgroundReward: "default",

    // Add snoozing cat on your profile! :3
    // LVL 20
    bp_catReward: true,

    // Choose a profile picture display style (this will make your profile picture wider or in box)
    // 5 LVL - box
    // 10 LVL - wide
    bp_pfpStyle: "default",

    // Choose a profile picture border color
    // 5 LVL - red
    // 8 LVL - pink
    // 10 LVL - white
    // 20 LVL - black
    bp_pfpBorder: "default",

    // Decals for your profile background
    // They're transparent and work off WEAPON MASTERY LEVELS IN YOUR LEADERBOARD PROFILE.
    // For them to work you should have Stattrack mod installed
    // 5 LVL - scratches
    // 10 LVL - cult-circle
    // 15 LVL - cult-signs
    // 20 LVL - cult-signs2
    bp_decal: "scratches",

    // Enable mod support to send extra data for your profile
    // Mod automatically detects mods that it supports
    // Currently supports: 
    // Stattrack by AcidPhantasm (extra weapon stats at battlepass tab and weapon mastery)
    enable_mod_support: true,

    // Maximum raids to retry to connect to the leaderboard if it failed for first time
    connectionRetries: 1,

    // Timeout for requesting data sent to the server, in MILLISECONDS
    // EXAMPLE: 10000 - 10 seconds | 20000 - 20 seconds
    connectionTimeout: 15000,

    // DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.
    // Domain (or both subdomain + domain) used for PHP requests
    PHP_ENDPOINT: "visuals.nullcore.net",

    // Path to PHP file that process incoming data from this mod
    // Example - domain.com/spt_profile_server.php <-- in this case you put "/spt_profile_server.php"
    PHP_PATH: "/hidden/SPT_Profiles_Backend.php",

    // Display debug messages in console and log them inside SPT server .log file
    DEBUG: false,
};