using System.Collections;
using System.Collections.Generic;
using Core.Config;
using UnityEngine;
using UnityEngine.Networking;

public class GetReferalCode : MonoBehaviour
{
    bool _isNew;
    public string refferalCode = "";
    private void OnEnable()
    {
        if (PlayerPrefs.GetInt("_isNew") != 22)
        {
            StartCoroutine(FetchPublicIP());
            //PlayerPrefs.SetInt("_isNew", 22);
            //isko lobby m koi script m dalna hai;
        }
    }

    IEnumerator FetchPublicIP()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://api6.ipify.org");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string publicIP = request.downloadHandler.text;
            StartCoroutine(CheckForReferalCode(publicIP));
        }
        else
        {
            Debug.LogError("Error fetching public IP: " + request.error);
        }
    }


    IEnumerator CheckForReferalCode(string ip_address)
    {
        string url = ServerConfig.BaseUrl + "/api/ipcheck.php";

        WWWForm form = new WWWForm();
        form.AddField("ip", ip_address);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Network error: " + www.error);
            }
            else
            {
                // Check the response from the server
                if (www.responseCode == 200)
                {
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Response: " + jsonResponse);
                    ReferalCodeClass[] rCode = JsonHelper.FromJson<ReferalCodeClass>(jsonResponse);
                    if(rCode[0].status == 1)
                    {
                        refferalCode = rCode[0].promocode;
                        //--------------------------/Login1.instance.signUp_promoIF.text = refferalCode;------------------------------------------
                        Debug.Log("Referal code : " + refferalCode);
                    }
                    else
                    {
                        Debug.Log("hare ");
                    }
                    
                }
                else
                {
                    Debug.Log("error : " + www.downloadHandler.text);
                }
            }
        }
    }
}


[System.Serializable]
class ReferalCodeClass
{
    public string promocode;
    public int status;
}
