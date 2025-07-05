using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SPTLeaderboard.Models
{
    public class NetworkApiRequestModel : MonoBehaviour
    {
        private string _url;
        private string _jsonBody;

        public Action<string, long> OnSuccess;
        public Action<string, long> OnFail;
        
        private bool _isComplete = false;

        /// <summary>
        /// Factory create request
        /// </summary>
        public static NetworkApiRequestModel Create(string url)
        {
            var obj = new GameObject("[SPTLeaderboard] NetworkRequest");
            DontDestroyOnLoad(obj);
            var request = obj.AddComponent<NetworkApiRequestModel>();
            request._url = url;
            return request;
        }

        /// <summary>
        /// Set body data request
        /// </summary>
        public NetworkApiRequestModel SetData(string jsonBody)
        {
            _jsonBody = jsonBody;
            return this;
        }

        /// <summary>
        /// Starting request start
        /// </summary>
        public void Start()
        {
            StartCoroutine(RunBaseRequest());
        }

        private IEnumerator RunBaseRequest()
        {
            if (_isComplete)
            {
                yield break;
            }
            _isComplete = true;
            
            using var request = new UnityWebRequest(_url, UnityWebRequest.kHttpVerbPOST);
            var bodyRaw = Encoding.UTF8.GetBytes(_jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-SPT-Mod", "SPTLeaderboard");

            var reqId = Guid.NewGuid().ToString();
            LeaderboardPlugin.logger.LogWarning($"[SPT Leaderboard] Request ID = {reqId}");
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                OnSuccess?.Invoke(request.downloadHandler.text, request.responseCode);
            }
            else
            {
                LeaderboardPlugin.logger.LogWarning($"[SPT Leaderboard] OnFail responce {request.downloadHandler.text}");
                OnFail?.Invoke(request.error, request.responseCode);
            }
            
            Destroy(gameObject);
        }
        
    }
}