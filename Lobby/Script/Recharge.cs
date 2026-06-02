// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using UnityEngine.Networking;

// public class Recharge : MonoBehaviour
// {

//     public GameObject[] swithcPanels;
//     public Button[] swithcButtons;
//     private int activePanelIndex = 1;
//     public Sprite normalSprite;
//     public Sprite highlightedSprite;
//     public TextMeshProUGUI principle, bonus, total;
//     public GameObject gateway1;
//     public GameObject gateway2;


//     public GameObject rechargeBtnPrefab;
//     public Transform rechargeBtns_Parent;
//     Error errorRef;
//     bool _isAllowedToFetch;
//     public static int amount;

//     public TMP_InputField rechargeAmountIF;
//     private float timerDuration = 600f; // 10 minutes in seconds
//     private float interval = 20f; // 20 seconds


//     private void OnApplicationFocus(bool focus)
//     {
//         if (_isAllowedToFetch)
//         {
//             liveRechResult();
//         }

//     }
//     void Start()
//     {
//         errorRef = FindObjectOfType<Error>();
//         amount = 300;
//         _isAllowedToFetch = false;
//         PopulateRecharge();
//         for(int i = 0; i< swithcButtons.Length; i++)
//         {
//             int index = i;
//             swithcButtons[i].onClick.AddListener(()=> Switch(index));
//             swithcButtons[i].GetComponent<Image>().sprite = normalSprite;
//         }
//         Switch(0);
//         Debug.Log("gateway 1: " + UserDetail.gateway1);
//         Debug.Log("gateway 2: " + UserDetail.gateway2);
//         Debug.Log("gateway 3: " + UserDetail.gateway3);
//         Debug.Log("gateway 4: " + UserDetail.gateway4);

//         if(UserDetail.gateway1 == 1)
//         {
//             gateway1.SetActive(true);
//         }
//         else
//         {
//             gateway1.SetActive(false);
//         }
        
//         if(UserDetail.gateway2 == 1)
//         {
//             gateway2.SetActive(true);
//         }
//         else
//         {
//             gateway2.SetActive(false);
//         }
        
//     }

//     public void Switch(int index)
//     {
//         if(index == activePanelIndex) return;
//         swithcPanels[activePanelIndex].SetActive(false);
//         swithcButtons[activePanelIndex].GetComponent<Image>().sprite = normalSprite;
//         activePanelIndex = index;
//         swithcButtons[activePanelIndex].GetComponent<Image>().sprite = highlightedSprite;
//         swithcPanels[activePanelIndex].SetActive(true);
//     }




//     public void Select_Recharge(int newAmount, int b)
//     {
//         rechargeAmountIF.text = "₹" + newAmount;
//         amount = newAmount;
//         principle.text = "Principle\n₹" + amount;
//         bonus.text = "Bonus\n₹" + b;
//         total.text = "Total\n₹" + (amount + b);
//     }

//     public void OnAmountEnter()
//     {
//         int a = 0;
//         int.TryParse(rechargeAmountIF.text, out a);
//         if (  a < UserDetail.rechargeData[0].amount)
//         {
//             rechargeAmountIF.text = UserDetail.rechargeData[0].amount.ToString();
//         }
//         amount = a;

//     }

//     public void Gateway1()
//     {
//         BuyCoin();
//     }

//     public void Gateway2()
//     {
//         Login login = FindObjectOfType<Login>();
//         login.Gateway2(amount, 0, UserDetail.Mobile.ToString());
//     }

//     public void BuyCoin()
//     {
//         string mobile = UserDetail.Mobile.ToString();
//         Debug.Log(mobile);
//         if (string.IsNullOrEmpty(mobile))
//         {
//             mobile = "9876543210";  // Assigning the fallback mobile number as a string
//         }

//         Login buyy = FindObjectOfType<Login>();
//         buyy.BuyCoin(amount, 0, mobile);  // Using mobilee instead of mobile
//     }


  
//     public void StartTimer()
//     {
//         // Start the coroutine to handle the timer and function calls
//         StartCoroutine(TimerCoroutine());
//     }

//     IEnumerator TimerCoroutine()
//     {
//         float elapsedTime = 0f;

//         while (elapsedTime < timerDuration)
//         {
//             // Call the function you want to execute every 20 seconds
//             liveRechResult();

//             // Wait for the specified interval
//             yield return new WaitForSeconds(interval);

//             // Update the elapsed time
//             elapsedTime += interval;
//         }
//     }



//     public void AllowToFetchWallet()
//     {
//         _isAllowedToFetch = true;
//     }

//     public void liveRechResult()
//     {
//         StartCoroutine(RechResultRech(UserDetail.Token));
//         _isAllowedToFetch = false;
//     }
//     IEnumerator RechResultRech(string token)
//     {
//         string url = ServerConfig.mainUrl + "/api/livewalletdata.php";
//         WWWForm form = new WWWForm();
//         form.AddField("verToken", token);
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();
//             Debug.Log(www.responseCode);
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
//             {
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log(jsonResponse);
//                 ResultRech[] witharray = JsonHelper.FromJson<ResultRech>(jsonResponse);
//                 if (witharray[0].status == 1)
//                 {
//                     UserDetail.Bonus = witharray[0].bonus;
//                     UserDetail.Wallet = witharray[0].wallet;
//                     UserDetail.WinAmount = witharray[0].WinAmount;

//                     Wallet.SetDepositWallet(witharray[0].wallet);
//                     UserData up = FindObjectOfType<UserData>();
//                     Wallet.slots_Pool = witharray[0].pool_slot;
//                     Wallet.teenpattiV_Pool = witharray[0].pool_teenpatti;
//                     up.WallCha(witharray[0].wallet, witharray[0].bonus, witharray[0].WinAmount, witharray[0].pool);
//                 }
//             }
//         }
//     }




//     #region displayData

//     public void PopulateRecharge()
//     {
//         for(int i = 0; i< UserDetail.rechargeData.Length; i++)
//         {
//             GameObject g = Instantiate(rechargeBtnPrefab, rechargeBtns_Parent);
//             int index = i;
//             g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "₹" + UserDetail.rechargeData[i].amount;

//             g.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = "₹" + UserDetail.rechargeData[i].bonus;
//             g.GetComponent<Button>().onClick.AddListener(() => Select_Recharge(UserDetail.rechargeData[index].amount, UserDetail.rechargeData[index].bonus));
//         }
//     }

//     [System.Serializable]
//     public class RechareButtons
//     {
//         public int amount;
//         public int bonus;
//         public TextMeshProUGUI amountText;
//         public TextMeshProUGUI bonusText;
//     }

//     #endregion


// }
