using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SPTLeaderboard.Models
{
    /// <summary>
    /// Handles HTTP network requests (GET/POST) with optional JSON payload, retries on timeout,
    /// and callbacks for success or failure.
    /// </summary>
    public class NetworkApiRequestModel : MonoBehaviour
    {
        private string _url;
        private string _jsonBody;
        private string _httpMethod = UnityWebRequest.kHttpVerbPOST;

        public Action<string, long> OnSuccess;
        public Action<string, long> OnFail;
        
        private bool _isComplete;
        
        private int _retryCount = 0;
        private int _maxRetries = 2;
        
        /// <summary>
        /// Sets the maximum number of retries when a request times out.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        public void SetMaxRetries(int maxRetries)
        {
            _maxRetries = maxRetries;
        }

        /// <summary>
        /// Factory method to create a POST request instance.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A new <see cref="NetworkApiRequestModel"/> configured for POST.</returns>
        public static NetworkApiRequestModel Create(string url)
        {
#if DEBUG || BETA
            LeaderboardPlugin.logger.LogWarning($"[POST] Request Url -> '{url}'");
#endif
            var obj = new GameObject("[SPTLeaderboard] NetworkRequest");
            DontDestroyOnLoad(obj);
            var request = obj.AddComponent<NetworkApiRequestModel>();
            request._url = url;
            request._httpMethod = UnityWebRequest.kHttpVerbPOST;
            return request;
        }

        /// <summary>
        /// Factory method to create a GET request instance.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A new <see cref="NetworkApiRequestModel"/> configured for GET.</returns>
        public static NetworkApiRequestModel CreateGet(string url)
        {
#if DEBUG || BETA
            LeaderboardPlugin.logger.LogWarning($"[GET] Request Url -> '{url}'");
#endif
            var obj = new GameObject("[SPTLeaderboard] NetworkRequest");
            DontDestroyOnLoad(obj);
            var request = obj.AddComponent<NetworkApiRequestModel>();
            request._url = url;
            request._httpMethod = UnityWebRequest.kHttpVerbGET;
            return request;
        }

        /// <summary>
        /// Sets the JSON payload for a POST request.
        /// </summary>
        /// <param name="jsonBody">The JSON string to send in the request body.</param>
        public void SetData(string jsonBody)
        {
            _jsonBody = jsonBody;
        }

        /// <summary>
        /// Starts sending the request. Handles retries on timeout automatically.
        /// </summary>
        public void Send()
        {
            StartCoroutine(RunBaseRequest());
        }

        /// <summary>
        /// Internal coroutine that executes the HTTP request and handles success, failure, and retries.
        /// </summary>
        /// <remarks>
        /// - Calls <see cref="OnSuccess"/> if the request succeeds.  
        /// - Calls <see cref="OnFail"/> if the request fails or exceeds retry attempts.  
        /// - Automatically destroys the GameObject after completion.
        /// </remarks>
        private IEnumerator RunBaseRequest()
        {
            if (_httpMethod == UnityWebRequest.kHttpVerbPOST && string.IsNullOrEmpty(_jsonBody))
            {
                LeaderboardPlugin.logger.LogWarning("Data is null or empty, skipping POST request");
                yield break;
            }
            
            if (_isComplete)
            {
                yield break;
            }
            
            _isComplete = true;
            
            UnityWebRequest request;

            if (_httpMethod == UnityWebRequest.kHttpVerbPOST)
            {
                request = new UnityWebRequest(_url, UnityWebRequest.kHttpVerbPOST);
                var bodyRaw = Encoding.UTF8.GetBytes(_jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
            }
            else // GET
            {
                request = UnityWebRequest.Get(_url);
            }

            request.downloadHandler ??= new DownloadHandlerBuffer();
            request.SetRequestHeader("X-SPT-Mod", "SPTLeaderboard");
            
#if DEBUG || BETA
            var reqId = Guid.NewGuid().ToString();
            LeaderboardPlugin.logger.LogWarning($"Request ID = {reqId}");
#endif

            request.timeout = SettingsModel.Instance.ConnectionTimeout.Value;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                OnSuccess?.Invoke(request.downloadHandler.text, request.responseCode);
                Destroy(gameObject);
            }
            else
            {
                bool isTimeout = request.error != null && request.error.ToLower().Contains("timeout");
                
                if (isTimeout && _retryCount < _maxRetries)
                {
                    _retryCount++;
                    LeaderboardPlugin.logger.LogWarning($"Timeout, retrying {_retryCount}/{_maxRetries}...");
                    _isComplete = false;
                    yield return new WaitForSeconds(0.5f);
                    StartCoroutine(RunBaseRequest());
                }
                else
                {
                    if (_retryCount >= _maxRetries && isTimeout)
                    {
                        LeaderboardPlugin.logger.LogWarning("After five tries, nothing came out");
                    }
#if DEBUG || BETA           
                    LeaderboardPlugin.logger.LogWarning($"OnFail response {request.downloadHandler.text}");
#endif
                    OnFail?.Invoke(request.error, request.responseCode);
                    Destroy(gameObject);
                }
            }
        }
    }
}