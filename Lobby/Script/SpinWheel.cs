using System.Collections;
using System.Collections.Generic;
using Core.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

public class SpinWheel : MonoBehaviour
{
    public AudioSource spinSound;
    public float RotatePower;
    public float StopPower;

    private Rigidbody2D rbody;
    int inRotate;
    private int inWheel;
    public Image SPin;
    public Image SliverSpin;
    public Image GoldSpin;
    public Image DiamondSpin;


    public Image GoldBtn;
    public Image SilverBtn;
    public Image DiamondBtn;
    public Image SliverGreenBtn;
    public Image SliverRedBtn;
    public Image GoldGreenBtn;
    public Image GoldRedBtn;
    public Image DiamoneGreenBtn;
    public Image DiamoneRedBtn;

    private Image ActiveBtn;
    private int ResultActive;
    private int BettingResult;
    private float BettAmount;
    private void Start()
    {
        ResultActive = 1;
        BettAmount = 10;
        rbody = GetComponent<Rigidbody2D>();
    }
    public void inWheelUp(int NewUp)
    {
        inWheel = NewUp;
        GoldBtn.sprite = GoldGreenBtn.sprite;
        SilverBtn.sprite = SliverGreenBtn.sprite;
        DiamondBtn.sprite = DiamoneGreenBtn.sprite;
        if (inWheel == 1)
        {
            SPin.sprite = SliverSpin.sprite;
            SilverBtn.sprite = SliverRedBtn.sprite;
            ResultActive = 1;
            BettAmount = 100;
        }
        else if (inWheel == 2)
        {
            SPin.sprite = GoldSpin.sprite;
            GoldBtn.sprite = GoldRedBtn.sprite;
            ResultActive = 2;
            BettAmount = 500;
        }
        else
        {
            SPin.sprite = DiamondSpin.sprite;
            DiamondBtn.sprite = DiamoneRedBtn.sprite;
            ResultActive = 3;
            BettAmount = 1800;
        }
        Debug.Log(inWheel);
    }


    float t;
    private void Update()
    {
        if (rbody.angularVelocity > 0)
        {
            rbody.angularVelocity -= StopPower * Time.deltaTime;

            rbody.angularVelocity = Mathf.Clamp(rbody.angularVelocity, 0, 1440);
        }

        if (rbody.angularVelocity == 0 && inRotate == 1)
        {
            t += 1 * Time.deltaTime;
            if (t >= 0.5f)
            {
                GetReward();

                inRotate = 0;
                t = 0;
            }
        }
    }


    public void Rotete()
    {
        float UserWallet = BootstrapService.Instance.Wallet.deposit_balance / 100f;
        if (UserWallet >= BettAmount)
        {
            if (inRotate == 0)
            {
                spinSound.Play();
                rbody.AddTorque(RotatePower);
                inRotate = 1;
                Betting Bett = FindObjectOfType<Betting>();
                Bett.BettingAMount(BettAmount);
            }
        }
        else
        {
            Debug.Log("Userwallet is not");
        }
    }



    public void GetReward()
    {
        float rot = transform.eulerAngles.z;

        if (rot > 0 + 22 && rot <= 45 + 22)
        {
            GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 45);
            Win(58);
        }
        else if (rot > 45 + 22 && rot <= 90 + 22)
        {
            GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 90);
            Win(1);
        }
        else if (rot > 90 + 22 && rot <= 135 + 22)
        {
            GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 135);
            Win(88);
        }
        else if (rot > 135 + 22 && rot <= 180 + 22)
        {
            GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 180);
            Win(5);
        }
        else if (rot > 180 + 22 && rot <= 225 + 22)
        {
            GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 225);
            Win(888);
        }
        else if (rot > 225 + 22 && rot <= 270 + 22)
        {
            GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 270);
            Win(1);
        }
        else if (rot > 270 + 22 && rot <= 315 + 22)
        {
            GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 315);
            Win(18);
        }
        else if (rot > 315 + 22 && rot <= 360 + 22)
        {
            GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
            Win(1);
        }

    }


    public void Win(int Score)
    {

        if (ResultActive == 1)
        {
            print(Score);
            BettingResult = Score;
        }
        else if (ResultActive == 2)
        {
            print(Score * 2);
            BettingResult = Score * 2;
        }
        else if (ResultActive == 3)
        {
            BettingResult = Score * 3;
            print(Score * 3);
        }
        else
        {
            print("Error" + Score);
        }
        Betting Bett = FindObjectOfType<Betting>();
        Bett.BeetingResult(BettingResult);
    }



    //   void Start()
    // {
    //     isSpinning = false;

    // }

    // private void OnEnable()
    // {
    //     SpinBtn.SetActive(true);
    // }

    // public void SpinWheelS()
    // {
    //     // SpinBtn.SetActive(false);

    //     if (!isSpinning)
    //     {
    //         StartCoroutine(SpinAnimation());
    //     }
    // }

    // public IEnumerator SpinAnimation()
    // {
    //     Debug.Log("spinning");
    //     isSpinning = true;

    //     float startAngle = wheelTransform.rotation.eulerAngles.z;
    //     float endAngle = startAngle + (speed * 360) + Random.Range(1, 360); // Spin one full rotation
    //     float t = 0f;

    //     while (t < 1f)
    //     {
    //         t += Time.deltaTime / spinDuration;
    //         float angle = Mathf.Lerp(startAngle, endAngle, spinCurve.Evaluate(t));
    //         wheelTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    //         yield return null;
    //     }

    //     // Determine the result based on the final angle
    //     float finalAngle = wheelTransform.rotation.eulerAngles.z;
    //     int result = Mathf.FloorToInt(finalAngle / 120f); // 120 degrees per section

    //     // Display the result (you can implement this part)

    //     isSpinning = false;

    //     WinnerCalculator(endAngle);
    // }

    // public void WinnerCalculator(float angle)
    // {
    //     while(angle >= 360)
    //     {
    //         angle -= 360;
    //     }
    //     Debug.Log(angle / 30);
    //     switch (angle / 30)
    //     {
    //         case 0:
    //             break;
    //         case 1:
    //             break;
    //         case 2:
    //             break;
    //         case 3:
    //             break;
    //         case 4:
    //             break;
    //         case 5:
    //             break;
    //         case 6:
    //             break;
    //         case 7:
    //             break;
    //         case 8:
    //             break;
    //         case 9:
    //             break;
    //         case 10:
    //             break;
    //         case 11:
    //             break;
    //         case 12:
    //             break;
    //         case 13:

    //             break;
    //     }
    //     angle = (int)angle / 30;
    //     if(angle == 0 || angle == 3|| angle == 6 || angle == 9){
    //         //red winner
    //     }
    //     else if (angle == 1 || angle == 4 || angle == 7 || angle == 10)
    //     {
    //         //blue winner

    //     }
    //     else if (angle == 2 || angle == 5 || angle == 8 || angle == 11)
    //     {
    //         //yellow winner

    //     }


    // }

    // public void Bet(int amount, int vetOn)
    // {

    // }


}




