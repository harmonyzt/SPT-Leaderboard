using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace SPTLeaderboard.Models
{
    public class EncryptionModel
    {
        public static EncryptionModel Instance { get; private set; }
        
        private string _token = "";
        
        private const string ExpectedToken = "aca1ee24fea237dd";

        private bool isEditedDll = false;
        
        public string Token => _token;
        
        private string PathToken => Path.Combine(BepInEx.Paths.PluginPath, "SPT-Leaderboard", "secret.token");

        private EncryptionModel()
        {
            try
            {
                CheckIntegrityMod();
                
                if (!File.Exists(PathToken))
                {
                    _token = GenerateToken();
                    WriteTokenToFile(_token);

                    LeaderboardPlugin.logger.LogWarning(
                        $"Generated your secret token, see mod directory. WARNING: DO NOT SHARE IT WITH ANYONE! If you lose it, you will lose access to the Leaderboard until next season!");
                }
                else
                {
                    LeaderboardPlugin.logger.LogWarning(
                        $"Your secret token was initialized by the mod. Remember to never show it to anyone!");
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
        
        public string GetHashMod()
        {
            try
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                byte[] dllBytes = File.ReadAllBytes(assemblyLocation);
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(dllBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (Exception e)
            {
                LeaderboardPlugin.logger.LogError($"[SPT Leaderboard] Error check integrity mod");
                return "ERROR CHECK INTEGRITY";
            }
        }

        private void CheckIntegrityMod()
        {
            if (IsSigned())
            {
                if (IsSignedWithMyKey(Assembly.GetExecutingAssembly()))
                {
                    isEditedDll = false;
                    return;
                }
                isEditedDll = true;
                return;
            }
            isEditedDll = true;
        }
        
        private bool IsAssemblySigned(Assembly assembly)
        {
            byte[] publicKey = assembly.GetName().GetPublicKey();
            return publicKey != null && publicKey.Length > 0;
        }
        
        private bool IsSigned()
        {
            bool isSigned = IsAssemblySigned(Assembly.GetExecutingAssembly());
            LeaderboardPlugin.logger.LogWarning(isSigned ? "Mod is signed" : "Mod not is signed");
            return isSigned;
        }

        private bool IsSignedWithMyKey(Assembly assembly)
        {
            byte[] tokenBytes = assembly.GetName().GetPublicKeyToken();

            if (tokenBytes == null || tokenBytes.Length == 0)
                return false;

            string token = BitConverter.ToString(tokenBytes).Replace("-", "").ToLowerInvariant();
            return token == ExpectedToken;
        }
    }
}