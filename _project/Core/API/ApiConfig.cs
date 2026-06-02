using Core.Config;

namespace Core.API
{
    // Single source for runtime API settings. Wraps ServerConfig (env-split base URL) and
    // adds the request-level knobs the ApiClient cares about. Features should read from
    // here rather than from ServerConfig directly.
    public static class ApiConfig
    {
        public static string BaseUrl => ServerConfig.BaseUrl;
        public static string RealtimeUrl => ServerConfig.RealtimeUrl;

        // Default per-request timeout (seconds). ApiClient picks this up at bootstrap.
        public const int TimeoutSeconds = 30;

        // Versioned route prefix used by everything except the unversioned recharge gateways
        // / webhook surface. Defined here so a future bump to v2 is a one-line change.
        public const string ApiVersion = "v1";

        // Reserved for future use by ApiClient (background retry policy). Today the client
        // only retries 401s after a token refresh — see ApiClient.Send.
        public const int RetryCount = 0;
    }
}
