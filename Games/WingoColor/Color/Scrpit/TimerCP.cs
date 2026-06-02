using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class TimerCP : MonoBehaviour
{
    public Sprite activeImg;
    public AudioSource beepSound;
    public Animator refreshAnimator;
    public GameObject roda;
    public TextMeshProUGUI timerTxt;
    public Color myRedColor;
    public Color neela;
    private DateTime targetDate = new DateTime(2024, 2, 8, 12, 0, 0);
    private float timer;

    public int GameTime;
    public Image[] TimerBtn;
    public TextMeshProUGUI TimerSecound1;
    public TextMeshProUGUI TimerSecound2;
    public TextMeshProUGUI TimerSecound3;
    public TextMeshProUGUI TimerSecound4;
    public TextMeshProUGUI HidePanel1;
    public TextMeshProUGUI HidePanel2;
     public GameObject HideBettingPanel;
     private bool RefreshBool;
     public GameObject bettingPopup;

    void Start(){
        GameTime = 1;
         RefreshBool = false ;
        InvokeRepeating("UpdateTimer", 0f, 1f);
        roda.SetActive(false);
    }

    void UpdateTimer() {
        DateTime now = DateTime.Now;
        TimeSpan timeDiff = (targetDate - now );
        timeDiff = timeDiff + TimeSpan.FromSeconds(5);
        int totalMinutes = (int)timeDiff.TotalMinutes;
        int seconds = (int)timeDiff.TotalSeconds;
        
        // Extract remaining seconds in the current minute
        int remainingMinutes = (totalMinutes % 60)+59;
        int remainingSeconds = ((seconds % 60)+60);
        int Min = (remainingMinutes%GameTime);
        // Convert minutes and remaining seconds to strings
        string minutesStr = Min.ToString("00");
        string secondsStr = remainingSeconds.ToString("00");
        
        // Displaying minutes and seconds
        TimerSecound1.text = minutesStr[0].ToString(); 
        TimerSecound2.text = minutesStr[1].ToString();  
        TimerSecound3.text = secondsStr[0].ToString();  
        TimerSecound4.text = secondsStr[1].ToString();

        int x = int.Parse(secondsStr);
        if (x <= 10 && minutesStr == "00"){
            bettingPopup.SetActive(false);
            beepSound.Play();
            HideBettingPanel.SetActive(true);
            HidePanel1.text = secondsStr[0].ToString();
            HidePanel2.text = secondsStr[1].ToString();
            roda.SetActive(true);
            if(x == 1){
                Invoke("TimeEnd", 1f);
            }
        }else{
            HideBettingPanel.SetActive(false);
        }


    }



    void TimeEnd(){
        Api op = FindObjectOfType<Api>();
        op.StartApI(GameTime, 1);
        HideBettingPanel.SetActive(false);
        OpenBett rref = FindObjectOfType<OpenBett>();
        if(rref.activemybetsbtn == true){
            OpenBett ap = FindObjectOfType<OpenBett>();
            ap.MyBestRecords(1);
        }
        roda.SetActive(false);

    }
    public void CHangeTimer(int timerrrr){
        
        GameTime= timerrrr;  
        Api ap = FindObjectOfType<Api>();
        ap.StartApI(GameTime, 1);
        CP_Betting app = FindObjectOfType<CP_Betting>();
        app.ChangePage();
        timerTxt.text = "WinX " + timerrrr + "Min";
  
    }
    public void Refresh(){
        if(RefreshBool == false){
            refreshAnimator.SetBool("_is", true);
            RefreshBool = true;
            Api ap = FindObjectOfType<Api>();
            ap.StartApI(GameTime,1);
            Invoke("RefreshActive", 5F);
        }
    }

    void RefreshActive(){
        RefreshBool = false;
        refreshAnimator.SetBool("_is", false);

    }

    public void ChangeBtnTImer(Image ChangeBtn){

        for (int i = 0; i < TimerBtn.Length; i++){
            TimerBtn[i].sprite = null;
            TimerBtn[i].transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.gray;
        }
        ChangeBtn.sprite = activeImg;
        ChangeBtn.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

    }
}


