using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    public class OnPlayerAddedItem: ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("Player"), "OnItemAdded");
    
        [PatchPostfix]
        static void Postfix(object __instance, GEventArgs1 eventArgs)
        {
            if (ReferenceEquals(__instance, PlayerHelper.Instance.Player))
            {
                foreach (var item in eventArgs.Item.GetAllItems())
                {
                    LeaderboardPlugin.Instance.TrackingLoot.Add(item);
                }
            }
        }

    }
}