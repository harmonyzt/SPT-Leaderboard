using System.Reflection;
using EFT.UI;
using SPT.Reflection.Patching;
using SPTLeaderboard.Utils;

namespace SPTLeaderboard.Patches;

public class HookEftBattleUIScreenPatch: ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return (MethodBase)typeof(EftBattleUIScreen).GetConstructors()[0];
    }
    
    [PatchPostfix]
    static void PostFix(EftBattleUIScreen __instance)
    {
        PlayerHelper.Instance.EftBattleUIScreen = __instance;
    }
}