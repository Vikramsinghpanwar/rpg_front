using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Core.API.Endpoints;
using Core.Services;

namespace Core.API
{
    public class ApiClient : MonoBehaviour
    {
        public static ApiClient Instance { get; private set; }

        [SerializeField] string baseUrl;
        [SerializeField] int timeoutSeconds = ApiConfig.TimeoutSeconds;

        readonly Dictionary<string, string> defaultHeaders = new Dictionary<string, string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]

        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[ApiClient]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<ApiClient>();
            if (string.IsNullOrEmpty(Instance.baseUrl))
                Instance.Configure(ApiConfig.BaseUrl);
            Instance.WireRefreshCallback();
        }

        void WireRefreshCallback()
        {
            var tp = TokenProvider.Instance;
            if (tp == null) return;
            // tp.RegisterRefreshCallback(async refreshToken =>
            // {
            //     var body = new { refresh_token = refreshToken };
            //     return await SendRaw<TokenProvider.RefreshResult>(
            //         UnityWebRequest.kHttpVerbPOST, AuthRoutes.Refresh, body, null, sendAuth: false);
            // });
        }

        public void Configure(string url)
        {
            if (!string.IsNullOrEmpty(url)) baseUrl = url.TrimEnd('/');
        }

        public void SetHeader(string key, string value) => defaultHeaders[key] = value;
        public void RemoveHeader(string key) => defaultHeaders.Remove(key);
        public void SetAuthToken(string token)
        {
            if (string.IsNullOrEmpty(token)) defaultHeaders.Remove(ApiHeaders.Authorization);
            else defaultHeaders[ApiHeaders.Authorization] = $"Bearer {token}";
        }

        public Task<T> Get<T>(string path) => Send<T>(UnityWebRequest.kHttpVerbGET, path, null, null);
        public Task<T> Get<T>(string path, IDictionary<string, string> headers) => Send<T>(UnityWebRequest.kHttpVerbGET, path, null, headers);
        public Task<T> GetAsync<T>(string path) => Get<T>(path);

        public Task<T> Post<T>(string path) => Send<T>(UnityWebRequest.kHttpVerbPOST, path, null, null);
        public Task<T> Post<T>(string path, object body) => Send<T>(UnityWebRequest.kHttpVerbPOST, path, body, null);
        public Task<T> Post<T>(string path, object body, IDictionary<string, string> headers) => Send<T>(UnityWebRequest.kHttpVerbPOST, path, body, headers);
        public Task<TResp> Post<TResp, TReq>(string path, TReq body) => Send<TResp>(UnityWebRequest.kHttpVerbPOST, path, body, null);
        public Task<T> PostAsync<T>(string path, object body = null) => Post<T>(path, body);

        public Task<T> Put<T>(string path, object body, IDictionary<string, string> headers = null) => Send<T>(UnityWebRequest.kHttpVerbPUT, path, body, headers);
        public Task<T> Delete<T>(string path, IDictionary<string, string> headers = null) => Send<T>(UnityWebRequest.kHttpVerbDELETE, path, null, headers);

        async Task<T> Send<T>(string method, string path, object body, IDictionary<string, string> perRequestHeaders)
        {
            try
            {
                return await SendRaw<T>(method, path, body, perRequestHeaders, sendAuth: true);
            }
            catch (ApiException ex) when (ex.StatusCode == 401 && !path.EndsWith(AuthRoutes.Refresh, StringComparison.Ordinal))
            {
                var tp = TokenProvider.Instance;
                if (tp == null || !tp.HasRefreshToken) throw;
                var refreshed = await tp.TryRefreshAsync();
                if (!refreshed) throw;
                return await SendRaw<T>(method, path, body, perRequestHeaders, sendAuth: true);
            }
        }

        async Task<T> SendRaw<T>(string method, string path, object body, IDictionary<string, string> perRequestHeaders, bool sendAuth)
        {
            var url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : baseUrl + path;

            using (var req = new UnityWebRequest(url, method))
            {
                req.timeout = timeoutSeconds;
                req.downloadHandler = new DownloadHandlerBuffer();

                if (body != null)
                {
                    var json = JsonConvert.SerializeObject(body);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    req.uploadHandler = new UploadHandlerRaw(bytes) { contentType = ApiHeaders.ApplicationJson };
                    req.SetRequestHeader(ApiHeaders.ContentType, ApiHeaders.ApplicationJson);
                }

                req.SetRequestHeader(ApiHeaders.Accept, ApiHeaders.ApplicationJson);

                // Auth precedence: live TokenProvider > SetAuthToken'd default > nothing.
                // Stale defaults must never override a freshly refreshed token.
                string authHeader = null;
                if (sendAuth)
                {
                    var liveToken = TokenProvider.Instance?.AccessToken;
                    if (!string.IsNullOrEmpty(liveToken))
                        authHeader = $"Bearer {liveToken}";
                    else if (defaultHeaders.TryGetValue(ApiHeaders.Authorization, out var fallback))
                        authHeader = fallback;
                }
                if (authHeader != null) req.SetRequestHeader(ApiHeaders.Authorization, authHeader);

                foreach (var kv in defaultHeaders)
                {
                    if (kv.Key == ApiHeaders.Authorization) continue;
                    req.SetRequestHeader(kv.Key, kv.Value);
                }

                if (perRequestHeaders != null)
                {
                    foreach (var kv in perRequestHeaders)
                        req.SetRequestHeader(kv.Key, kv.Value);
                }

                Debug.Log($"[ApiClient] Sending request: {method} {path}");
                // Debug.Log($"[ApiClient] Headers: {string.Join(", ", req.GetRequestHeaders())}");
                Debug.Log($"[ApiClient] Body: {(body != null ? JsonConvert.SerializeObject(body) : "null")}");
                await AwaitWebRequest(req);

                var status = (int)req.responseCode;
                var raw = req.downloadHandler != null ? req.downloadHandler.text : null;
                Debug.Log($"[ApiClient] Received response: {status} {raw}");
                if (req.result == UnityWebRequest.Result.ConnectionError ||
                    req.result == UnityWebRequest.Result.DataProcessingError)
                {
                    throw new ApiException(req.error ?? "Network error", 0, null, raw);
                }

                if (status == 429)
                {
                    var retryAfter = ParseRetryAfter(req.GetResponseHeader("Retry-After"));
                    throw new RateLimitedException(retryAfter, raw);
                }

                if (status >= 200 && status < 300)
                {
                    if (string.IsNullOrEmpty(raw) || typeof(T) == typeof(object)) return default;
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(raw);
                    }
                    catch (Exception ex)
                    {
                        throw new ApiException($"Failed to parse response: {ex.Message}", status, null, raw);
                    }
                }

                var (code, message) = ParseError(raw);
                throw new ApiException(message ?? $"HTTP {status}", status, code, raw);
            }
        }

        static Task AwaitWebRequest(UnityWebRequest req)
        {
            var tcs = new TaskCompletionSource<bool>();
            var op = req.SendWebRequest();
            op.completed += _ => tcs.TrySetResult(true);
            return tcs.Task;
        }

        static int ParseRetryAfter(string header)
        {
            if (string.IsNullOrEmpty(header)) return 60;
            if (int.TryParse(header, out var seconds)) return Math.Max(1, seconds);
            if (DateTimeOffset.TryParse(header, out var when))
            {
                var delta = (int)(when - DateTimeOffset.UtcNow).TotalSeconds;
                return Math.Max(1, delta);
            }
            return 60;
        }

        static (string code, string message) ParseError(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return (null, null);
            if (raw.TrimStart().StartsWith("{"))
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<ErrorEnvelope>(raw);
                    return (obj?.code, obj?.message ?? obj?.error);
                }
                catch
                {
                    return (null, raw);
                }
            }
            return (null, raw);
        }

        class ErrorEnvelope
        {
            public string error;
            public string code;
            public string message;
        }
    }

    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public string RawBody { get; }

        public ApiException(string message, int statusCode, string errorCode, string rawBody) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            RawBody = rawBody;
        }
    }

    public class RateLimitedException : ApiException
    {
        public int RetryAfterSeconds { get; }

        public RateLimitedException(int retryAfterSeconds, string rawBody)
            : base($"Rate limited; retry after {retryAfterSeconds}s", 429, "rate_limited", rawBody)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }
}
