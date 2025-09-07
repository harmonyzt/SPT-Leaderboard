using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SPTLeaderboard;
using SPTLeaderboard.Data;
using SPTLeaderboard.Models;
using SPTLeaderboard.Utils;
using UnityEngine;

public class ZoneTracker: MonoBehaviour
{
    private static ZoneTracker _instance;
    public static ZoneTracker Instance => _instance ??= new ZoneTracker();
    
    public string CurrentZone { get; private set; }
    
    private Dictionary<string, List<ZoneData>> allZones;
    
    public List<ZoneData> Zones = new List<ZoneData>();
    public List<ZoneData> ZonesEntered = new List<ZoneData>();
    private List<LineRenderer> debugViews = new List<LineRenderer>();

    public void Enable()
    {
        LeaderboardPlugin.Instance.FixedTick += CheckPlayerPosition;
#if DEBUG
        LeaderboardPlugin.Instance.Tick += CheckInput;
#endif        
        LoadZones(DataUtils.GetPrettyMapName(DataUtils.GetRaidRawMap()));
        
        foreach (var zone in Zones)
        {
            DrawZone(zone.Size, zone.Center);
        }
    }

    public void CheckInput()
    {
        if (SettingsModel.Instance.KeyBind.Value.IsDown())
        {
            LoadZones(DataUtils.GetPrettyMapName(DataUtils.GetRaidRawMap()));
        }

        if (SettingsModel.Instance.KeyBindTwo.Value.IsDown())
        {
            Redraw();
        }
    }

    public void Disable()
    {
        LeaderboardPlugin.Instance.FixedTick -= CheckPlayerPosition;
#if DEBUG
        LeaderboardPlugin.Instance.Tick -= CheckInput;
#endif        
        foreach (var debugView in debugViews)
        {
            Destroy(debugView.gameObject);
        }

        debugViews = null;
        Zones = null;
        ZonesEntered = null;
        allZones = null;
        CurrentZone = null;
    }
    
    public void DrawZone(Vector3 Size, Vector3 Center)
    {
        Vector3 half = Size / 2;
        
        Vector3[] corners = new Vector3[8];
        corners[0] = Center + new Vector3(-half.x, -half.y, -half.z);
        corners[1] = Center + new Vector3(-half.x, -half.y,  half.z);
        corners[2] = Center + new Vector3( half.x, -half.y,  half.z);
        corners[3] = Center + new Vector3( half.x, -half.y, -half.z);

        corners[4] = Center + new Vector3(-half.x,  half.y, -half.z);
        corners[5] = Center + new Vector3(-half.x,  half.y,  half.z);
        corners[6] = Center + new Vector3( half.x,  half.y,  half.z);
        corners[7] = Center + new Vector3( half.x,  half.y, -half.z);

        int[,] edges = new int[,]
        {
            {0,1}, {1,2}, {2,3}, {3,0}, // низ
            {4,5}, {5,6}, {6,7}, {7,4}, // верх
            {0,4}, {1,5}, {2,6}, {3,7}  // вертикали
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            var go = new GameObject($"Edge{i}");
            var lr = go.AddComponent<LineRenderer>();
            debugViews.Add(lr);
            lr.widthMultiplier = 0.05f;
            lr.positionCount = 2;

            lr.SetPosition(0, corners[edges[i, 0]]);
            lr.SetPosition(1, corners[edges[i, 1]]);
        }
    }
    
    public void LoadZones(string mapName)
    { 
        if (!File.Exists(GlobalData.ZonesConfig))
        {
            LeaderboardPlugin.logger.LogWarning($"[ZoneManager] zones.json не найден: {GlobalData.ZonesConfig}");
            return;
        }

        string json = File.ReadAllText(GlobalData.ZonesConfig);
        try
        {
            allZones = JsonConvert.DeserializeObject<Dictionary<string, List<ZoneData>>>(json);

            if (allZones != null && allZones.ContainsKey(mapName))
            {
                Zones = allZones[mapName];
                LeaderboardPlugin.logger.LogWarning($"[ZoneManager] Загружено {Zones.Count} зон для карты {mapName}");
            }
            else
            {
                Zones.Clear();
                LeaderboardPlugin.logger.LogWarning($"[ZoneManager] Для карты {mapName} зон не найдено.");
            }
        }
        catch (System.Exception ex)
        {
            LeaderboardPlugin.logger.LogError($"[ZoneManager] Ошибка чтения JSON: {ex.Message}");
        }
    }

    public void Redraw()
    {
        foreach (var debugView in debugViews)
        {
            Destroy(debugView.gameObject);
        }

        debugViews.Clear();
        foreach (var zone in Zones)
        {
            DrawZone(zone.Size, zone.Center);
        }
    }

    public void CheckPlayerPosition()
    {
        if (PlayerHelper.Instance.Player)
        {
            CheckPlayerPosition(PlayerHelper.Instance.Player.PlayerBones.transform.position);
        }
    }
    
    public void CheckPlayerPosition(Vector3 pos)
    {
        foreach (var zone in Zones)
        {
            if (zone.GetBounds().Contains(pos))
            {
                if (CurrentZone != zone.Name)
                {
                    CurrentZone = zone.Name;
                    if (!ZonesEntered.Contains(zone))
                    {
                        ZonesEntered.Add(zone);
                    }
                    #if DEBUG || BETA
                    if (SettingsModel.Instance.Debug.Value)
                    {
                        LeaderboardPlugin.logger.LogWarning($"Игрок вошёл в зону: {zone.Name}");
                    }
                    #endif
                }
                return;
            }
        }

        if (CurrentZone != null)
        {
            CurrentZone = null;
#if DEBUG || BETA
            if (SettingsModel.Instance.Debug.Value)
            {
                LeaderboardPlugin.logger.LogWarning($"Игрок покинул зону: {CurrentZone}");
            }
#endif
        }
    }
}