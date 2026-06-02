// using System.Collections;
// using System.Net.Http;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.Networking;
// using System.Net.Http.Headers;



// public class PaymentGateway : MonoBehaviour
// {
//     private string mobile;
//     private long orderId;
//     private readonly string apiUrl = "https://pay.imb.org.in/api/create-order";
//     private readonly string apiUrlCheck = "https://pay.imb.org.in/api/check-order-status";
//     private readonly string userToken = "df60a257148c72bf516ef3a988889fcd";
//     private readonly string remark1 = "aberf@gmail.com";
//     private readonly string remark2 = "any data";
//     private bool isOrderComplete = false;
//      private float checkInterval = 5f; // Time interval in seconds
//     private float totalDuration = 600f;
//     private float amount;

//     public long GenerateRandom10DigitNumber()
//     {
//         // Create an instance of System.Random
//         System.Random random = new System.Random();

//         // Generate a random number in the range 1000000000 to 9999999999
//         long randomNumber = (long)(random.NextDouble() * (9999999999L - 1000000000L) + 1000000000L);

//         return randomNumber;
//     }

//     // public void Payment(){
//     //     mobile = UserDetail.Mobile.ToString();
//     //     if(mobile.Length < 5){
//     //         mobile = GenerateRandom10DigitNumber().ToString();
//     //     }
//     //     amount = Recharge.amount;
//     //     //amount = 1;
//     //     CreateOrder(amount, mobile);
//     // }

//     public async void CreateOrder(float amount, string mobile){
//         orderId = GenerateRandomOrderId();
//         var formData = new MultipartFormDataContent
//         {
//             { new StringContent(mobile), "customer_mobile" },
//             { new StringContent(userToken), "user_token" },
//             { new StringContent(amount.ToString()), "amount" },
//             { new StringContent(orderId.ToString()), "order_id" },
//             { new StringContent(ServerConfig.gatewayUrl + "/callbacki.php"), "redirect_url" },
//             { new StringContent(remark1), "remark1" },
//             { new StringContent(remark2), "remark2" }
//         };
//         using (HttpClient client = new HttpClient())
//         {
//             HttpResponseMessage response = await client.PostAsync(apiUrl, formData);
//             string responseContent = await response.Content.ReadAsStringAsync();

//             // Parse the JSON response
//             if (response.IsSuccessStatusCode)
//             {
//                 try
//                 {
//                     var jsonResponse = JsonUtility.FromJson<ApiResponse>(responseContent);

//                     if (!string.IsNullOrEmpty(jsonResponse.result.payment_url))
//                     {
//                         isOrderComplete = false;
//                         string paymentUrl = jsonResponse.result.payment_url;
//                         Debug.Log("Payment URL: " + paymentUrl);
//                         StartCoroutine(CheckOrderRepeatedly());

//                         // Redirect to payment URL
//                         Application.OpenURL(paymentUrl);
//                     }
//                     else
//                     {
//                         Debug.LogError("Payment URL not found in response.");
//                     }
//                 }
//                 catch (System.Exception e)
//                 {
//                     Debug.LogError("Error parsing JSON response: " + e.Message);
//                 }
//             }
//             else
//             {
//                 Debug.LogError("Request failed: " + response.StatusCode);
//             }
//         }
//     }


    
//     IEnumerator CheckOrderRepeatedly(){
//         float elapsedTime = 0f;
//         while (elapsedTime < totalDuration && !isOrderComplete){
//             yield return new WaitForSeconds(checkInterval);
//             CheckOrderStatus();
//             elapsedTime += checkInterval;
//         }
//         Debug.Log("10 minutes completed. Stopping order checks.");
//     }

//     public class Jdata{
//         public string user_token;
//         public long order_id;
//     }

//     public async void CheckOrderStatus()
//     {
//         // Prepare the request data
//         Jdata requestData = new Jdata{};
//         requestData.user_token = userToken;
//         requestData.order_id = orderId;
  
//         string jsonData = JsonUtility.ToJson(requestData);
//         using (HttpClient client = new HttpClient())
//         {
//             try
//             {
//                 // Set headers and make the POST request
//                 var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
//                 requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

//                 HttpResponseMessage response = await client.PostAsync(apiUrlCheck, requestContent);

//                 // Read the response content
//                 string responseContent = await response.Content.ReadAsStringAsync();

//                 var jsonResponse = JsonUtility.FromJson<ApiResponse>(responseContent);
//                 Debug.Log(jsonResponse.ToString());
//                 string startu = jsonResponse.result.status;
//                 if (startu == "SUCCESS")
//                 {
//                     isOrderComplete = true;
//                     long orderIdd = jsonResponse.result.orderId; // Use the correct orderId from the response
//                     StartCoroutine(APiCall(orderIdd)); // Pass the correct orderId here
//                     Debug.Log("Payment Status: " + jsonResponse.result.txnStatus);
//                 }else{
//                     Debug.Log("Payment Status: Wait" );

//                 }
               
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError("Exception occurred: " + e.Message);
//             }
//         }
//     }

//     IEnumerator APiCall(long OrderIdd){
//         string Token = UserDetail.Token;
//         string url = ServerConfig.mainUrl + "/api/paymentstattdfkjf.php";
//         WWWForm form = new WWWForm();
//         form.AddField("orderidsg", OrderIdd.ToString());
//         form.AddField("nameToken", Token);
//         form.AddField("nameamount", amount.ToString());

//         using (UnityWebRequest www = UnityWebRequest.Post(url, form)){
//             yield return www.SendWebRequest();
//             if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200){
//                 string jsonResponse = www.downloadHandler.text;
//                 Debug.Log(jsonResponse);
//                 var apidata = JsonUtility.FromJson<ApiResponse>(jsonResponse);
//                 if(apidata.status == 1){
//                     Login App = FindObjectOfType<Login>();
//                     App.liveRechResult();
//                 }
//             }
//         }
//     }

//     private long GenerateRandomOrderId()
//     {
//         System.Random random = new System.Random();
//         return random.Next(1000000000, 1999999999); // Adjusted for Unity-compatible random
//     }

//     [System.Serializable]
//     private class ApiResponse
//     {
//         public Result result;
//         public int status;
//         public string message;
//     }

//     [System.Serializable]
//     private class Result
//     {
//         public string payment_url;
//         public string status;
//         public int orderId;
//         public string txnStatus;
//     }
// }
