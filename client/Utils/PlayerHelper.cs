using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;
using SPTLeaderboard.Models;

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

    #region Equipment

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
        equipmentData.Stash = GetStashCapacity(pmcData);

        return equipmentData;
    }

    /// <summary>
    /// Get capacity slot
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    private static int GetSlotCapacity(Profile pmcData, EquipmentSlot slot)
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
    
    /// <summary>
    /// Get capacity stash player
    /// </summary>
    /// <param name="pmcData"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    private static int GetStashCapacity(Profile pmcData)
    {
        var item = pmcData.Inventory.Stash as CompoundItem;
        if (item == null)
            return 0;

        var capacity = item.Grids.Sum(CompoundItem.Class2227.class2227_0.method_10);
#if DEBUG
        LeaderboardPlugin.logger.LogWarning($"Size Stash {capacity}");
#endif
        return capacity;
    }
    
    /// <summary>
    /// Check limit violation capacity
    /// </summary>
    /// <param name="input"></param>
    public static void GetLimitViolations(EquipmentData input)
    {
        if (input.TacticalVest > GlobalData.EquipmentLimits.TacticalVest)
                NotificationManagerClass.DisplayWarningNotification(
                    LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "TacticalVest"), 
                    ENotificationDurationType.Long);

        if (input.Pockets > GlobalData.EquipmentLimits.Pockets)
                NotificationManagerClass.DisplayWarningNotification(
                    LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "Pockets"), 
                    ENotificationDurationType.Long);

        if (input.Backpack > GlobalData.EquipmentLimits.Backpack)
                NotificationManagerClass.DisplayWarningNotification(
                    LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "Backpack"), 
                    ENotificationDurationType.Long);

        if (input.SecuredContainer > GlobalData.EquipmentLimits.SecuredContainer)
                NotificationManagerClass.DisplayWarningNotification(
                    LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "SecuredContainer"), 
                    ENotificationDurationType.Long);
        
        if (input.Stash > GlobalData.EquipmentLimits.Stash)
            NotificationManagerClass.DisplayWarningNotification(
                LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "STASH"), 
                ENotificationDurationType.Long);
    }
    
    /// <summary>
    /// Check limit violation capacity silent
    /// </summary>
    /// <param name="input"></param>
    public static bool GetLimitViolationsSilent(EquipmentData input)
    {
        if (input.TacticalVest > GlobalData.EquipmentLimits.TacticalVest) { return true; }

        if (input.Pockets > GlobalData.EquipmentLimits.Pockets) { return true; }

        if (input.Backpack > GlobalData.EquipmentLimits.Backpack) { return true; }

        if (input.SecuredContainer > GlobalData.EquipmentLimits.SecuredContainer) { return true; }
        
        return false;
    }
    
    #endregion

    public static string TryGetAgressorName(Profile profile)
    {
        string nameKiller = "";
        GClass767 agressorData = profile.EftStats.Aggressor;
        if (agressorData != null)
        {
                    
            if (((GInterface187)agressorData).ProfileId != profile.Id)
            {
                if (((GInterface187)agressorData).ProfileId == "66f3fad50ec64d74847d049d")
                {
                    nameKiller = LocalizationModel.GetLocaleName(agressorData.Name, false);
                }
                else
                {
                    nameKiller = LocalizationModel.GetCorrectedNickname(agressorData);
                }
            }
                    
            LeaderboardPlugin.logger.LogWarning($"Agressor Name {nameKiller}\n");
            return nameKiller;
        }

        LeaderboardPlugin.logger.LogWarning($"Aggressor Data == null");
        return nameKiller;
    }
}

