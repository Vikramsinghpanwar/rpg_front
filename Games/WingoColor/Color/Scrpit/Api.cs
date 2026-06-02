using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using Core.Config;

public class Api : MonoBehaviour
{
    public TextMeshProUGUI TextBettingError;
    public GameObject PanelBettingError;
    public GameObject BettingPancelClose;
    public Text UserWalletShow;
    private string Token;
    public static long gameid;

    void Start()
    {
        Application.targetFrameRate = 90;
        Token = UserDetail.Token;
        StartApI(1, 1);
    }


    public void StartApI(int page, int currentPage)
    {
        if (page == 5) { page = 3; } else if (page == 10) { page = 4; } else if (page == 3) { page = 2; }
        StartCoroutine(startApi(page, Token, currentPage));
    }

    IEnumerator startApi(int page, string token, int currentPage)
    {
        Debug.Log(token);
        string url = ServerConfig.BaseUrl + "/api/cp/open" + page + ".php";
        WWWForm form = new WWWForm();
        form.AddField("token", token);
        form.AddField("page", currentPage);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.responseCode == 200 && www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log(jsonResponse);
                CP_histroy ap = FindObjectOfType<CP_histroy>();
                CP_Chart app = FindObjectOfType<CP_Chart>();
                ap.CPGameHistroy(jsonResponse);
                app.chartdata(jsonResponse);

            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
        }
    }

    public void Betting(int amount, string SelectBtn, string SelectType)
    {
        CP_histroy ap = FindObjectOfType<CP_histroy>();
        gameid = ap.gameid;
        StartCoroutine(CpBetting(amount, SelectBtn, gameid, Token, SelectType));
    }
    IEnumerator CpBetting(int amount, string SelectBtn, long gameid, string token, string SelectType)
    {
        string url = ServerConfig.BaseUrl + "/api/cp/betting.php";
        WWWForm form = new WWWForm();
        form.AddField("Btamount", amount);
        form.AddField("SelectBtn", SelectBtn);
        form.AddField("gameid", gameid.ToString());
        form.AddField("gametype", SelectType);
        form.AddField("verToken", token);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.responseCode == 200 && www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log(jsonResponse);
                BettingClass[] BettingData = JsonHelper.FromJson<BettingClass>(jsonResponse);
                if (BettingData[0].status == 1)
                {
                    UserWalletShow.text = "₹" + BettingData[0].wallet.ToString();
                    //UserWalletShow.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
                    OpenErrorPanel(BettingData[0].message);
                    OpenBett be = FindObjectOfType<OpenBett>();
                    be.MyRecord();
                }
                else
                {
                    OpenErrorPanel(BettingData[0].message);
                }
                BettingPancelClose.SetActive(false);
            }
        }
    }

    public void MyBestRecord(string SelectType, int currentPage)
    {
        Debug.Log("" + SelectType);

        StartCoroutine(DelayedCall(SelectType, currentPage));
    }
    private IEnumerator DelayedCall(string SelectType, int currentPage)
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(MyBestRecordS(Token, SelectType, currentPage));
    }
    IEnumerator MyBestRecordS(string token, string SelectType, int currentPage)
    {
        string url = ServerConfig.BaseUrl + "/api/cp/mybetsrecoreds.php";
        WWWForm form = new WWWForm();
        form.AddField("gametype", SelectType);
        form.AddField("verToken", token);
        form.AddField("page", currentPage);
        form.AddField("limit", 10);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.responseCode == 200 && www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log(jsonResponse);
                CP_Betting be = FindObjectOfType<CP_Betting>();
                be.CPBetting(jsonResponse);
            }
        }
    }
    public void OpenErrorPanel(string message)
    {
        PanelBettingError.SetActive(true);
        TextBettingError.text = message;
        Invoke("HideError", 2f);
    }
    void HideError()
    {
        PanelBettingError.SetActive(false);
    }


    // Get data my bets
    // public void CallBetApi(){
    //     StartCoroutine(ApiMyBets(Token));
    // }
    // IEnumerator APiMyBets(string token){
    //     string url = "https://bet777games.online/api/cp/mybetsrecoreds.php";
    //     WWWForm form = new WWWForm();
    //     form.AddField("gametype", SelectType);
    //     form.AddField("verToken", token);
    //     using (UnityWebRequest www =  UnityWebRequest.Post(url, form)){
    //         yield return www.SendWebRequest();
    //         if(www.responseCode == 200 && www.result == UnityWebRequest.Result.Success){
    //                 string jsonResponse = www.downloadHandler.text;
    //                 Debug.Log(jsonResponse);
    //                 BettingClass[] BettingData = JsonHelper.FromJson<BettingClass>(jsonResponse);
    //                 if(BettingData[0].status == 1){
    //                     UserWalletShow.text = BettingData[0].wallet.ToString();
    //                     OpenErrorPanel(BettingData[0].message);
    //                 }else{
    //                     OpenErrorPanel(BettingData[0].message);
    //                 }
    //                 BettingPancelClose.SetActive(false);
    //             }
    //     }
    // }
}

[System.Serializable]
public class BettingClass
{
    public int status;
    public float wallet;
    public string message;
}

//    [System.Serializable]
// public class GameHistoryData {
//     public long gameid;
//     public string number;
//     public string bigsmall;
//     public string color;
// }



// [System.Serializable]
// public class ApiResponse {
//     public int wallet;
//     public float bettamount;
//     public float winningamount;
//     public GameHistoryData[] game_history;

// }


// Helper class for JsonUtility array parsing
