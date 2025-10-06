using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using SPT.Reflection.Utils;
using SPTLeaderboard.Data;
using SPTLeaderboard.Models;
using UnityEngine;

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

    public Vector3 LastDeathPosition { get; set; } = Vector3.zero;

    public static Vector3 ConvertToMapPosition(Vector3 unityPosition)
    {
        return new Vector3(unityPosition.x, unityPosition.z, unityPosition.y);
    }
    
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

        var capacity = item.Grids.Sum(CompoundItem.Class2341.class2341_0.method_10);
#if DEBUG || BETA
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

        var capacity = item.Grids.Sum(CompoundItem.Class2341.class2341_0.method_10);
#if DEBUG || BETA
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
            LocalizationModel.NotificationWarning(
                LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "TacticalVest"));

        if (input.Pockets > GlobalData.EquipmentLimits.Pockets)
            LocalizationModel.NotificationWarning(
                LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "Pockets"));

        if (input.Backpack > GlobalData.EquipmentLimits.Backpack)
            LocalizationModel.NotificationWarning(
                LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "Backpack"));

        if (input.SecuredContainer > GlobalData.EquipmentLimits.SecuredContainer)
            LocalizationModel.NotificationWarning(
                LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "SecuredContainer"));
        
        if (input.Stash > GlobalData.EquipmentLimits.Stash)
            LocalizationModel.NotificationWarning(
                LocalizationModel.Instance.GetLocaleErrorText(ErrorType.CAPACITY, "STASH"));
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
        GClass788 agressorData = profile.EftStats.Aggressor;
        if (agressorData != null)
        {
                    
            if (((GInterface214)agressorData).ProfileId != profile.Id)
            {
                if (((GInterface214)agressorData).ProfileId == "66f3fad50ec64d74847d049d")
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

