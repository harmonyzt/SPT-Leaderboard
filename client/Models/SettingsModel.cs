using BepInEx.Configuration;
#if DEBUG || BETA
using SPTLeaderboard.Utils;
using UnityEngine;
#endif

namespace SPTLeaderboard.Models
{
	/// <summary>
	/// Model with config fields
	/// </summary>
	public class SettingsModel
	{
		public static SettingsModel Instance { get; private set; }
		
#if DEBUG || BETA
		public ConfigEntry<KeyboardShortcut> KeyBind;
		public ConfigEntry<KeyboardShortcut> KeyBindTwo;
		public ConfigEntry<float> PositionXDebug;
		public ConfigEntry<float> PositionYDebug;
		public ConfigEntry<int> FontSizeDebug;
#endif
#if DEBUG
		public ConfigEntry<bool> Debug;
#endif
		
		public ConfigEntry<bool> EnableSendData;
		public ConfigEntry<bool> ShowPointsNotification;
		public ConfigEntry<bool> ModCasualMode;
		public ConfigEntry<bool> PublicProfile;
		public ConfigEntry<bool> EnableModSupport;
		public ConfigEntry<int> ConnectionTimeout;
		public ConfigEntry<string> PhpEndpoint;
		public ConfigEntry<string> PhpPath;
		public ConfigEntry<int> SupportInRaidConnectionTimer;

		private SettingsModel(ConfigFile configFile)
		{
#if DEBUG || BETA
			KeyBind = configFile.Bind(
				"2. Debug",
				"Test key bind 1", 
				new KeyboardShortcut(KeyCode.LeftArrow), 
				new ConfigDescription(
					"Just keybind for tests")); 
			
			KeyBindTwo = configFile.Bind(
				"2. Debug",
				"Test key bind 2", 
				new KeyboardShortcut(KeyCode.UpArrow), 
				new ConfigDescription(
					"Just keybind for tests"));

			PositionXDebug = configFile.Bind(
				"2. Debug",
				"PositionX",
				10f,
				new ConfigDescription("X Position", new AcceptableValueRange<float>(-2000f, 2000f)));

			PositionYDebug = configFile.Bind(
				"2. Debug",
				"PositionY",
				-10f,
				new ConfigDescription("Y Position", new AcceptableValueRange<float>(-2000f, 2000f)));
			
			FontSizeDebug = configFile.Bind(
				"2. Debug",
				"FontSizeDebug",
				28,
				new ConfigDescription("FontSizeDebug", new AcceptableValueRange<int>(0, 200)));
#endif
#if DEBUG
			Debug = configFile.Bind(
				"2. Debug",
				"Debug", 
				true,
				new ConfigDescription(
					"Display debug messages in console and log them inside SPT server .log file"));
#endif
			
			EnableSendData = configFile.Bind(
				"1. Settings", 
				"Is Sending Data", 
				true, 
				new ConfigDescription(
					"When disable, stops sending your scores and statistics to the leaderboard server",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 8
					}));
			
			ShowPointsNotification = configFile.Bind(
				"1. Settings", 
				"Show Notification Points", 
				true, 
				new ConfigDescription(
					"When turned on, display a notification about the issuance of leaderboard points at the end of the raid.",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 7
					}));
			
			ModCasualMode = configFile.Bind(
				"1. Settings", 
				"Casual mode", 
				false, 
				new ConfigDescription(
					"Enabling this will switch you to a Casual Mode.\n You will not be ranked in the leaderboard and your stats won't count towards its progress.\n You'll be free off any leaderboard restrictions (except reasonable ones), have access to raid history and your profile like usual.\n DANGER - Once you played with this ON - YOU CANT GET BACK INTO RANKING.",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 6
					}));
			
			PublicProfile = configFile.Bind(
				"1. Settings", 
				"Public Profile", 
				true, 
				new ConfigDescription(
					"If you want to share more Profile SPT stats with anyone and the leaderboard - set to true\n This also allows your server to send heartbeats to API",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 5
					}));
			
			EnableModSupport = configFile.Bind(
				"1. Settings", 
				"Mod Support", 
				true, 
				new ConfigDescription(
					"Enable mod support to send extra data for your profile\n Mod automatically detects mods that it supports\n Currently supports: \n Stattrack by AcidPhantasm (extra weapon stats at battlepass tab and weapon mastery)",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 4
					}));
			
			ConnectionTimeout = configFile.Bind(
				"1. Settings", 
				"Connection Timeout", 
				10, 
				new ConfigDescription(
					"How long mod will be waiting for the response from Leaderboard API, in SECONDS",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 3,
						IsAdvanced = true
					}));
			
			PhpEndpoint = configFile.Bind(
				"1. Settings", 
				"PHP Endpoint", 
				"sptlb.yuyui.moe", 
				new ConfigDescription(
					"DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.\n Domain (or both subdomain + domain) used for PHP requests",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 2,
						IsAdvanced = true
					}));
			
			PhpPath = configFile.Bind(
				"1. Settings", 
				"PHP Path", 
				"/api/main/", 
				new ConfigDescription(
					"DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.\n Domain (or both subdomain + domain) used for PHP requests",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 1,
						IsAdvanced = true
					}));
			
			SupportInRaidConnectionTimer = configFile.Bind(
				"1. Settings", 
				"Support In Raid Connection Timer", 
				60, 
				new ConfigDescription(
					"Timer for requests in server for support status IN_RAID",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 0,
						IsAdvanced = true
					}));
			
			#if DEBUG || BETA
			PositionXDebug.SettingChanged += (_, __) =>
			{
				OverlayDebug.Instance.SetOverlayPosition(new Vector2(PositionXDebug.Value, PositionYDebug.Value));
			};
			
			PositionYDebug.SettingChanged += (_, __) =>
			{
				OverlayDebug.Instance.SetOverlayPosition(new Vector2(PositionXDebug.Value, PositionYDebug.Value));
			};

			FontSizeDebug.SettingChanged += (_, __) =>
			{
				OverlayDebug.Instance.SetFontSize(FontSizeDebug.Value);
			};
			#endif
		}
		
		/// <summary>
		/// Init configs model
		/// </summary>
		/// <param name="configFile"></param>
		/// <returns></returns>
		public static SettingsModel Create(ConfigFile configFile)
		{
			if (Instance != null)
			{
				return Instance;
			}
			return Instance = new SettingsModel(configFile);
		}
	}
}