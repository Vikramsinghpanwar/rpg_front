using System.Collections;
using System.Collections.Generic;
using Core.Config;
using UnityEngine;
using UnityEngine.Networking;

public class Jugad : MonoBehaviour
{
  
    public void SaveGameHistoryDatabase(long periodId, string gameName, float betAmount)
    {
        StartCoroutine(AddGameHistoryInDatabase(periodId, gameName, betAmount));
    }
    
    public void AviatorCancelBet(long periodId, string gameName, float betAmount)
    {
        StartCoroutine(CancelBetAviator(periodId, gameName, betAmount));
    }

    public void UpdateGameHistory(long periodId, float winAmount)
    {
        Debug.Log("period id Up : " + periodId);
        StartCoroutine(UpdateGameHistoryInDatabase(periodId, winAmount));
    }

     public void UpdateGameHistoryBestOf5(long periodId, float winAmount)
    {
        StartCoroutine(UpdateGameHistoryInDatabaseBestOf5(periodId, winAmount));
    }


    public void UpdateTeenpattiWinAmountGameHistory(long periodId, float chaalAmount)
    {
        StartCoroutine(UpdateTeenpattiWinAmountGameHistoryInDatabase(periodId, chaalAmount));
    }

    IEnumerator CancelBetAviator(long periodId, string GameName, float Betamount)
    {
        string url = ServerConfig.BaseUrl + "/api/api/avbet.php";
        WWWForm form = new WWWForm();
        string token = UserDetail.Token;
        form.AddField("verToken", token);
        form.AddField("periodid", periodId.ToString());
        form.AddField("gamename", GameName);
        form.AddField("Betamount", Betamount.ToString());

        //Debug.Log("token or user id : " + token);
        //Debug.Log("period id : " + periodId);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {
                string jsonResponse = www.downloadHandler.text;
                //Debug.Log("response data when adding game history : " + jsonResponse);
                BettingDate[] BettingDateArray = JsonHelper.FromJson<BettingDate>(jsonResponse);
                int status = BettingDateArray[0].status;
                if (status == 1)
                {
                   
                }
                else
                {
                    //api unsuccessful
                }
            }
        }
    }
    
    
    IEnumerator AddGameHistoryInDatabase(long periodId, string GameName, float Betamount)
    {
        string url = ServerConfig.BaseUrl + "/api/bettinggame.php";
        WWWForm form = new WWWForm();
        string token = UserDetail.Token;
        form.AddField("verToken", token);
        form.AddField("periodid", periodId.ToString());
        form.AddField("gamename", GameName);
        form.AddField("Betamount", Betamount.ToString());

        //Debug.Log("token or user id : " + token);
        //Debug.Log("period id : " + periodId);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {
                string jsonResponse = www.downloadHandler.text;
                
                Debug.Log("response data when adding game history : " + jsonResponse);
                BettingDate[] BettingDateArray = JsonHelper.FromJson<BettingDate>(jsonResponse);
                int status = BettingDateArray[0].status;
                if (status == 1)
                {
                   
                }
                else
                {
                    //api unsuccessful
                }
            }
        }
    }

    IEnumerator UpdateGameHistoryInDatabase(long periodId, float winAmount)
    {
        Debug.Log("updating win history");
        Debug.Log("win wali api : " + winAmount);
        string url = ServerConfig.BaseUrl + "/api/bettresult.php";
        WWWForm form = new WWWForm();
        Debug.Log("token : " + UserDetail.Token);
        string token = UserDetail.Token;
        form.AddField("verToken", token);
        form.AddField("periodid", periodId.ToString());
        form.AddField("winAmount", winAmount.ToString());

        
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {
                Debug.Log("period Id : " + periodId);

                string jsonResponse = www.downloadHandler.text;
                Debug.Log("api/bettresult.php response : " + jsonResponse);
                Debug.Log("r : " + jsonResponse);
                BettingDate[] BettingDateArray = JsonHelper.FromJson<BettingDate>(jsonResponse);
                int status = BettingDateArray[0].status;
                Debug.Log(status);
                if (status == 1)
                {
                  
                }
                else
                {
                    
                }
            }
            else
            {
                Debug.Log("unsuccessful");
                //api unsuccessful
            }
        }
    }


    IEnumerator UpdateGameHistoryInDatabaseBestOf5(long periodId, float winAmount)
    {
        Debug.Log("updating win amount in bestOf5 : " + winAmount);
        string url = ServerConfig.BaseUrl + "/api/bettresultbestoffive.php";
        WWWForm form = new WWWForm();
        string token = UserDetail.Token;
        form.AddField("verToken", token);
        form.AddField("periodid", periodId.ToString());
        form.AddField("winAmount", winAmount.ToString());
        Debug.Log("tum");

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            Debug.Log("hhhhhhhhhhhh");
            Debug.Log(www.result);
            Debug.Log(www.responseCode);
            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {

                string jsonResponse = www.downloadHandler.text;
                Debug.Log("data from update : " + jsonResponse);
                BettingDate[] BettingDateArray = JsonHelper.FromJson<BettingDate>(jsonResponse);
                int status = BettingDateArray[0].status;
                if (status == 1)
                {

                }
                else
                {
                    //api unsuccessful
                }
            }
            else
            {
                Debug.Log("unsuccessful");
            }
        }
    }

    IEnumerator UpdateTeenpattiWinAmountGameHistoryInDatabase(long periodId, float betAmount)
    {
        string url = ServerConfig.BaseUrl + "/api/TeenpattiBetAmountUpdate.php";
        WWWForm form = new WWWForm();
        string token = UserDetail.Token;
        form.AddField("verToken", token);
        form.AddField("periodid", periodId.ToString());
        form.AddField("Betamount", betAmount.ToString());


        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {
                string jsonResponse = www.downloadHandler.text;
                BettingDate[] BettingDateArray = JsonHelper.FromJson<BettingDate>(jsonResponse);
                int status = BettingDateArray[0].status;
                if (status == 1)
                {

                }
                else
                {
                    //api unsuccessful
                }
            }
        }
    }

    [System.Serializable]
    public class BettingDate
    {
        public int status;
        public float wallet;
        public float bonus;
        public float winamount;
        public string message;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Wallet.SetWinWallet(1.11111111111111111111f);
        }
    }
}
