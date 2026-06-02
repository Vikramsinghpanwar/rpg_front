using System.Collections;
using System.Collections.Generic;
using Core.Config;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class APIs : MonoBehaviour
{
    public event System.Action<float> OnWalletFetched;

    public void FetchWallet()
    {
        StartCoroutine(RechResultRech());
    }
    IEnumerator RechResultRech()
    {
        string token = UserDetail.Token;
        string url = ServerConfig.BaseUrl + "/api/livewalletdata.php";
        WWWForm form = new WWWForm();
        form.AddField("verToken", token);
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Wallet Fetched Res. : " + jsonResponse);
                //-----------------------------------------------------------------------------//
                
                WalletData[] witharray = JsonHelper.FromJson<WalletData>(jsonResponse);
                if(witharray[0].status == 4)
                {
                    SceneManager.LoadScene(0);
                    yield break;
                }
                
                float myWallet = witharray[0].wallet + witharray[0].WinAmount + witharray[0].bonus;
                OnWalletFetched?.Invoke(myWallet);
                Wallet.teenpattiV_Pool = witharray[0].pool_teenpatti;
            }
            else Debug.Log("unable to fetch wallet");
        }
    }
}

[System.Serializable]
public class WalletData
{
    public float wallet;
    public float WinAmount;
    public float bonus;
    public float pool_teenpatti;
    public int status;
}
