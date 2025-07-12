using System.Reflection;
using System.Threading.Tasks;
using EFT;
using SPT.Reflection.Patching;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches
{
    public class OnInitPlayerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Player).GetMethod(
                "Init",
                BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        static async void PostFix(Player __instance, Task __result)
        {
            await __result;
            if (__instance.IsYourPlayer)
            {
                PlayerHelper.Instance.Player = __instance;
            }
        }
    }
}