using BepInEx.Configuration;
using UnityEngine;

namespace SPTLeaderboard.Models
{
	/// <summary>
	/// Model with config fields
	/// </summary>
	public class SettingsModel
	{
		public static SettingsModel Instance { get; private set; }
		
		public ConfigEntry<KeyboardShortcut> KeyBind;
		public ConfigEntry<KeyboardShortcut> KeyBindTwo;
		public ConfigEntry<bool> ModCasualMode;
		public ConfigEntry<bool> PublicProfile;
		public ConfigEntry<bool> EnableModSupport;
		public ConfigEntry<int> ConnectionRetries;
		public ConfigEntry<int> ConnectionTimeout;
		public ConfigEntry<string> PhpEndpoint;
		public ConfigEntry<string> PhpPath;
		public ConfigEntry<bool> Debug;
		public ConfigEntry<int> SupportInRaidConnectionTimer;

		private SettingsModel(ConfigFile configFile)
		{
			#region TEST
				
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
			#endregion
			
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
				"/SPT/testEnv/api/", 
				new ConfigDescription(
					"DO NOT TOUCH UNLESS YOU KNOW WHAT YOU ARE DOING.\n Domain (or both subdomain + domain) used for PHP requests",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 2,
						IsAdvanced = true
					}));
			
			Debug = configFile.Bind(
				"Settings", 
				"Debug", 
				false, 
				new ConfigDescription(
					"Display debug messages in console and log them inside SPT server .log file",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 1
					}));
			SupportInRaidConnectionTimer = configFile.Bind(
				"Settings", 
				"Support In Raid Connection Timer", 
				20, 
				new ConfigDescription(
					"Timer for requests in server for support status IN_RAID",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 0,
						IsAdvanced = true
					}));
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