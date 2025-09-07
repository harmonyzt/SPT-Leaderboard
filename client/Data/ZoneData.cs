using UnityEngine;

namespace SPTLeaderboard.Data;

[System.Serializable]
public class ZoneData
{
    public string Name;
    public Vector3 Center;
    public Vector3 Size;

    public Bounds GetBounds()
    {
        return new Bounds(Center, Size);
    }
}