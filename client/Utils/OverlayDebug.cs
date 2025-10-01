#if DEBUG || BETA
using System;
using SPTLeaderboard.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SPTLeaderboard.Utils;

public class OverlayDebug: MonoBehaviour
{
    private static OverlayDebug _instance;
    public static OverlayDebug Instance => _instance ??= new OverlayDebug();
    
    private TextMeshProUGUI _overlayText;
    private GameObject _overlay;
    
    public void Enable()
    {
        _instance = this;
        
        _overlay = new GameObject("[SPTLeaderboard] Overlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = _overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = _overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        var textObj = new GameObject("[SPTLeaderboard] OverlayText", typeof(RectTransform));
        textObj.transform.SetParent(_overlay.transform, false);

        _overlayText = textObj.AddComponent<TextMeshProUGUI>();
        _overlayText.text = "Overlay initialized";
        _overlayText.fontSize = SettingsModel.Instance.FontSizeDebug.Value;
        _overlayText.color = Color.white;
        _overlayText.alignment = TextAlignmentOptions.TopLeft;
        _overlayText.enableWordWrapping = false;

        var rectTransform = _overlayText.rectTransform;
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.sizeDelta = new Vector2(800, 200);
        
        SetOverlayPosition(new Vector2(SettingsModel.Instance.PositionXDebug.Value, SettingsModel.Instance.PositionYDebug.Value));
    }
    
    public void UpdateOverlay()
    {
        if (_overlayText == null) return;
        
        var currentHitsData = HitsTracker.Instance.GetHitsData();
        var longestShot = HitsTracker.Instance.GetLongestShot();
        
        _overlayText.text = string.Format(
                "Raid hits:\nHead -> {0}\nChest -> {1}\nStomach -> {2}\nLeftArm -> {3}\nRightArm -> {4}\nLeftLeg -> {5}\nRightLeg -> {6}\n \n \n LongestShot -> {7}",
            currentHitsData.Head, currentHitsData.Chest, currentHitsData.Stomach, currentHitsData.LeftArm,
            currentHitsData.RightArm, currentHitsData.LeftLeg, currentHitsData.RightLeg, longestShot);
    }

    public void SetOverlayPosition(Vector2 anchoredPosition)
    {
        if (_overlayText != null)
            _overlayText.rectTransform.anchoredPosition = anchoredPosition;
    }
    
    public void SetFontSize(int size)
    {
        if (_overlayText != null)
            _overlayText.fontSize = size;
    }

    public void Disable()
    {
        Destroy(_overlay);
        Destroy(this);
    }
}
#endif