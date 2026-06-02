namespace Core.Config
{
    // Single source of truth for backend base URL. ApiClient picks this up automatically at
    // startup (RuntimeInitializeOnLoadMethod). Editor / DEVELOPMENT_BUILD / production are
    // separated so you don't have to ifdef in feature code.
    public static class ServerConfig
    {
        public static string BaseUrl
        {
            get
            {
#if UNITY_EDITOR
                return EditorBaseUrl;
#elif DEVELOPMENT_BUILD
                return DevBaseUrl;
#else
                return ProdBaseUrl;
#endif
            }
        }

        // Tweak these once. Features should never hard-code URLs.
        public const string EditorBaseUrl = "http://localhost:8000";
        public const string DevBaseUrl    = "https://dev-api.example.com";
        public const string ProdBaseUrl   = "https://api.example.com";
        public const string SocketUrl   = "https://api.example.com";
        public const string GatewayUrl   = "https://api.example.com";
        public const string Downloadable_Assets_Url   = "https://api.example.com";

        // WebSocket URL for the realtime gateway. Same env-split pattern.
        public static string RealtimeUrl
        {
            get
            {
#if UNITY_EDITOR
                return "ws://localhost:8001/ws";
#elif DEVELOPMENT_BUILD
                return "wss://dev-realtime.example.com/ws";
#else
                return "wss://realtime.example.com/ws";
#endif
            }
        }
    }
}
