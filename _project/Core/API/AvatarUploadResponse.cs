using Newtonsoft.Json;

namespace Core.API
{
    public class AvatarUploadResponse
    {
        [JsonProperty("success")] public bool Success;
        [JsonProperty("statusCode")] public int StatusCode;
        [JsonProperty("data")] public AvatarUploadData Data;
    }

    public class AvatarUploadData
    {
        [JsonProperty("id")] public string Id;
        [JsonProperty("publicUrl")] public string PublicUrl;
        [JsonProperty("mimeType")] public string MimeType;
        [JsonProperty("sizeBytes")] public long SizeBytes;
        [JsonProperty("entityId")] public string EntityId;
        [JsonProperty("category")] public string Category;
    }
}
