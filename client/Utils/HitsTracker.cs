using System;
using SPTLeaderboard.Data;

namespace SPTLeaderboard.Utils;

public class HitsTracker
{
    private static HitsTracker _instance;
    public static HitsTracker Instance => _instance ??= new HitsTracker();

    private HitsData _data = new HitsData();

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
    }

    public HitsData GetHitsData()
    {
        return _data;
    }
}