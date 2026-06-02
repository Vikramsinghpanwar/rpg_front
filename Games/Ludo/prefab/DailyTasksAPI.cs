using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class DailyTasksAPI : MonoBehaviour
{
    public string baseUrl = Const.Server_Url + "/api/dailytasks";

    public void GetStatus(string userId, Action<string> onJson)
    {
        StartCoroutine(GetStatusCoroutine(userId, onJson));
    }

    IEnumerator GetStatusCoroutine(string userId, Action<string> onJson)
    {
        var url = $"{baseUrl}/status/{UnityWebRequest.EscapeURL(userId)}";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.SetRequestHeader("Accept", "application/json");
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError("GetStatus error: " + www.error);
                onJson?.Invoke(null);
                yield break;
            }

            onJson?.Invoke(www.downloadHandler.text);
        }
    }

    public void ClaimTask(string userId, string taskType, Action<string> onJson)
    {
        StartCoroutine(ClaimTaskCoroutine(userId, taskType, onJson));
    }

    IEnumerator ClaimTaskCoroutine(string userId, string taskType, Action<string> onJson)
    {
        var url = $"{baseUrl}/claim";
        var payload = JsonUtility.ToJson(new ClaimPayload { userId = userId, taskType = taskType });

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, payload))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError("ClaimTask error: " + www.error + " " + www.downloadHandler.text);
                onJson?.Invoke(null);
                yield break;
            }

            onJson?.Invoke(www.downloadHandler.text);
        }
    }

    [Serializable]
    public class ClaimPayload { public string userId; public string taskType; }
}
