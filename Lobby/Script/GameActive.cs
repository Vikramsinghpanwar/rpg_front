using System.Collections;
using System.Collections.Generic;
using Core.Config;
using UnityEngine;
using UnityEngine.Networking;
public class GameActive : MonoBehaviour{

    [System.Serializable]
    public class GameStatus    {
        public string gamename;
        public int active;
    }


    public GameStatus[] games;
    public static GameActive Instance;
    private void Awake() {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    void Start(){
        StartCoroutine(FetchActiveGames());
    }
    IEnumerator FetchActiveGames() {
        string url = ServerConfig.BaseUrl + "/api/gameactive.php";
        WWWForm form = new WWWForm();

        using (UnityWebRequest www = UnityWebRequest.Post(url, form)) {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200) {
                string jsonResponse = www.downloadHandler.text;
                games = JsonHelper.FromJson<GameStatus>(jsonResponse);
                Debug.Log("EWE" + www.downloadHandler.text);
                GameFilter.Instance.PopulateGames();
            } else {
                Debug.LogError("Failed to fetch game data: " + www.error);
            }
        }
    }
}



