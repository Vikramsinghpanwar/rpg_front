using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Core.Config;
using Features.Lobby.Integration;

public class InGameRecharge : MonoBehaviour
{
    public static InGameRecharge instance;
    public TextMeshProUGUI totalGetTMP;
    public TextMeshProUGUI cashTMP;
    public TextMeshProUGUI bonusTMP;
    public Text walletTxt;
    int rechargeAmount;
    int bonusAmount = 100;//fixed isko variable krna h
    public Text rechargeAmountTxt;
    public Text managerWalletTxt;
    public Transform rechargeBtns_Parent;
    public GameObject amountBtnPrefab;
    ScreenOrientation gameOrientation;
    GameObject rechargePanel;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        if (managerWalletTxt != null)
        {
            walletTxt.text = managerWalletTxt.text;
        }
        else
        {
            walletTxt.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        }
    }

    private void Start()
    {
        rechargePanel = transform.GetChild(0).gameObject;
        ArrangeData();
    }

    public Button[] allRechargeBtn_Array;
    void ArrangeData()
    {
        allRechargeBtn_Array = new Button[UserDetail.rechargeData.Length];
        for (int i = 0; i < UserDetail.rechargeData.Length; i++)
        {
            GameObject g = Instantiate(amountBtnPrefab, rechargeBtns_Parent);
            int index = i;
            g.transform.GetChild(0).GetComponent<Text>().text = "₹" + UserDetail.rechargeData[i].amount;
            Button b = g.GetComponent<Button>();
            allRechargeBtn_Array[i] = b;
            g.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = "₹" + UserDetail.rechargeData[i].bonus;
            b.onClick.AddListener(() => SelectRechargeAmount(UserDetail.rechargeData[index].amount, UserDetail.rechargeData[index].bonus, index));
        }
        transform.GetChild(0).gameObject.SetActive(false);
        SelectRechargeAmount(UserDetail.rechargeData[0].amount, UserDetail.rechargeData[0].bonus, 0);

    }
    int activeRechargeBtn = 0;
    public Color primaryColor;
    public void SelectRechargeAmount(int val, int bonus, int index)
    {
        allRechargeBtn_Array[activeRechargeBtn].transform.GetChild(0).GetComponent<Text>().color = Color.black;
        allRechargeBtn_Array[activeRechargeBtn].image.color = Color.white;

        activeRechargeBtn = index;
        allRechargeBtn_Array[index].transform.GetChild(0).GetComponent<Text>().color = Color.white;
        allRechargeBtn_Array[index].image.color = primaryColor;
        rechargeAmount = val;
        rechargeAmountTxt.text = "Add Cash ₹" + val;
        totalGetTMP.text = "₹" + (val + bonus);
        cashTMP.text = "₹" + val;
        bonusTMP.text = "₹" + bonus;
    }

    public void OpenPanel()
    {
        gameOrientation = Screen.orientation;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        rechargePanel.SetActive(true);
    }

    public void ClosePanel()
    {
        rechargePanel.SetActive(false);
        Screen.orientation = gameOrientation;
    }

    public void BuyCoin()
    {
        string mobilee = PlayerPrefs.GetString("mobile");
        int amount = rechargeAmount;
        int bonus = bonusAmount;
        string token = UserDetail.Token;
        rechargePanel.SetActive(false);
        Debug.Log("gateway: " + UserDetail.gateway3 + " gateway2: " + UserDetail.gateway4);
        if (UserDetail.gateway3 == 1)
            StartCoroutine(BuyCoinCoroutine(amount, bonus, token, mobilee));
        else if (UserDetail.gateway4 == 1)
            StartCoroutine(BuyCoinGateway2Coroutine(amount, bonus, mobilee, token));
    }

    [System.Serializable]
    public class RootResponse
    {
        public bool status;
        public string message;
        public string redirect_url;
        public Data data;
    }

    [System.Serializable]
    public class Data
    {
        public bool status;
        public string message;
        public ResultN result;
    }

    [System.Serializable]
    public class ResultN
    {
        public string orderId;
        public string payment_url;
    }


    public IEnumerator BuyCoinGateway2Coroutine(int amount, int bonus, string mobile, string token)
    {
        string url = "https://thecrownempire.live/paytm/create.php";

        WWWForm form = new WWWForm();
        form.AddField("amount", amount);
        form.AddField("token", token);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("gateway response: " + jsonResponse);

                RootResponse response = JsonUtility.FromJson<RootResponse>(jsonResponse);

                if (response != null &&
                    response.data != null &&
                    response.data.result != null)
                {
                    string paymentUrl = response.data.result.payment_url;

                    Debug.Log("Payment URL: " + paymentUrl);

                    if (!string.IsNullOrEmpty(paymentUrl))
                    {
                        Application.OpenURL(paymentUrl);
                    }
                    else
                    {
                        Debug.LogError("Payment URL is null or empty");
                    }
                }
                else
                {
                    Debug.LogError("Invalid JSON structure");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch the gateway");
            }
        }
    }


    IEnumerator BuyCoinCoroutine(int amount, int bonus, string token, string mobilee)
    {
        string url = ServerConfig.GatewayUrl + $"?token={token}&amount={amount}&bonus={bonus}&mobile={mobilee}";
        Application.OpenURL(url);

        yield return new WaitForSeconds(1);
    }

    private long orderId;
    private readonly string apiUrl = "https://pay.imb.org.in/api/create-order";
    private readonly string apiUrlCheck = "https://pay.imb.org.in/api/check-order-status";
    private readonly string userToken = "df60a257148c72bf516ef3a988889fcd";
    private readonly string remark1 = "aberf@gmail.com";
    private readonly string remark2 = "any data";
    private bool isOrderComplete = false;
    private float checkInterval = 5f; // Time interval in seconds
    private float totalDuration = 600f;
    private float amount;

    public async void CreateOrder(float amount, string mobile)
    {
        orderId = GenerateRandomOrderId();
        var formData = new MultipartFormDataContent
        {
            { new StringContent(mobile), "customer_mobile" },
            { new StringContent(userToken), "user_token" },
            { new StringContent(amount.ToString()), "amount" },
            { new StringContent(orderId.ToString()), "order_id" },
            { new StringContent(ServerConfig.GatewayUrl + "/callbacki.php"), "redirect_url" },
            { new StringContent(remark1), "remark1" },
            { new StringContent(remark2), "remark2" }
        };
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.PostAsync(apiUrl, formData);
            string responseContent = await response.Content.ReadAsStringAsync();

            // Parse the JSON response
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var jsonResponse = JsonUtility.FromJson<ApiResponse>(responseContent);

                    if (!string.IsNullOrEmpty(jsonResponse.result.payment_url))
                    {
                        isOrderComplete = false;
                        string paymentUrl = jsonResponse.result.payment_url;
                        Debug.Log("Payment URL: " + paymentUrl);

                        // Redirect to payment URL
                        Application.OpenURL(paymentUrl);
                    }
                    else
                    {
                        Debug.LogError("Payment URL not found in response.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing JSON response: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Request failed: " + response.StatusCode);
            }
        }
    }


    private long GenerateRandomOrderId()
    {
        System.Random random = new System.Random();
        return random.Next(1000000000, 1999999999); // Adjusted for Unity-compatible random
    }

    [System.Serializable]
    private class ApiResponse
    {
        public Result result;
        public int status;
        public string message;
    }

    [System.Serializable]
    private class Result
    {
        public string payment_url;
        public string status;
        public int orderId;
        public string txnStatus;
    }
}
