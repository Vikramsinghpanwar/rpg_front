using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    
    public Text[] diceButtonTexts;
    public Text[] multiplierTexts;
    //public CountdownTimer countdownTimer;
    public Text resultText;
    public Text winLoseText;
    public Text walletText;
    public Animation diceAnime;
    public GameObject Dice1;
    public GameObject Result;
    public List<Player> players;
    private int[] diceResults = new int[6];
    private bool hasDiceAnimationPlayed;
    public Button[] bettingAmountButtons;
    public Button[] diceNumberButtons;
    private int selectedBetAmount;
    private int selectedDiceNumber;
    public Player humanPlayer;
    private Dictionary<int, int> playerBets = new Dictionary<int, int>();
    private Button selectedBettingAmountButton;
    private Burst brustRef;
    private Dice dice;
    private FrameByFrameAnimation Frame;

    void Start()
    {
        Frame = FindObjectOfType<FrameByFrameAnimation>();
        dice = FindObjectOfType<Dice>(); 
        dice.StartGame();
        brustRef = FindObjectOfType<Burst>();
        //countdownTimer.OnTimerEnded += EndBettingPhase;
        Result.SetActive(false);
        Dice1.SetActive(false);

        players = new List<Player>()
        {
            new Player("Ram", 10000),
            new Player("Aman", 10000),
            new Player("Jaya", 10000),
            new Player("Shubham", 10000),
            new Player("Aryan", 10000),
            new Player("Gunjan", 10000)
        };

        humanPlayer = new Player("Human", 10000);

        foreach (Button button in bettingAmountButtons)
        {
            button.onClick.AddListener(() => OnBetAmountButtonClicked(button));
        }

        foreach (Button button in diceNumberButtons)
        {
            button.interactable = false;
            button.onClick.AddListener(() => OnDiceNumberButtonClicked(button));
        }

        //StartCoroutine(GameLoop());
        UpdateWalletUI();
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            StartBettingPhase();

            //yield return new WaitUntil(() => countdownTimer == null);

            yield return new WaitForSeconds(1);
            Dice1.SetActive(true);
            dice.MovePanel();
            Frame.RollDice();
            yield return new WaitForSeconds(5);

            RollDice();

            yield return new WaitForSeconds(5);

            RestartGame();
        }
    }

    void StartBettingPhase()
    {
        //StartCoroutine(brustRef.AnimStart());
        //countdownTimer.ResetTimer(15f);
        hasDiceAnimationPlayed = false;

        foreach (Player player in players)
        {
            int betAmount = RandomBetAmount();
            int betNumber = Random.Range(1, 7);
            player.PlaceBet(betAmount, betNumber);
        }

        UpdateBettingAmountButtonInteractability();
    }

    void EndBettingPhase()
    {
        if (playerBets.Count == 0)
        {
            winLoseText.text = "No Bet Placed";
        }
        else
        {
            PlayDiceAnimation();
        }

        //countdownTimer = null;
        brustRef.StopAnim();
    }

    void PlayDiceAnimation()
    {
        if (!hasDiceAnimationPlayed && diceAnime != null)
        {
            diceAnime.Play();
            hasDiceAnimationPlayed = true;
        }
    }

    void RollDice()
    {
        for (int i = 0; i < 6; i++)
        {
            diceResults[i] = Random.Range(1, 7);
        }

        resultText.text = "Dice Rolled: " + string.Join(", ", diceResults);
        ShowResults();
    }

    void ShowResults()
    {
        int totalWinnings = 0;
        bool anyWin = false;

        winLoseText.text = "";
        foreach (Text multiplierText in multiplierTexts)
        {
            multiplierText.text = "";
        }

        foreach (var bet in playerBets)
        {
            int betNumber = bet.Key;
            int betAmount = bet.Value;
            int hitCount = 0;

            foreach (int diceResult in diceResults)
            {
                if (diceResult == betNumber)
                {
                    hitCount++;
                }
            }

            if (hitCount > 1)
            {
                anyWin = true;
                int multiplier = GetMultiplier(hitCount);
                int winAmount = betAmount * multiplier;
                totalWinnings += winAmount;

                winLoseText.text += $"Bet on {betNumber}: You Win {multiplier}x!\n";
                multiplierTexts[betNumber - 1].text = $"{multiplier}x";
            }
            else
            {
                winLoseText.text += $"Bet on {betNumber}: You Lose.\n";
            }
        }

        humanPlayer.balance += totalWinnings;

        if (!anyWin)
        {
            winLoseText.text = "No Winning Bets!";
        }

        UpdateWalletUI();
    }

    int GetMultiplier(int hitCount)
    {
        switch (hitCount)
        {
            case 2: return 3;
            case 3: return 5;
            case 4: return 10;
            case 5: return 20;
            case 6: return 100;
            default: return 1;
        }
    }

    void OnBetAmountButtonClicked(Button button)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            int betAmount = int.Parse(buttonText.text);

            if (humanPlayer.balance >= betAmount)
            {
                selectedBetAmount = betAmount;
                UpdateBettingButtonVisuals(button);

                foreach (Button diceButton in diceNumberButtons)
                {
                    diceButton.interactable = true;
                }
            }
            else
            {
                resultText.text = "Insufficient funds for the bet.";
            }
        }
    }

    void UpdateBettingButtonVisuals(Button newSelectedButton)
    {
        if (selectedBettingAmountButton != null)
        {
            selectedBettingAmountButton.GetComponent<Image>().color = Color.white;
        }

        selectedBettingAmountButton = newSelectedButton;
        selectedBettingAmountButton.GetComponent<Image>().color = Color.yellow;
    }

    void OnDiceNumberButtonClicked(Button button)
    {
        int diceNumber = int.Parse(button.GetComponentInChildren<Text>().text);

        if (humanPlayer.balance >= selectedBetAmount)
        {
            humanPlayer.balance -= selectedBetAmount;

            if (playerBets.ContainsKey(diceNumber))
            {
                playerBets[diceNumber] += selectedBetAmount;
            }
            else
            {
                playerBets[diceNumber] = selectedBetAmount;
            }

            UpdateWalletUI();

            Text diceText = diceButtonTexts[diceNumber - 1];
            if (diceText != null)
            {
                diceText.text = $"0 / {playerBets[diceNumber]}";
            }
        }
        else
        {
            resultText.text = "Insufficient funds for the bet.";
        }
    }

    int RandomBetAmount()
    {
        int[] possibleBets = { 10, 50, 100, 500, 1000, 5000 };
        return possibleBets[Random.Range(0, possibleBets.Length)];
    }

    void RestartGame()
    {
        resultText.text = "";
        winLoseText.text = "";
        Result.SetActive(false);
        selectedDiceNumber = 0;
        playerBets.Clear();
        Dice1.SetActive(false);
        brustRef.MoveAllcoinsBack();

        foreach (Text diceText in diceButtonTexts)
        {
            diceText.text = "0 / 0";
        }

        foreach (Text multiplierText in multiplierTexts)
        {
            multiplierText.text = "";
        }

        //countdownTimer = FindObjectOfType<CountdownTimer>();
        //countdownTimer.ResetTimer(15f);

        if (selectedBettingAmountButton != null)
        {
            selectedBettingAmountButton.GetComponent<Image>().color = Color.yellow;
        }

        UpdateWalletUI();
    }

    void UpdateBettingAmountButtonInteractability()
    {
        foreach (Button button in bettingAmountButtons)
        {
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                int betAmount = int.Parse(buttonText.text);
                button.interactable = humanPlayer.balance >= betAmount;
            }
        }
    }

    void UpdateWalletUI()
    {
        walletText.text = $"{humanPlayer.balance}";
        UpdateBettingAmountButtonInteractability();
    }
}
