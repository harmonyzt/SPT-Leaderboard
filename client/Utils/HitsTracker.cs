using System;
using System.Collections.Generic;
using System.Linq;
using SPTLeaderboard.Data;

namespace SPTLeaderboard.Utils;

public class HitsTracker
{
    private static HitsTracker _instance;
    public static HitsTracker Instance => _instance ??= new HitsTracker();

    private HitsData data = new HitsData();
    private List<float> hitDistances = new List<float>();

    private HitsTracker() { }

    public void IncreaseHit(EBodyPart part)
    {
        switch (part)
        {
            case EBodyPart.Head:
                data.Head++;
                break;
            case EBodyPart.Chest:
                data.Chest++;
                break;
            case EBodyPart.Stomach:
                data.Stomach++;
                break;
            case EBodyPart.LeftArm:
                data.LeftArm++;
                break;
            case EBodyPart.RightArm:
                data.RightArm++;
                break;
            case EBodyPart.LeftLeg:
                data.LeftLeg++;
                break;
            case EBodyPart.RightLeg:
                data.RightLeg++;
                break;
            case EBodyPart.Common:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(part), part, null);
        }
    }
    
    public void Clear()
    {
        data = new HitsData();
        hitDistances = new List<float>();
    }

    public HitsData GetHitsData()
    {
        return data;
    }
    
    public float GetLongestShot()
    {
        return hitDistances.Count <= 0 ? 0f : hitDistances.Max();
    }
    
    public void AddHit(float distance)
    {
        float roundedDistance = (float)Math.Round(distance, 1);
#if DEBUG || BETA
        LeaderboardPlugin.logger.LogInfo($"[HitsTracker] Add hit with distance {roundedDistance}");
#endif
        hitDistances.Add(roundedDistance);
    }

    public List<float> GetHitDistances()
    {
        return hitDistances;
    }
}