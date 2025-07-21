using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using Capacity = EFT.InventoryLogic.CompoundItem.Class2228;
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
    
    public EftBattleUIScreen EftBattleUIScreen { get; set; }

    public static EquipmentData GetEquipmentData()
    {
        var equipmentData = new EquipmentData
        {
            TacticalVest = 0,
            Pockets = 0,
            Backpack = 0,
            SecuredContainer = 0
        };
        
        var pmcData = GetSession().GetProfileBySide(ESideType.Pmc);
        if (pmcData == null)
            return equipmentData;
                
        var tacticalVestData = pmcData.Inventory.Equipment.GetSlot(EquipmentSlot.TacticalVest).ContainedItem as CompoundItem;
        if (tacticalVestData != null)
        {
            var capacityContainer = new Capacity
            {
                gridsCapacity = tacticalVestData.Grids.Sum(CompoundItem.Class2227.class2227_0.method_10)
            };
            equipmentData.TacticalVest = capacityContainer.gridsCapacity;
#if DEBUG
            LeaderboardPlugin.logger.LogWarning($"Size TacticalVest {equipmentData.TacticalVest}");
#endif
        }
        
        var pocketsData = pmcData.Inventory.Equipment.GetSlot(EquipmentSlot.Pockets).ContainedItem as CompoundItem;
        if (pocketsData != null)
        {
            var capacityContainer = new Capacity
            {
                gridsCapacity = pocketsData.Grids.Sum(CompoundItem.Class2227.class2227_0.method_10)
            };
            equipmentData.Pockets = capacityContainer.gridsCapacity;
#if DEBUG
            LeaderboardPlugin.logger.LogWarning($"Size Pockets {equipmentData.Pockets}");
#endif
        }
        
        var backpackData = pmcData.Inventory.Equipment.GetSlot(EquipmentSlot.Backpack).ContainedItem as CompoundItem;
        if (backpackData != null)
        {
            var capacityContainer = new Capacity
            {
                gridsCapacity = backpackData.Grids.Sum(CompoundItem.Class2227.class2227_0.method_10)
            };
            equipmentData.Backpack = capacityContainer.gridsCapacity;
#if DEBUG
            LeaderboardPlugin.logger.LogWarning($"Size Backpack {equipmentData.Backpack}");
#endif
        }
        
        var securedContainerData = pmcData.Inventory.Equipment.GetSlot(EquipmentSlot.SecuredContainer).ContainedItem as CompoundItem;
        if (securedContainerData != null)
        {
            var capacityContainer = new Capacity
            {
                gridsCapacity = securedContainerData.Grids.Sum(CompoundItem.Class2227.class2227_0.method_10)
            };
            equipmentData.SecuredContainer = capacityContainer.gridsCapacity;
#if DEBUG
            LeaderboardPlugin.logger.LogWarning($"Size SecuredContainer {equipmentData.SecuredContainer}");
#endif
        }

        return equipmentData;
    }
}

