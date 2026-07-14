using System;
using Newtonsoft.Json;

namespace Core.VersionControl
{
    [Serializable]
    public class VersionCheckResponse
    {
        [JsonProperty("status")]
        public VersionStatus? FlatStatus { get; set; }

        [JsonProperty("latestVersion")]
        public string FlatLatestVersion { get; set; }

        [JsonProperty("updateUrl")]
        public string FlatUpdateUrl { get; set; }

        [JsonProperty("releaseNotes")]
        public string FlatReleaseNotes { get; set; }

        [JsonProperty("maintenanceMessage")]
        public string FlatMaintenanceMessage { get; set; }

        [JsonProperty("maintenanceUntil")]
        public string FlatMaintenanceUntil { get; set; }

        [JsonProperty("canContinue")]
        public bool? FlatCanContinue { get; set; }

        [JsonProperty("gate")]
        public VersionGate Gate { get; set; }

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                if (Gate != null && !string.IsNullOrEmpty(Gate.Action))
                {
                    return Gate.Action.ToUpperInvariant() switch
                    {
                        "ALLOW" => true,
                        "SOFT_UPDATE" => true,
                        "FORCE_UPDATE" => true,
                        "UNSUPPORTED" => true,
                        "MAINTENANCE" => true,
                        _ => false
                    };
                }

                return FlatStatus.HasValue;
            }
        }

        [JsonIgnore]
        public VersionStatus Status
        {
            get
            {
                if (Gate != null && !string.IsNullOrEmpty(Gate.Action))
                {
                    return Gate.Action.ToUpperInvariant() switch
                    {
                        "ALLOW" => VersionStatus.UpToDate,
                        "SOFT_UPDATE" => VersionStatus.SoftUpdate,
                        "FORCE_UPDATE" => VersionStatus.ForceUpdate,
                        "UNSUPPORTED" => VersionStatus.Unsupported,
                        "MAINTENANCE" => VersionStatus.Maintenance,
                        _ => VersionStatus.UpToDate
                    };
                }

                if (FlatStatus.HasValue)
                    return FlatStatus.Value;

                return VersionStatus.UpToDate;
            }
        }

        [JsonIgnore]
        public string LatestVersion
        {
            get
            {
                if (Gate != null && !string.IsNullOrEmpty(Gate.LatestVersion))
                    return Gate.LatestVersion;
                return FlatLatestVersion;
            }
        }

        [JsonIgnore]
        public string UpdateUrl
        {
            get
            {
                if (Gate != null && !string.IsNullOrEmpty(Gate.UpdateUrl))
                    return Gate.UpdateUrl;
                return FlatUpdateUrl;
            }
        }

        [JsonIgnore]
        public string ReleaseNotes
        {
            get
            {
                if (Gate != null && !string.IsNullOrEmpty(Gate.ReleaseNotes))
                    return Gate.ReleaseNotes;
                return FlatReleaseNotes;
            }
        }

        [JsonIgnore]
        public string MaintenanceMessage
        {
            get
            {
                if (Gate != null && !string.IsNullOrEmpty(Gate.MaintenanceMessage))
                    return Gate.MaintenanceMessage;
                return FlatMaintenanceMessage;
            }
        }

        [JsonIgnore]
        public string MaintenanceUntil
        {
            get
            {
                if (Gate != null && !string.IsNullOrEmpty(Gate.MaintenanceUntil))
                    return Gate.MaintenanceUntil;
                return FlatMaintenanceUntil;
            }
        }

        [JsonIgnore]
        public bool CanContinue
        {
            get
            {
                if (Gate != null)
                    return Gate.CanContinue;
                if (FlatCanContinue.HasValue)
                    return FlatCanContinue.Value;
                return true;
            }
        }
    }

    [Serializable]
    public class VersionGate
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("currentVersion")]
        public string CurrentVersion { get; set; }

        [JsonProperty("minVersion")]
        public string MinVersion { get; set; }

        [JsonProperty("latestVersion")]
        public string LatestVersion { get; set; }

        [JsonProperty("updateUrl")]
        public string UpdateUrl { get; set; }

        [JsonProperty("releaseNotes")]
        public string ReleaseNotes { get; set; }

        [JsonProperty("maintenanceMessage")]
        public string MaintenanceMessage { get; set; }

        [JsonProperty("maintenanceUntil")]
        public string MaintenanceUntil { get; set; }

        [JsonProperty("canContinue")]
        public bool CanContinue { get; set; }
    }
}
