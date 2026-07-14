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
        public const string EditorBaseUrl = "https://api.thecrownempire.live";
        public const string DevBaseUrl = "https://api.thecrownempire.live";
        public const string ProdBaseUrl = "https://api.thecrownempire.live";
        public const string SocketUrl = "https://cgs.thecrownempire.live";
        public const string GatewayUrl = "https://api.example.com";
        public const string Downloadable_Assets_Url = "https://cre-media-upload.s3.ap-south-1.amazonaws.com/";//"https://api.example.com";

        public const string LandingPageUrl = "https://theblackpearl.online";


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
