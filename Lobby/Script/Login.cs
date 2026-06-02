// using System.Collections;
// using System.Collections.Generic;
// using System;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Networking;
// using UnityEngine.SceneManagement;




// public class Login : MonoBehaviour
// {
//     // These fields should be assigned in the Inspector
//     public InputField usernameField;
//     public InputField referField;
//     public InputField NameEditField;
//     public InputField otpField; // Added otpField
//     public Text errorText;
//     public Image MobileNumber;
//     public Image OtpNumber;
//     public Image LobbyImage;
//     public Image LoginImage;
//     public Image RenamePanel;


//     public Image AvImagemain;
//     public Image ChooseImage;
//     public Image EditPanelImage;



//     private string Token;

//     public long whatapp;
//     public string telegram;


//     private string loginTokenKey = "LoginToken";
//     public static string AppUrl = ServerConfig.mainUrl + "/api/";
//     public static string AppUrl1 = ServerConfig.mainUrl;

//     //public static string AppUrl = "https://bet777matka.in/api/";
//     //public static string AppUrl1 = "https://bet777matka.in/";

//     [System.Serializable]
//     public class ResponseData
//     {
//         public int status;
//         public string token;
//     }

//     [System.Serializable]
//     public class UpdateProFileIm
//     {
//         public int status;
//         public string imageurl;
//         public string message;
//     }



//     public void liveRechResult()
//     {
//         StartCoroutine(LiveDataUpdate(Token));
//     }
//     IEnumerator LiveDataUpdate(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/livewalletdata.php";
//         WWWForm form = new WWWForm();
//         form.AddField("verToken", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 ResultRech[] witharray = JsonHelper.FromJson<ResultRech>(jsonResponse);
//                 Debug.Log("Live Data Update Response: " + jsonResponse);
//                 if (witharray[0].status == 1)
//                 {
//                     UserDetail.Bonus = witharray[0].bonus;
//                     UserDetail.Wallet = witharray[0].wallet;
//                     UserDetail.WinAmount = witharray[0].WinAmount;

//                     UserData up = FindObjectOfType<UserData>();
//                     Wallet.slots_Pool = witharray[0].pool_slot;
//                     Wallet.teenpattiV_Pool = witharray[0].pool_teenpatti;
//                     Wallet.slots_Pool = witharray[0].pool_slot;
//                     Wallet.teenpattiV_Pool = witharray[0].pool_teenpatti;
//                     up.WallCha(witharray[0].wallet, witharray[0].bonus, witharray[0].WinAmount, witharray[0].pool);
//                 }
//                 else if (witharray[0].status == 4)
//                  {
//                      SceneManager.LoadScene(0);
//                      yield break;
                

//                 }
//             }
//             else
//             {

//             }
//         }
//     }


  

//     private void Start()
//     {
        
//         // Check if a login token exists
//         if (PlayerPrefs.HasKey(loginTokenKey))
//         {
//             Token = PlayerPrefs.GetString(loginTokenKey);  
//         }
//         LobbyImage.gameObject.SetActive(true);
//         StartCoroutine(NoticeImage());
//         liveRechResult();

//         UpdateUploadedImg();

//     }

//     public void UpdateUploadedImg()
//     {
//         if (PlayerPrefs.HasKey("myProfile"))
//         {
//             Sprite s = LoadSpriteFromPlayerPrefs("myProfile");
//             AvImagemain.sprite = s;
//             EditPanelImage.sprite = s;
//             ChooseImage.sprite = s;
//         }
//     }
//     public Sprite LoadSpriteFromPlayerPrefs(string key)
//     {
//         // Get the Base64 string from PlayerPrefs
//         string textureBase64 = PlayerPrefs.GetString(key, null);

//         if (!string.IsNullOrEmpty(textureBase64))
//         {
//             // Convert the Base64 string back to a byte array
//             byte[] textureBytes = Convert.FromBase64String(textureBase64);

//             // Create a Texture2D from the byte array
//             Texture2D texture = new Texture2D(1, 1); // Create a placeholder texture
//             texture.LoadImage(textureBytes); // Load the image data into the texture

//             // Create and return a new Sprite from the Texture2D
//             Rect rect = new Rect(0, 0, texture.width, texture.height);
//             return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
//         }

//         return null; // Return null if no sprite was saved in PlayerPrefs
//     }


//     public void OnLogoutButtonClicked() 
//     {
//         PlayerPrefs.DeleteKey("mobile");
//         PlayerPrefs.DeleteKey(Token);
//         PlayerPrefs.DeleteKey(loginTokenKey);
//         PlayerPrefs.Save();
//         // errorText.text = "Logged out successfully.";
//         SceneManager.LoadScene(0);
//     }


//     public void ProfileImageChange()
//     {

//                     string token = Token;
//             StartCoroutine(ChosseImageChange(ProfileImgPopulator.Instance.selectedIndex, token));


//         // GameObject imageGameObject = GameObject.Find(newImage);
//         // Image imagekd = imageGameObject.GetComponent<Image>();

//         // if (ChooseImage == null && imageGameObject == null && imagekd == null)
//         // {
//         //     Debug.LogError("ChooseImage is not assigned in the Inspector.");
//         //     return;
//         // }
//         // else
//         // {
//         //     ChooseImage.sprite = imagekd.sprite;
//         //     // PlayerPrefs.SetString("NewProfileImage", newImage);
//         //     PlayerPrefs.DeleteKey("myProfile");
//         // }
//     }

//     IEnumerator ChosseImageChange(int index, string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/profileimageupdated.php";
//         WWWForm form = new WWWForm();
//         form.AddField("image_index", index);
//         form.AddField("UserToken", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {

//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("Api is not working");
//             }
//             else
//             {
//                 if (www.responseCode == 200)
//                 {
//                     string jsonResponse = www.downloadHandler.text;
//                     UpdateProFileIm[] responseDataArray = JsonHelper.FromJson<UpdateProFileIm>(jsonResponse);
//                     int status = responseDataArray[0].status;
//                     if (status == 1)
//                     {
//                         string NewProfileImage = responseDataArray[0].imageurl;
//                         UserData.Instance.ProfileImage(ProfileImgPopulator.Instance.Profiles[index]);
//                         Logger.Instance.Log("Profile image updated successfully.");
//                     }
//                 }
//             }
//         }
//     }



   
//     public void NameEditAPi()
//     {
//         if (NameEditField != null)
//         {
//             string nameeditfield = NameEditField.text;
//             string token = Token;
//             StartCoroutine(NameEditCoroutine(nameeditfield, token));
//             UserDetail.UserName = nameeditfield;
//         }
//         else
//         {
//         }
//     }
//     IEnumerator NameEditCoroutine(string editname, string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/nameeditapi.php";
//         Debug.Log("Huuu : " + token);
//         WWWForm form = new WWWForm();
//         form.AddField("nameEdit", editname);
//         form.AddField("nameToken", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("Network error: " + www.error);
//                 errorText.text = "Network error. Please try again later.";
//             }
//             else
//             {
//                 if (www.responseCode == 200)
//                 {
//                     string jsonResponse = www.downloadHandler.text;
//                     NameEditApiData(jsonResponse);
//                 }
//             }
//         }
//     }

//     void NameEditApiData(string jsonResponse)
//     {
//         NameEditArray[] nameEditData = JsonHelper.FromJson<NameEditArray>(jsonResponse);
//         if (nameEditData.Length != null)
//         {
//             int status = nameEditData[0].status;
//             string userNamee = nameEditData[0].userName;
//             Debug.Log("Status" + status);
//             if (status == 1)
//             {
//                 RenamePanel.gameObject.SetActive(false);
//                 UserData ud = FindObjectOfType<UserData>();
//                 ud.UserNameChange(userNamee);
//             }
//             else
//             {
//                 Debug.LogError("Server error");
//             }
//         }
//         else
//         {
//             Debug.LogError("Invalid response format or empty response");
//             errorText.text = "Invalid response format";
//         }
//     }

//     [System.Serializable]
//     public class NameEditArray
//     {
//         public int status;
//         public string userName;
//         public string message;
//     }

//     IEnumerator NoticeImage()
//     {
//         string url = ServerConfig.mainUrl + "/api/getnotice.php";

//         using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, ""))
//         {
//             yield return www.SendWebRequest();

//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Notice Noti = FindObjectOfType<Notice>();
//                 if (Noti != null)
//                 {
//                     Noti.AllBannerName(jsonResponse);
//                 }

//             }
//             else
//             {
//             }
//         }
//     }

//     public void BuyCoin(int amount, int bonus, string mobilee)
//     {
//         string token = Token;
//         StartCoroutine(BuyCoinCoroutine(amount, bonus, token, mobilee));
//     }
//     public void Gateway2(int amount, int bonus, string mobile)
//     {
//         string token = Token;
//         StartCoroutine(BuyCoinGateway2Coroutine(amount, bonus,mobile, Token));
//     }

// public IEnumerator BuyCoinGateway2Coroutine(int amount, int bonus, string mobile, string token)
// {
//     string url = "https://thecrownempire.live/paytm/create.php";

//     WWWForm form = new WWWForm();
//     form.AddField("amount", amount);
//     form.AddField("token", token);

//     using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//     {
//         yield return www.SendWebRequest();

//         if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//         {
//             string jsonResponse = www.downloadHandler.text;
//             Debug.Log("gateway response: " + jsonResponse);

//             RootResponse response = JsonUtility.FromJson<RootResponse>(jsonResponse);

//             if (response != null && 
//                 response.data != null && 
//                 response.data.result != null)
//             {
//                 string paymentUrl = response.data.result.payment_url;

//                 Debug.Log("Payment URL: " + paymentUrl);

//                 if (!string.IsNullOrEmpty(paymentUrl))
//                 {
//                     Application.OpenURL(paymentUrl);
//                 }
//                 else
//                 {
//                     Debug.LogError("Payment URL is null or empty");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Invalid JSON structure");
//             }
//         }
//         else
//         {
//             Debug.LogError("Failed to fetch the gateway");
//         }
//     }
// }
//     [System.Serializable]
// public class RootResponse
// {
//     public bool status;
//     public string message;
//     public string redirect_url;
//     public Data data;
// }

// [System.Serializable]
// public class Data
// {
//     public bool status;
//     public string message;
//     public Result result;
// }

// [System.Serializable]
// public class Result
// {
//     public string orderId;
//     public string payment_url;
// }

//     IEnumerator BuyCoinCoroutine(int amount, int bonus, string token, string mobilee)
//     {
//         string url = ServerConfig.gatewayUrl + $"? token={token}&amount={amount}&bonus={bonus}&mobile={mobilee}";
//         Application.OpenURL(url);

//         yield return new WaitForSeconds(1);
//     }
//     public void UpdateUpiIdAdd(string userUpi, string userHolder)
//     {
//         StartCoroutine(UpdateUpiIdAddIE(userUpi, userHolder, Token));
//     }

//     IEnumerator UpdateUpiIdAddIE(string userUpi, string userHolder, string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/updateupiidadd.php";

//         WWWForm form = new WWWForm();
//         form.AddField("userUpi", userUpi);
//         form.AddField("userHolder", userHolder);
//         form.AddField("nameToken", token);

//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();

//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 UserDetail.UpiId = userUpi;
//                 UserDetail.HolderUpi = userHolder;

//             }
//             else
//             {
//             }
//         }
//     }

//     public void UpdateBankAdd(string bankname, string accountnum, string bankholder, string ifsccode)
//     {
//         StartCoroutine(UpdateBankAddIE(bankname, accountnum, bankholder, ifsccode, Token));
//     }

//     IEnumerator UpdateBankAddIE(string bankname, string accountnum, string bankholder, string ifsccode, string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/updatebankadd.php";

//         WWWForm form = new WWWForm();
//         form.AddField("bankname", bankname);
//         form.AddField("accountnum", accountnum);
//         form.AddField("bankholder", bankholder);
//         form.AddField("ifsccode", ifsccode);
//         form.AddField("Banktoken", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 Logger.Instance.Log("Bank details updated successfully.");
//                 string jsonResponse = www.downloadHandler.text;
//                 UserDetail.Account = accountnum;
//                 UserDetail.HolderBank = bankholder;
//                 UserDetail.IfscCode = ifsccode;
//                 UserDetail.BankName = bankname;

//             }
//             else
//             {
//                 Debug.Log("radhey " + www.result);
//             }
//         }
//     }

//     public void WithProcess(int WithAMount, int upibank)
//     {
//         StartCoroutine(withRequest(WithAMount, upibank, Token));
//     }

//     IEnumerator withRequest(int withamount, int upibank, string Token)
//     {
//         string url = ServerConfig.mainUrl + "/api/withrequest.php";
//         WWWForm form = new WWWForm();
//         form.AddField("AmountWith", withamount);
//         form.AddField("TokenWith", Token);
//         form.AddField("upibank", upibank);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log("withdraw response: " + jsonResponse);
//                 WithAbleamount withdrwalResponse = JsonUtility.FromJson<WithAbleamount>(jsonResponse);
                
//                 if (withdrwalResponse.status == 1)
//                 {
//                     Withdraw withf = FindObjectOfType<Withdraw>();
//                     //withf.withdrawalresult(withdrwalResponse.winning);
//                     liveRechResult();
//                     Logger.Instance.Log("Withdrawal request sent successfully.");
//                     if(Withdraw.Instance != null)
//                     {
//                         Withdraw.Instance.WithHistry();
//                     }

//                 }
//                 else
//                 {
//                     Logger.Instance.Warning("Withdrawal request failed: " + withdrwalResponse.message);
//                     // errorRef.Show("Withdrawal request failed: " + withdrwalResponse.message);
//                 }
//             }
//         }
//     }

//     [System.Serializable]
//     public class WithAbleamount
//     {
//         public int status;
//         public string message;
//         public string winning;
//     }

//     public void rechauto(int amount, string tran, string urlToCopy)
//     {
//         StartCoroutine(RechaMus(amount, tran, Token, urlToCopy));
//     }
//     IEnumerator RechaMus(int withamount, string trans, string Token, string urlToCopy)
//     {
//         string url = ServerConfig.mainUrl + "/api/rechagrerequest.php";
//         WWWForm form = new WWWForm();
//         form.AddField("AmountRech", withamount);
//         form.AddField("TokenRech", Token);
//         form.AddField("trans", trans);
//         form.AddField("urlToCopy", urlToCopy);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 WithAbleamount[] witharray = JsonHelper.FromJson<WithAbleamount>(jsonResponse);
//                 if (witharray[0].status == 1)
//                 {
                  
//                 }
//             }
//         }
//     }

//     public void LevelApi()
//     {
//         StartCoroutine(Levelaapi(Token));
//     }

//     IEnumerator Levelaapi(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/level.php";
//         WWWForm form = new WWWForm();
//         form.AddField("usertoken", Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Level leveda = FindObjectOfType<Level>();
//                 leveda.levedata(jsonResponse);

               
//             }
//         }
//     }

//     public void levelRankapi()
//     {
//         StartCoroutine(levelrankapi(Token));
//     }
//     IEnumerator levelrankapi(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/levelrank.php";
//         WWWForm form = new WWWForm();
//         form.AddField("usertoken", Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log("level rank api response: " + jsonResponse);
//                 Level leveda = FindObjectOfType<Level>();
//                 leveda.levelrankAPI(jsonResponse);

//             }
//         }
//     }

//     public void mybonusapi()
//     {
//         StartCoroutine(Bonusapi(Token));
//     }
//     IEnumerator Bonusapi(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/mybonus.php";
//         WWWForm form = new WWWForm();
//         form.AddField("usertoken", Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log("bonus api response: " + jsonResponse);
//                 Level leveda = FindObjectOfType<Level>();
//                 leveda.MyBonsuResult(jsonResponse);

//             }
//         }
//     }

//     public void selfbonusapi()
//     {
//         StartCoroutine(SelfAPiBonus(Token));
//     }

//     IEnumerator SelfAPiBonus(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/refer.php";
//         WWWForm form = new WWWForm();
//         form.AddField("usertoken", Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log("self bonus api response: " + jsonResponse);
//                 Level Selfb = FindObjectOfType<Level>();
//                 Selfb.SelfBounsResult(jsonResponse);
//             }
//         }
//     }

//     public void dailybonusapi()
//     {
//         StartCoroutine(DailyAPiBonus(Token));
//     }

//     IEnumerator DailyAPiBonus(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/dailybonusrecords.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log("daily bonus api response: " + jsonResponse);
//                 Level Selfb = FindObjectOfType<Level>();
//                 Selfb.DailyBounsResult(jsonResponse);
//             }
//         }
//     }

//     public void WithHistryApi()
//     {
//         StartCoroutine(WithhistryApi());
//     }
//     IEnumerator WithhistryApi()
//     {
//         Debug.Log("WithHistryApi called with token: " + UserDetail.Token);
//         string url = ServerConfig.mainUrl + "/api/withrecord.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", UserDetail.Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             Debug.Log("req sent: ");
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Withdraw Selfb = FindObjectOfType<Withdraw>();
//                 Selfb.WithhistryApiResult(jsonResponse);
//                 Debug.Log("withHistory : " + jsonResponse);
//             }
//             else
//             {
//                 Debug.LogError("Error fetching withdrawal history: " + www.error);
//             }
//         }
//     }
//     public void Gamerecodes()
//     {
//         StartCoroutine(GameRecode(Token));
//     }

//     IEnumerator GameRecode(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/gamerecods.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log("game records: " + jsonResponse);
//                 Withdraw Selfb = FindObjectOfType<Withdraw>();
//                 Selfb.GameRecodeResult(jsonResponse);
//             }
//         }
//     }
//     public void RechageApi()
//     {
//         StartCoroutine(Rechapi(Token));
//     }
//     IEnumerator Rechapi(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/rechagerecods.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log("recharge records: " + jsonResponse);

//                 Withdraw Selfb = FindObjectOfType<Withdraw>();
//                 Selfb.RechapiResult(jsonResponse);
//             }
//         }
//     }


//     public void LevelRewardApi()
//     {
//         StartCoroutine(LevelRewardApi_Enum(Token));
//     }
//     IEnumerator LevelRewardApi_Enum(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/profile-rewards.php"; 
//         WWWForm form = new WWWForm();
//         form.AddField("UserToken", Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log("data : " + jsonResponse);

//                 Withdraw Selfb = FindObjectOfType<Withdraw>();
//                 Selfb.LevelReward(jsonResponse);
//             }
//         }
//     }


//     public void getUpi()
//     {
//         StartCoroutine(RechResultRech());
//     }
//     IEnumerator RechResultRech()
//     {
//         string url = ServerConfig.mainUrl + "/api/GetUpi.php";
//         WWWForm form = new WWWForm();
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 GetUpi upid = FindObjectOfType<GetUpi>();
//                 if (upid != null)
//                 {
//                     upid.GetUpiQr(jsonResponse);
//                 }
//                 else
//                 {
//                     Debug.LogError("GetUpi component not found.");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Error: " + www.error);
//             }
//         }
//     }


//     public void BoundSendOpt(string usemobile, string UserPromocode)
//     {
//         StartCoroutine(GetBoOpt(usemobile, UserPromocode, Token));
//     }

//     IEnumerator GetBoOpt(string usemobile, string UserPromocode, string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/rebound.php";
//         WWWForm form = new WWWForm();
//         form.AddField("userMobile", usemobile);
//         form.AddField("PromoCode", UserPromocode);
//         form.AddField("verToken", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Bound res = FindObjectOfType<Bound>();
//                 if (res != null)
//                 {
//                     res.GetBOund(jsonResponse);
//                     PlayerPrefs.SetString("ReboundMobile", usemobile);
//                 }
//                 else
//                 {
//                 }

//             }
//             else
//             {
//             }
//         }
//     }

//     public void SubmitBonus(int userOpt, string RMobile, string password)
//     {
//         StartCoroutine(GetBoOptSubmit(RMobile, userOpt, Token, password));
//     }
//     IEnumerator GetBoOptSubmit(string usemobile, int userOpt, string token, string password)
//     {
//         string url = ServerConfig.mainUrl + "/api/reboundsubmit.php";
//         WWWForm form = new WWWForm();
//         form.AddField("userMobile", usemobile);
//         form.AddField("VerOtp", userOpt);
//         form.AddField("verToken", token);
//         form.AddField("Password", password);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Bound res = FindObjectOfType<Bound>();
//                 if (res != null)
//                 {
//                     res.GetBOundSumit(jsonResponse);
//                 }
//                 else
//                 {
//                     Debug.LogError("GetUpi component not found.");
//                 }

//             }
//             else
//             {
//                 Debug.Log("radhey " + www.result + " & " + www.responseCode);
//             }
//         }
//     }


//     public void ApplyDaily()
//     {
//         StartCoroutine(GetBoOptApply(Token));
//     }
//     IEnumerator GetBoOptApply(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/getdaily.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 DailyBonus res = FindObjectOfType<DailyBonus>();
//                 res.GetDailly(jsonResponse);

//             }
//         }
//     }

//     public void PersoanlMal()
//     {
//         Debug.Log("Personal mail called with token: " );
//         StartCoroutine(GetPersonal());
//     }
//     IEnumerator GetPersonal()
//     {
//         string url = ServerConfig.mainUrl + "/api/personalmail.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", UserDetail.Token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             Debug.Log("personal mail: " + 22);
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 MailManager res = FindObjectOfType<MailManager>();
//                 Debug.Log("personal mail: " + jsonResponse);
//                 res.GetPmail(jsonResponse);
//             }else
//             {
//                 Debug.LogError("Error fetching personal mail: " + www.error);
//             }
//         }
//     }
//     public void BoardCastMail()
//     {
//         StartCoroutine(GetBoardCastMail(Token));
//     }
//     IEnumerator GetBoardCastMail(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/boradcastmail.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 MailManager res = FindObjectOfType<MailManager>();
//                 //res.GetPmail(jsonResponse);
//             }
//         }
//     }
//     public void Getfestival()
//     {
//         StartCoroutine(GetGfestival(Token));
//     }
//     IEnumerator GetGfestival(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/getfestival.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log(jsonResponse);
//                 Mail res = FindObjectOfType<Mail>();
//                 res.GetFastival(jsonResponse);
//             }
//         }
//     }
//     public void GetMailFestival(int Idd)
//     {
//         StartCoroutine(GetGetMailFestival(Token, Idd));
//     }
//     IEnumerator GetGetMailFestival(string token, int Idd)
//     {
//         string url = ServerConfig.mainUrl + "/api/getfestivalbonus.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", token);
//         form.AddField("OrderIdd", Idd);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 MailBtn res = FindObjectOfType<MailBtn>();
//                 res.GetFastival(jsonResponse);
//             }
//         }
//     }


//     public void BettigResultBt(string GameName, float Betamount, float winAmount)
//     {
//         string token = UserDetail.Token;
//         StartCoroutine(BettigResultBtn(GameName, Betamount, winAmount, Token));
//     }
//     IEnumerator BettigResultBtn(string GameName, float Betamount, float winAmount, string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/Bettingg.php";
//         WWWForm form = new WWWForm();
//         form.AddField("gameName", GameName);
//         form.AddField("verToken", token);
//         form.AddField("Betamount", Betamount.ToString());
//         form.AddField("winAmount", winAmount.ToString());
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log(jsonResponse);
//                 BettingDate[] BettingDateArray = JsonHelper.FromJson<BettingDate>(jsonResponse);
//                 int status = BettingDateArray[0].status;
//                 if (status == 1)
//                 {
//                     UserDetail.Bonus = BettingDateArray[0].bonus;
//                     UserDetail.Wallet = BettingDateArray[0].wallet;
//                     UserDetail.WinAmount = BettingDateArray[0].winamount;
                  
//                     UserData ud = FindObjectOfType<UserData>();

//                     ud.WallCha(BettingDateArray[0].wallet, BettingDateArray[0].bonus, BettingDateArray[0].winamount, Wallet.GetPool());
//                 }
//             }
//         }
//     }
//     [System.Serializable]
//     public class BettingDate
//     {
//         public int status;
//         public float wallet;
//         public float bonus;
//         public float winamount;
//         public string message;
//     }

// }


// [System.Serializable]
// public class ResultRech
// {
//     public int status;
//     public float wallet;
//     public float bonus;
//     public float WinAmount;
//     public float pool;
//     public float pool_teenpatti;
//     public float pool_slot;
// }