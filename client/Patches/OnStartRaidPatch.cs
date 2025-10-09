using System;
using System.Reflection;
using SPT.Reflection.Patching;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;
using UnityEngine;

namespace SPTLeaderboard.Patches
{
    internal class OnStartRaidPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Class308).GetMethod(
                "LocalRaidStarted",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPrefix]
        static bool Prefix()
        {
            if (!SettingsModel.Instance.EnableSendData.Value)
                return true;
            
            HitsTracker.Instance.Clear();

            LeaderboardPlugin.Instance.TrackingLoot.OnStartRaid();
            
            LeaderboardPlugin.Instance.CreateIconPlayer();
            LeaderboardPlugin.Instance.StartInRaidHeartbeat();
            LeaderboardPlugin.logger.LogWarning("[State] Player started raid");
            PlayerHelper.Instance.LastDeathPosition = Vector3.zero;
            return true;
        }
    }
}