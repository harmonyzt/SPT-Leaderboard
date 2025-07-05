using System.Collections;
using System.Text;
using UnityEngine.Networking;

namespace SPTLeaderboard.Models
{
    public class NetworkApiRequestModel
    {
        public static NetworkApiRequestModel Instance { get; private set; }
        
        // public string url = "https://visuals.nullcore.net/SPT/testEnv/api/v1/main.php";
        public string url = "https://visuals.nullcore.net/SPT/testEnv/api/heartbeat/v1.php";
        public string body = """{"type":"online","timestamp":1751658790369,"ver":"2.6.0","sessionId":"6862c9040004a645b8febe48"}""";
    
        public IEnumerator SendPostRequest()
        {
            string url = "https://visuals.nullcore.net/SPT/testEnv/api/heartbeat/v1.php";

            // Твой JSON тело
            string jsonBody = "{\"type\":\"online\",\"timestamp\":1751658790369,\"ver\":\"2.6.0\",\"sessionId\":\"6862c9040004a645b8febe48\"}";
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-SPT-Mod", "SPTLeaderboard");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    LeaderboardPlugin.ManualLogger.LogWarning("Response: " + request.downloadHandler.text);
                    LeaderboardPlugin.ManualLogger.LogWarning("Response Code: " + request.responseCode);
                }
                else
                {
                    LeaderboardPlugin.ManualLogger.LogWarning("Error: " + request.error + ", Response Code: " + request.responseCode);
                }
            }
        }
        
        public static NetworkApiRequestModel Create()
        {
            if (Instance != null)
            {
                return Instance;
            }
            return Instance = new NetworkApiRequestModel();
        }
    }
}