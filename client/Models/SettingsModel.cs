using BepInEx.Configuration;
using SPTLeaderboard.Utils;
using UnityEngine;

namespace SPTLeaderboard.Models
{
	/// <summary>
	/// Model with config fields
	/// </summary>
	public class SettingsModel
	{
		public static SettingsModel Instance { get; private set; }
		
#if DEBUG
		public ConfigEntry<KeyboardShortcut> KeyBind;
		public ConfigEntry<KeyboardShortcut> KeyBindTwo;
		public ConfigEntry<float> PositionXDebug;
		public ConfigEntry<float> PositionYDebug;
		public ConfigEntry<int> FontSizeDebug;
		public ConfigEntry<bool> Debug;
#endif
		
		public ConfigEntry<bool> EnableSendData;
		public ConfigEntry<bool> ModCasualMode;
		public ConfigEntry<bool> PublicProfile;
		public ConfigEntry<bool> EnableModSupport;
		public ConfigEntry<int> ConnectionRetries;
		public ConfigEntry<Vector2> IconSize;
		public ConfigEntry<int> ConnectionTimeout;
		public ConfigEntry<string> PhpEndpoint;
		public ConfigEntry<string> PhpPath;
		public ConfigEntry<int> SupportInRaidConnectionTimer;

		private SettingsModel(ConfigFile configFile)
		{
#if DEBUG
				
			KeyBind = configFile.Bind(
				"Settings", 
				"Test key bind", 
				new KeyboardShortcut(KeyCode.LeftArrow), 
				new ConfigDescription(
					"Just keybind for test requests")); 
			
			KeyBindTwo = configFile.Bind(
				"Settings", 
				"Test key bind2", 
				new KeyboardShortcut(KeyCode.UpArrow), 
				new ConfigDescription(
					"Just keybind for test requests"));

			PositionXDebug = configFile.Bind(
				"Settings",
				"PositionX",
				10f,
				new ConfigDescription("X Position", new AcceptableValueRange<float>(-2000f, 2000f)));

			PositionYDebug = configFile.Bind(
				"Settings",
				"PositionY",
				-10f,
				new ConfigDescription("Y Position", new AcceptableValueRange<float>(-2000f, 2000f)));
			
			FontSizeDebug = configFile.Bind(
				"Settings",
				"FontSizeDebug",
				28,
				new ConfigDescription("FontSizeDebug", new AcceptableValueRange<int>(0, 200)));
			
			Debug = configFile.Bind(
				"Settings", 
				"Debug", 
				true, //TODO: Set to false. BEFORE PROD
				new ConfigDescription(
					"Display debug messages in console and log them inside SPT server .log file",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 1
					}));
#endif
			
			IconSize = configFile.Bind(
				"Settings", 
				"Icon Size", 
				new Vector2(500f, 450f), 
				new ConfigDescription(
					"Size for icon save...",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 10
					}));
			
			EnableSendData = configFile.Bind(
				"Settings", 
				"Is Sending Data", 
				true, 
				new ConfigDescription(
					"When disable, stops sending your scores and statistics to the leaderboard server",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 9
					}));
			
			ModCasualMode = configFile.Bind(
				"Settings", 
				"Casual mode", 
				false, 
				new ConfigDescription(
					"Enabling this will switch you to a Casual Mode.\n You will not be ranked in the leaderboard and your stats won't count towards its progress.\n You'll be free off any leaderboard restrictions (except reasonable ones), have access to raid history and your profile like usual.\n DANGER - Once you played with this ON - YOU CANT GET BACK INTO RANKING.",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 8
					}));
			
			PublicProfile = configFile.Bind(
				"Settings", 
				"Public Profile", 
				true, 
				new ConfigDescription(
					"If you want to share more Profile SPT stats with anyone and the leaderboard - set to true\n This also allows your server to send heartbeats to API",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 7
					}));
			
			EnableModSupport = configFile.Bind(
				"Settings", 
				"Mod Support", 
				true, 
				new ConfigDescription(
					"Enable mod support to send extra data for your profile\n Mod automatically detects mods that it supports\n Currently supports: \n Stattrack by AcidPhantasm (extra weapon stats at battlepass tab and weapon mastery)",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 6
					}));
			
			ConnectionRetries = configFile.Bind(
				"Settings", 
				"Connection Retries", 
				1, 
				new ConfigDescription(
					"Maximum raids to retry to connect to the Leaderboard API if it failed for first time",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 5,
						IsAdvanced = true
					}));
			
			ConnectionTimeout = configFile.Bind(
				"Settings", 
				"Connection Timeout", 
				10, 
				new ConfigDescription(
					"How long mod will be waiting for the response from Leaderboard API, in SECONDS",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 4,
						IsAdvanced = true
					}));
			
			PhpEndpoint = configFile.Bind(
				"Settings", 
				"PHP Endpoint", 
				"visuals.nullcore.net", 
				new ConfigDescription(
					"DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.\n Domain (or both subdomain + domain) used for PHP requests",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 3,
						IsAdvanced = true
					}));
			
			PhpPath = configFile.Bind(
				"Settings", 
				"PHP Path", 
				"/SPT/api/", 
				new ConfigDescription(
					"DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.\n Domain (or both subdomain + domain) used for PHP requests",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 2,
						IsAdvanced = true
					}));
			
			SupportInRaidConnectionTimer = configFile.Bind(
				"Settings", 
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
			
			#if DEBUG
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