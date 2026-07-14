using System;
using Newtonsoft.Json;

namespace Core.Models
{
    [Serializable]
    public class UpdateUserRequest
    {
        [JsonProperty("username")] public string Username;
        [JsonProperty("avatar")] public string Avatar;
    }
}
