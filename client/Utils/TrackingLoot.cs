using System;
using System.Collections.Generic;
using EFT.InventoryLogic;

namespace SPTLeaderboard.Utils;

// Taken from https://github.com/HiddenCirno/ShowLootValue
public class TrackingLoot
{
    public HashSet<string> TrackedIds = new HashSet<string>();

    public int LootedValue = 0;
    public int PreRaidLottValue = 0;

    public bool Add(Item item)
    {
        if (TrackedIds.Add(item.TemplateId.ToString()))
        {
            return true;
        }
        return false;
    }

    public bool Remove(Item item)
    {
        if (TrackedIds.Remove(item.TemplateId.ToString()))
        {
            return true;
        }
        return false;
    }

    private void Clear() => TrackedIds.Clear();

    public void OnStartRaid()
    {
        Clear();
        LootedValue = 0;
        PreRaidLottValue = 0;
        
        LeaderboardPlugin.Instance.BeforeRaidPlayerEquipment.Clear();
            
        foreach (var item in PlayerHelper.GetEquipmentItemsIds())
        {
            LeaderboardPlugin.Instance.BeforeRaidPlayerEquipment.Add(item);
        }
    }
}