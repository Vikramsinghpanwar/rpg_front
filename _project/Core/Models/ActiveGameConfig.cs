using System.Collections.Generic;

namespace Core.Models
{
    // [Serializable]
    public class ActiveGameConfig
    {
        public string gameCode;
        public string displayName;
        public bool enabled;
        public bool isNew;
        public bool isTrending;
        public bool isPopular;
        public int sortOrder;
        public string iconUrl;
        public string bannerUrl;
        public bool maintenanceMode;
        public string maintenanceMessage;
        public GameMetadata metadata;
    }

    // [Serializable]
    public class GameMetadata
    {
        public long stakeMax;
        public long stakeMin;
        public string abVariant;
        public List<string> allowedRegions;
        public List<string> blockedRegions;
        public bool tournamentEnabled;
        public bool privateTablesEnabled;
    }

    // [Serializable]
    public class GamesConfigResponse
    {
        public bool success;
        public int statusCode;
        public string timestamp;
        public string path;
        public List<ActiveGameConfig> data;
    }
}
