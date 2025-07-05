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

		private SettingsModel(ConfigFile configFile)
		{
			KeyBind = configFile.Bind(
				"Settings", 
				"Test key bind", 
				new KeyboardShortcut(KeyCode.LeftArrow), 
				new ConfigDescription(
					"Just keybind for test requests",
					null, 
					new ConfigurationManagerAttributes
					{
						Order = 0
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