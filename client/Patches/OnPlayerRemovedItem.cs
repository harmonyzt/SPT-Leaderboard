using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    public class OnPlayerRemovedItem: ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("Player"), "OnItemRemoved");

        [PatchPostfix]
        static void Postfix(object __instance, GEventArgs3 eventArgs)
        {
            if (ReferenceEquals(__instance, PlayerHelper.Instance.Player))
            {
                foreach (var item in eventArgs.Item.GetAllItems())
                {
                    LeaderboardPlugin.Instance.TrackingLoot.Remove(item);
                }
            }
        }
    }
}