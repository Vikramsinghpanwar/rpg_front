using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Features.GameHistory.Models
{
    [Serializable]
    public class GameHistoryResponse
    {
        [JsonProperty("items")] public List<GameHistoryItem> items;
        [JsonProperty("total")] public int total;
        [JsonProperty("limit")] public int limit;
        [JsonProperty("offset")] public int offset;
    }

    [Serializable]
    public class GameHistoryItem
    {
        [JsonProperty("id")] public string id;
        [JsonProperty("game_type")] public string game_type;
        [JsonProperty("game_name")] public string game_name;
        [JsonProperty("room_id")] public string room_id;
        [JsonProperty("round_id")] public string round_id;
        [JsonProperty("debit_amount")] public long debit_amount;
        [JsonProperty("credit_amount")] public long credit_amount;
        [JsonProperty("net_result")] public long net_result;
        [JsonProperty("played_at")] public string played_at;
    }
}
