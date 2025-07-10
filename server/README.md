# SPT Leaderboard Mod

A mod for Single Player Tarkov (SPT) that tracks and displays player statistics on an online leaderboard.

## Features

- **Automatic Stat Tracking**: Records raid results, kills, damage, and other performance stats
- **Public/Private Profiles**: Choose whether to display your stats publicly
- **Achievement Tracking**: Your most recent achievement with details
- **Detailed Statistics**: Tracks:
  - PMC and SCAV levels
  - Raid results (Survived/Died/Runner)
  - Damage dealt
  - Longest kill shot
  - Win streaks
  - Play time
  - And more!

## Installation and How it works

1. Download the latest release
2. Extract the folder into your `Root SPT game` directory
3. Launch SPT Server to generate your unique token
4. Finish a raid

## Configuration

Edit `config/config.js` to customize:

```json
{
  "public_profile": true,
  "connectionRetries": 1,
  "profile_aboutMe": "Your profile description",
  "profile_profilePicture": "URL to your profile picture",
  "profile_profileTheme": "Darker",
  "profile_usePrestigeStyling": true
}
