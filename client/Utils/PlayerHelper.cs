using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;

namespace SPTLeaderboard.Utils;

public class PlayerHelper
{
    private static PlayerHelper _instance;

    public static PlayerHelper Instance => _instance ??= new PlayerHelper();
    
    public static ISession GetSession(bool throwIfNull = false)
    {
        var session = ClientAppUtils.GetClientApp().Session;

        if (throwIfNull && session is null)
        {
            LeaderboardPlugin.logger.LogWarning("Trying to access the Session when it's null");
        }

        return session;
    }
    
    public static Profile GetProfile(bool throwIfNull = false)
    {
        var profile = GetSession()?.Profile;

        if (throwIfNull && profile is null)
        {
            LeaderboardPlugin.logger.LogWarning("Trying to access the Profile when it's null");
        }
        
        return GetSession()?.Profile;
    }
    
    public static bool HasRaidStarted()
    {
        bool? inRaid = Singleton<AbstractGame>.Instance?.InRaid;
        return inRaid.HasValue && inRaid.Value;
    }
    
    public Player Player { get; set; }

    /// <summary>
    /// Calculate capacity for each main equipment items
    /// </summary>
    /// <returns></returns>
    public static EquipmentData GetEquipmentData()
    {
        var equipmentData = new EquipmentData();

        var pmcData = GetSession().GetProfileBySide(ESideType.Pmc);
        if (pmcData == null)
            return equipmentData;

        equipmentData.TacticalVest = GetSlotCapacity(pmcData, EquipmentSlot.TacticalVest);
        equipmentData.Pockets = GetSlotCapacity(pmcData, EquipmentSlot.Pockets);
        equipmentData.Backpack = GetSlotCapacity(pmcData, EquipmentSlot.Backpack);
        equipmentData.SecuredContainer = GetSlotCapacity(pmcData, EquipmentSlot.SecuredContainer);

        return equipmentData;
    }

    /// <summary>
    /// Get capacity for slot in equipment
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    public static int GetSlotCapacity(Profile pmcData, EquipmentSlot slot)
    {
        var item = pmcData.Inventory.Equipment.GetSlot(slot).ContainedItem as CompoundItem;
        if (item == null)
            return 0;

        var capacity = item.Grids.Sum(CompoundItem.Class2227.class2227_0.method_10);
#if DEBUG
        LeaderboardPlugin.logger.LogWarning($"Size {slot.ToString()} {capacity}");
#endif
        return capacity;
    }
}

