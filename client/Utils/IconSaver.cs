using UnityEngine;
using System.IO;
using Comfort.Common;
using SPTLeaderboard.Data;

namespace SPTLeaderboard.Utils
{
    public class IconSaver : MonoBehaviour
    {
        private GClass907 _presetIcon;
        
        public void SaveSpriteAsPNG(string filePath, Sprite spriteToSave)
        {
            Texture2D texture = spriteToSave.texture;
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);
            LeaderboardPlugin.logger.LogWarning($"Saved player icon in {filePath}");
        }
        
        public void IconChangedState()
        {
            bool flag = _presetIcon.Sprite != null;
            LeaderboardPlugin.logger.LogWarning($"Is loaded icon? {flag}");

            if (flag)
            {
                LeaderboardPlugin.SendProfileIcon(_presetIcon);
                new IconSaver().SaveSpriteAsPNG(GlobalData.LeaderboardIconPath, _presetIcon.Sprite);
            }
        }

        public void Create()
        {
            var profile = PlayerHelper.GetProfile();
                
            XYCellSizeStruct textureSize = new XYCellSizeStruct(500, 500);
                
            _presetIcon = Singleton<GClass905>.Instance.method_11(new GClass910(profile.Inventory.Equipment.CloneVisibleItem(), profile.Customization), textureSize);
            
            if (_presetIcon.Sprite == null)
            {
                _presetIcon.Changed.Bind(IconChangedState);
            }
            else
            {
                LeaderboardPlugin.SendProfileIcon(_presetIcon);
                
                new IconSaver().SaveSpriteAsPNG(GlobalData.LeaderboardIconPath, _presetIcon.Sprite);
            }
        }
    }
}
