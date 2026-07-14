using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Core.VersionControl
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VersionStatus
    {
        [EnumMember(Value = "UP_TO_DATE")]
        UpToDate,
        [EnumMember(Value = "SOFT_UPDATE")]
        SoftUpdate,
        [EnumMember(Value = "FORCE_UPDATE")]
        ForceUpdate,
        [EnumMember(Value = "UNSUPPORTED")]
        Unsupported,
        [EnumMember(Value = "MAINTENANCE")]
        Maintenance
    }
}
