using UnityEngine;
using System.IO;
namespace SPTLeaderboard.Utils
{
    public class SpriteSaver : MonoBehaviour
    {
        public void SaveSpriteAsPNG(string filePath, Sprite spriteToSave)
        {
            Texture2D texture = spriteToSave.texture;
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngData);
            LeaderboardPlugin.logger.LogWarning($"Saved player icon in {filePath}");
        }
    }
}
