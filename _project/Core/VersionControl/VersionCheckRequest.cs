using System;
using Newtonsoft.Json;

namespace Core.VersionControl
{
    [Serializable]
    public class VersionCheckRequest
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("currentVersion")]
        public string CurrentVersion { get; set; }

        [JsonProperty("buildNumber")]
        public string BuildNumber { get; set; }

        [JsonProperty("environment")]
        public string Environment { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("appFlavor", NullValueHandling = NullValueHandling.Ignore)]
        public string AppFlavor { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
