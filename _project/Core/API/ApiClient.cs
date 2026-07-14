using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Core.API.Endpoints;
using Core.Services;
using System.Linq;

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

        void Awake()
        {
            InitializeMetadataHeaders();
        }

        void InitializeMetadataHeaders()
        {
            // Custom Testing
            defaultHeaders["X-Platform"] = "ANDROID";
            defaultHeaders["X-App-Version"] = "1.0.0";
            // defaultHeaders["X-Build-Number"] = "1";
            // defaultHeaders["X-Environment"] = "PRODUCTION";
            defaultHeaders["X-Region"] = "IN";
            defaultHeaders["X-Locale"] = "en-US";
            defaultHeaders["X-Device-Type"] = "mobile";

            // Production
            // defaultHeaders["X-Platform"] = GetPlatform();
            // defaultHeaders["X-App-Version"] = Application.version;
            // defaultHeaders["X-Build-Number"] = GetBuildNumber();
            // defaultHeaders["X-Environment"] = GetEnvironment();
            // defaultHeaders["X-Region"] = GetRegion();
            // defaultHeaders["X-Locale"] = GetLocale();
            // defaultHeaders["X-Device-Type"] = GetDeviceType();
        }

        string GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android: return "android";
                case RuntimePlatform.IPhonePlayer: return "ios";
                case RuntimePlatform.WebGLPlayer: return "web";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor: return "windows";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor: return "macos";
                default: return Application.platform.ToString().ToLowerInvariant();
            }
        }

        string GetBuildNumber()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (var packageManager = context.Call<AndroidJavaObject>("getPackageManager"))
                using (var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", context.Call<string>("getPackageName"), 0))
                {
                    return packageInfo.Get<int>("versionCode").ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ApiClient] Failed to retrieve Android versionCode: {ex.Message}");
            }
#endif
            return "0";
        }

        string GetEnvironment()
        {
#if UNITY_EDITOR
            return "INTERNAL";
#elif DEVELOPMENT_BUILD
            return "INTERNAL";
#else
            return "PRODUCTION";
#endif
        }

        string GetRegion()
        {
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                if (culture != null && !string.IsNullOrEmpty(culture.Name))
                {
                    var parts = culture.Name.Split('-');
                    if (parts.Length > 1)
                    {
                        return parts[1].ToUpperInvariant();
                    }
                }
            }
            catch { }
            return "IN";
        }

        string GetLocale()
        {
            try
            {
                var culture = System.Globalization.CultureInfo.CurrentCulture;
                if (culture != null && !string.IsNullOrEmpty(culture.Name))
                {
                    return culture.Name;
                }
            }
            catch { }
            return "en-US";
        }

        string GetDeviceType()
        {
            switch (SystemInfo.deviceType)
            {
                case DeviceType.Handheld: return "handheld";
                case DeviceType.Console: return "console";
                case DeviceType.Desktop: return "desktop";
                default: return "unknown";
            }
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
        public Task<T> Patch<T>(string path, object body, IDictionary<string, string> headers = null) => Send<T>("PATCH", path, body, headers);
        public Task<T> Delete<T>(string path, IDictionary<string, string> headers = null) => Send<T>(UnityWebRequest.kHttpVerbDELETE, path, null, headers);

        public Task<T> PostMultipart<T>(string path, string formFieldName, byte[] fileBytes, string fileName, string mimeType, IDictionary<string, string> extraFormFields, IDictionary<string, string> extraHeaders)
        {
            return SendMultipartRaw<T>(UnityWebRequest.kHttpVerbPOST, path, formFieldName, fileBytes, fileName, mimeType, extraFormFields, extraHeaders);
        }

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

        async Task<T> SendMultipartRaw<T>(string method, string path, string formFieldName, byte[] fileBytes, string fileName, string mimeType, IDictionary<string, string> extraFormFields, IDictionary<string, string> perRequestHeaders)
        {
            string url = path.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? path : baseUrl + path;

            var form = new WWWForm();
            if (extraFormFields != null)
            {
                foreach (var kv in extraFormFields)
                {
                    form.AddField(kv.Key, kv.Value);
                }
            }
            form.AddBinaryData(formFieldName, fileBytes, fileName, mimeType);

            using (var req = UnityWebRequest.Post(url, form))
            {
                req.timeout = timeoutSeconds;
                req.downloadHandler = new DownloadHandlerBuffer();

                req.SetRequestHeader(ApiHeaders.Accept, ApiHeaders.ApplicationJson);

                string authHeader = null;
                var liveToken = TokenProvider.Instance?.AccessToken;
                if (!string.IsNullOrEmpty(liveToken))
                    authHeader = $"Bearer {liveToken}";
                else if (defaultHeaders.TryGetValue(ApiHeaders.Authorization, out var fallback))
                    authHeader = fallback;
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

                Debug.Log($"[ApiClient] Sending multipart request: {method} {path}");
                await AwaitWebRequest(req);

                var status = (int)req.responseCode;
                var raw = req.downloadHandler != null ? req.downloadHandler.text : null;
                Debug.Log($"[ApiClient] Received multipart response: {status} {raw}");

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

        static byte[] BuildMultipartBody(string boundary, string formFieldName, byte[] fileBytes, string fileName, string mimeType, System.Collections.Generic.IDictionary<string, string> extraFormFields)
        {
            var header = $"--{boundary}\r\n" +
                         $"Content-Disposition: form-data; name=\"{formFieldName}\"; filename=\"{fileName}\"\r\n" +
                         $"Content-Type: {mimeType}\r\n\r\n";

            var footerBuilder = new System.Text.StringBuilder();
            if (extraFormFields != null)
            {
                foreach (var kv in extraFormFields)
                {
                    footerBuilder.AppendLine();
                    footerBuilder.AppendLine($"--{boundary}");
                    footerBuilder.AppendLine($"Content-Disposition: form-data; name=\"{kv.Key}\"");
                    footerBuilder.AppendLine();
                    footerBuilder.Append(kv.Value);
                }
                footerBuilder.AppendLine();
            }
            footerBuilder.AppendLine($"--{boundary}--");

            var headerBytes = System.Text.Encoding.UTF8.GetBytes(header);
            var footerBytes = System.Text.Encoding.UTF8.GetBytes(footerBuilder.ToString());

            var combined = new byte[headerBytes.Length + fileBytes.Length + footerBytes.Length];
            System.Buffer.BlockCopy(headerBytes, 0, combined, 0, headerBytes.Length);
            System.Buffer.BlockCopy(fileBytes, 0, combined, headerBytes.Length, fileBytes.Length);
            System.Buffer.BlockCopy(footerBytes, 0, combined, headerBytes.Length + fileBytes.Length, footerBytes.Length);

            return combined;
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

                await AwaitWebRequest(req);

                var status = (int)req.responseCode;
                var raw = req.downloadHandler != null ? req.downloadHandler.text : null;

                var allHeaders = new Dictionary<string, string>(defaultHeaders);

                if (perRequestHeaders != null)
                {
                    foreach (var kv in perRequestHeaders)
                        allHeaders[kv.Key] = kv.Value;
                }

                Debug.Log(
                    $"[ApiClient] Sending request: {method} {path}\n" +
                    $"Headers:\n{string.Join("\n", allHeaders.Select(kv => $"{kv.Key}: {kv.Value}"))}"
                );

                Debug.Log($"[ApiClient] Sending request: {method} {path} \n Body: {(body != null ? JsonConvert.SerializeObject(body) : "null")}, \n Headers: {string.Join(", ", perRequestHeaders ?? new Dictionary<string, string>())} \n\n\n Received response: {status} {raw}");
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
