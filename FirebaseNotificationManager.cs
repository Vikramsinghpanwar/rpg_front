using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Messaging;
using UnityEngine.Networking;

public class FirebaseNotificationManager : MonoBehaviour
{
    private string apiUrl = "https://yourapi.com/save-token"; // CHANGE THIS

    void Start()
    {
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase Ready");

                FirebaseMessaging.TokenReceived += OnTokenReceived;
                FirebaseMessaging.MessageReceived += OnMessageReceived;

                // Request permission (important for Android 13+)
                FirebaseMessaging.RequestPermissionAsync();
            }
            else
            {
                Debug.LogError("Firebase Dependency Error: " + task.Result);
            }
        });
    }

    void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("FCM Token: " + token.Token);

        // Send token to your backend
        PlayerPrefs.SetString("fcm_token", token.Token);
        //StartCoroutine(SendTokenToServer(token.Token));
    }

    void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Message Received from: " + e.Message.From);
    }

    IEnumerator SendTokenToServer(string token)
    {
        WWWForm form = new WWWForm();
        form.AddField("token", token);

        UnityWebRequest request = UnityWebRequest.Post(apiUrl, form);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Token sent successfully");
        }
        else
        {
            Debug.LogError("Error sending token: " + request.error);
        }
    }
}