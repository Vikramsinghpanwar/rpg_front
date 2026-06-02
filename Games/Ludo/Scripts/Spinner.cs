using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Lobby.Integration;

public class Spinner : MonoBehaviour
{
    public AudioSource spinAudio;
    public GameObject insufficientFunds;
    public GameObject threeBtnRoda;
    public GameObject popupPanel; // Assign the panel in the Inspector
    private Coroutine closeCoroutine;
    public GameObject SpinBtn;
    public TextMeshProUGUI BtnText;
    public Text AmountText;
    private float WinningAMount;
    public float myAmount;
    public float speed;
    public Transform wheelTransform; // The transform of the spinner wheel
    public float spinDuration = 5f; // Duration of the spin in seconds
    public AnimationCurve spinCurve; // Animation curve for the spin animation
    public Image SPin;
    public Sprite BlueSilver;
    public Sprite RedSilver;
    public Sprite BlueGold;
    public Sprite RedGold;
    public Sprite BlueDiamond;
    public Sprite RedDiamond;
    public Sprite SliverSpin;
    public Sprite GoldSpin;
    public Sprite DiamondSpin;
    public Image BtnSliver;
    public Image BtnGold;
    public Image BtnDimoned;
    private int ResultActive;
    private int BettAmount;
    private int inWheel;
    public TextMeshProUGUI warningTxt;

    private bool isSpinning;

    private void DisableTxt()
    {
        warningTxt.text = "";
    }
    public void TempSpin()
    {
        warningTxt.text = "Insufficient Funds";
        Invoke("DisableTxt", 2f);
    }
    void Start()
    {
        ResultActive = 1;
        BettAmount = 100;
        isSpinning = false;
    }



    private void OnEnable()
    {
        threeBtnRoda.SetActive(false);
        SpinBtn.GetComponent<Button>().interactable = true;
        warningTxt.text = "";

    }

    public void SpinWheel()
    {
        float UserWallet = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
        if (UserWallet >= BettAmount)
        {
            SpinBtn.GetComponent<Button>().interactable = false;
            if (!isSpinning)
            {
                // Betting Bett = FindObjectOfType<Betting>();
                // Bett.BettingAMount(BettAmount);
                threeBtnRoda.SetActive(true);
                StartCoroutine(SpinAnimation());
            }
        }
        else
        {
            insufficientFunds.SetActive(true);
            Debug.Log("Userwallet is not");
        }
    }

    public IEnumerator SpinAnimation()
    {
        Debug.Log("spinning");
        isSpinning = true;
        spinAudio.Play();
        float startAngle = wheelTransform.rotation.eulerAngles.z;
        float endAngle = startAngle + (speed * 360) + Random.Range(1, 360); // Spin one full rotation
        float t = 0f;
        Debug.Log(endAngle);
        while (t < 1f)
        {
            t += Time.deltaTime / spinDuration;
            float angle = Mathf.Lerp(startAngle, endAngle, spinCurve.Evaluate(t));
            wheelTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // Determine the result based on the final angle
        float finalAngle = wheelTransform.rotation.eulerAngles.z;
        int result = Mathf.FloorToInt(finalAngle / 120f); // 120 degrees per section

        // Display the result (you can implement this part)

        isSpinning = false;
        spinAudio.Stop();

        WinnerCalculator(endAngle);
        OnEnable();
    }

    public void WinnerCalculator(float angle)
    {

        while (angle >= 360)
        {
            angle -= 360;
        }
        Debug.Log(angle);
        // angle = (int)angle / 8;
        // if(angle == 13){
        //     angle = 0;
        // }
        Debug.Log(angle);
        if (angle <= 22.5f && angle >= 337.5f)
        {
            WinningAMount = 1;
        }
        else if (angle >= 22.5f && angle <= 67.5f)
        {
            WinningAMount = 58;
        }
        else if (angle >= 67.5f && angle <= 112.5f)
        {
            WinningAMount = 1;
        }
        else if (angle >= 112.5f && angle <= 157.5f)
        {
            WinningAMount = 88;
        }
        else if (angle >= 157.5f && angle <= 202.5f)
        {
            WinningAMount = 5;
        }
        else if (angle >= 202.5f && angle <= 247.5f)
        {
            WinningAMount = 888;
        }
        else if (angle >= 247.5f && angle <= 297.5f)
        {
            WinningAMount = 1;
        }
        else if (angle >= 292.5f && angle <= 337.5f)
        {
            WinningAMount = 18;
        }
        else
        {
            WinningAMount = 1;
        }
        if (ResultActive == 2)
        {
            WinningAMount = WinningAMount * 5;
        }
        else if (ResultActive == 3)
        {
            WinningAMount = WinningAMount * 18;
        }
        else
        {
            WinningAMount = WinningAMount;
        }
        Debug.Log(WinningAMount);
        OpenPopup();
        //Bet(BettAmount ,WinningAMount);
    }

    // public void Bet(int amount, float WinningAMount){
    //     Debug.Log(amount + " fhngh" + WinningAMount);
    //     Login AP = FindObjectOfType<Login>();
    //     AP.BettigResultBt("Spin Wheel",amount, WinningAMount );
    // }

    void OpenPopup()
    {
        // Show the panel
        popupPanel.SetActive(true);

        // If a previous close coroutine is running, stop it
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
        }
        closeCoroutine = StartCoroutine(ClosePopupAfterDelay(2f));
    }
    IEnumerator ClosePopupAfterDelay(float delay)
    {
        float timeRemaining = delay;

        while (timeRemaining > 0)
        {
            AmountText.text = "₹ " + WinningAMount.ToString();
            BtnText.text = "Okay (" + timeRemaining.ToString() + ")";
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1f;
        }
        popupPanel.SetActive(false);
    }



    public void inWheelUp(int NewUp)
    {
        inWheel = NewUp;
        BtnSliver.sprite = BlueSilver;
        BtnGold.sprite = BlueGold;
        BtnDimoned.sprite = BlueDiamond;
        if (inWheel == 1)
        {
            SPin.sprite = SliverSpin;
            BtnSliver.sprite = RedSilver;
            ResultActive = 1;
            BettAmount = 100;
        }
        else if (inWheel == 2)
        {
            SPin.sprite = GoldSpin;
            BtnGold.sprite = RedGold;
            ResultActive = 2;
            BettAmount = 500;
        }
        else
        {
            SPin.sprite = DiamondSpin;
            BtnDimoned.sprite = RedDiamond;
            ResultActive = 3;
            BettAmount = 1800;
        }
        Debug.Log(inWheel);
    }


}