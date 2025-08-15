using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPTLeaderboard.Data;

namespace SPTLeaderboard.Patches
{
	public class LeaderboardVersionLabelPatch : ModulePatch
	{
		protected override MethodBase GetTargetMethod()
		{
			return typeof(PreloaderUI).GetMethod("method_6");
		}

		[PatchPrefix]
		static bool Prefix(PreloaderUI __instance)
		{
			string string_2 = (string)AccessTools.Field(typeof(PreloaderUI), "string_2").GetValue(__instance);
			string string_3 = (string)AccessTools.Field(typeof(PreloaderUI), "string_3").GetValue(__instance);
			string string_4 = (string)AccessTools.Field(typeof(PreloaderUI), "string_4").GetValue(__instance);
			string string_5 = (string)AccessTools.Field(typeof(PreloaderUI), "string_5").GetValue(__instance);

			string str = string_2;

			if (!string.IsNullOrEmpty(string_3))
				str = str + " | " + string_3;
			if (!string.IsNullOrEmpty(string_5))
				str = str + " | " + string_5;
			if (!string.IsNullOrEmpty(string_4))
				str = str + " | " + string_4;
#if DEBUG
			str = str + " | " + $"SPT Leaderboard {GlobalData.Version} [DEBUG] - {GlobalData.SubVersion}";
#elif BETA
			str = str + " | " + $"SPT Leaderboard {GlobalData.Version} [BETA] - {GlobalData.SubVersion}";
#else
			str = str + " | " + "SPT Leaderboard {GlobalData.Version}";
#endif
			
			var labelField = AccessTools.Field(typeof(PreloaderUI), "_alphaVersionLabel");
			var label = labelField.GetValue(__instance);

			var locKeyProperty = label.GetType().GetProperty("LocalizationKey");
			locKeyProperty?.SetValue(label, str);

			return false;
		}
	}
}