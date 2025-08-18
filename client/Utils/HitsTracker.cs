using System;
using System.Collections.Generic;
using SPTLeaderboard.Data;

namespace SPTLeaderboard.Utils;

public class HitsTracker
{
    private static HitsTracker _instance;
    public static HitsTracker Instance => _instance ??= new HitsTracker();

    private HitsData _data = new HitsData();
    private List<float> _hitDistances = new List<float>();

    private HitsTracker() { }

    public void IncreaseHit(EBodyPart part)
    {
        switch (part)
        {
            case EBodyPart.Head:
                _data.Head++;
                break;
            case EBodyPart.Chest:
                _data.Chest++;
                break;
            case EBodyPart.Stomach:
                _data.Stomach++;
                break;
            case EBodyPart.LeftArm:
                _data.LeftArm++;
                break;
            case EBodyPart.RightArm:
                _data.RightArm++;
                break;
            case EBodyPart.LeftLeg:
                _data.LeftLeg++;
                break;
            case EBodyPart.RightLeg:
                _data.RightLeg++;
                break;
            case EBodyPart.Common:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(part), part, null);
        }
    }
    
    public void Clear()
    {
        _data = new HitsData();
        _hitDistances = new List<float>();
    }

    public HitsData GetHitsData()
    {
        return _data;
    }
    
    public void AddHit(float distance)
    {
        float roundedDistance = (float)Math.Round(distance, 1);
        LeaderboardPlugin.logger.LogInfo($"[HitsTracker] Add hit with distance {roundedDistance}");
        _hitDistances.Add(roundedDistance);
    }

    public List<float> GetHitDistances()
    {
        return _hitDistances;
    }
}