using Newtonsoft.Json;

// DTOs for the CGS HTTP bet API.
// All money values are integer paisa (long). Never use float for money.

public class CGSBetRequest
{
    [JsonProperty("gameKey")]    public string GameKey;
    [JsonProperty("amountPaisa")] public long   AmountPaisa;  // canonical field — integer paisa
    [JsonProperty("betOn")]      public object  BetOn;         // game-specific; null for crash/aviator
    [JsonProperty("tableCode")]  public string  TableCode;     // null for non-room games
}

public class CGSBetResponse
{
    [JsonProperty("data")] public CGSBetData Data;
}

public class CGSBetData
{
    [JsonProperty("txnId")]      public string       TxnId;
    [JsonProperty("gameKey")]    public string       GameKey;
    [JsonProperty("roundRef")]   public string       RoundRef;
    [JsonProperty("betOn")]      public object       BetOn;
    [JsonProperty("amount")]     public long         Amount;
    [JsonProperty("status")]     public string       Status;
    [JsonProperty("balance")]    public CGSBalance   Balance;
}

// Balance snapshot returned by CGS after each bet and cashout — values are integer paisa.
public class CGSBalance
{
    [JsonProperty("wallet")]    public long Wallet;     // deposit wallet paisa
    [JsonProperty("winamount")] public long WinAmount;  // win wallet paisa
    [JsonProperty("bonus")]     public long Bonus;      // bonus wallet paisa
}

public class CGSCashoutRequest
{
    [JsonProperty("gameKey")]   public string GameKey;
    [JsonProperty("tableCode")] public string TableCode;
}

public class CGSCashoutResponse
{
    [JsonProperty("data")] public CGSCashoutData Data;
}

public class CGSCashoutData
{
    [JsonProperty("txnId")]      public string     TxnId;
    [JsonProperty("multiplier")] public float      Multiplier;
    [JsonProperty("win")]        public long        Win;
    [JsonProperty("status")]     public string     Status;
    [JsonProperty("balance")]    public CGSBalance  Balance;
}
