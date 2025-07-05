using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SPTLeaderboard.Models
{
    public class EncryptionModel
    {
        public static EncryptionModel Instance { get; private set; }
        
        private string _token = "";
        
        public string Token => _token;
        private string PathToken => Path.Combine(BepInEx.Paths.PluginPath, "SPT-Leaderboard", "secret.token");

        private EncryptionModel()
        {
            try
            {
                if (!File.Exists(PathToken))
                {
                    _token = GenerateToken();
                    WriteTokenToFile(_token);

                    LeaderboardPlugin.logger.LogWarning(
                        $"[SPT Leaderboard] Generated your secret token, see mod directory. WARNING: DO NOT SHARE IT WITH ANYONE! If you lose it, you will lose access to the Leaderboard until next season!");
                }
                else
                {
                    LeaderboardPlugin.logger.LogWarning(
                        $"[SPT Leaderboard] Your secret token was initialized by the mod. Remember to never show it to anyone!");
                    LoadToken();
                }
            }
            catch (Exception e)
            {
                LeaderboardPlugin.logger.LogError(
                    $"[SPT Leaderboard] Error handling token file: ${e.Message}");
                _token = GenerateToken();
            }
        }
        

        private void LoadToken()
        {
            _token = File.ReadAllText(PathToken);
        }
        
        private string GenerateToken()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return BitConverter.ToString(tokenData).Replace("-", "").ToLower();
            }
        }
        
        private void WriteTokenToFile(string token)
        {
            _token = token;
            File.WriteAllText(PathToken, token);
        }
        
        public static EncryptionModel Create()
        {
            if (Instance != null)
            {
                return Instance;
            }
            return Instance = new EncryptionModel();
        }

        
    }
}