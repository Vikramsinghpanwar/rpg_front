public class Player
{
    public string playerName;  // Name of the player
    public int balance;        // Player's balance
    public int betAmount;      // Amount player is betting
    public int betNumber;      // The number the player is betting on

    // Constructor for Player class
    public Player(string name, int initialBalance)
    {
        playerName = name;
        balance = initialBalance;
    }

    // Method to place a bet
    public void PlaceBet(int amount, int number)
    {
        betAmount = amount;
        betNumber = number;
        balance -= amount;  // Deduct the bet amount from the balance
    }

    // Method to receive winnings
    public void ReceiveWinnings(int winnings)
    {
        balance += winnings;
    }
}
