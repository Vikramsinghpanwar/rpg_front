using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using UnityEngine.Networking;
using System.Collections;
using Features.Lobby.Integration;

public class LobbyUI : MonoBehaviour
{
    static bool isFirstTime = true;
    public TMP_InputField roomIdInput;
    public TextMeshProUGUI roomId_TMP;
    public TextMeshProUGUI private_room_code_TMP;
    public GameObject roomDetailsPanel;
    SocketManager socketManager_Ref;
    public GameObject startGameButton;
    public GameObject joinRoomPanel;
    public static LobbyUI instance;
    public Image[] custom_Room_PlayerImages;
    public Sprite[] playerProfileImages;
    public Sprite unknownPlayerSprite;
    public RoomAmount matchmakingAmount;
    public GameObject matchmakingPanel;
    public MatchmakingAnimation matchmakingAnimation;
    public GameObject paymentLoader;
    public GameObject create_join_room_panel;

    public GameObject referalBonusPopup;
    public TextMeshProUGUI promoCodeTMP;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        socketManager_Ref = FindFirstObjectByType<SocketManager>();
        GameLiveData.instance.roomLength = 2;
        _canRefresh = true;
        Screen.orientation = ScreenOrientation.Portrait;


    }

    public void BackToLobby()
    {
        SceneManager.LoadScene(1);
    }
    public void RoomCreated(string jsonResponse)
    {
        string roomCode = "";
        private_room_code_TMP.text = roomCode;
        roomDetailsPanel.SetActive(true);
    }
    public void OnCreateRoomClicked()
    {
        Debug.Log("connecting");
        socketManager_Ref.ConnectToServer(() =>
        {
            Debug.Log("Now creating room after connection established.");
            socketManager_Ref.CreateAndJoinRoom();
        });
        startGameButton.SetActive(true);
    }

    public void OnJoinRoomClicked()
    {
        string roomId = roomIdInput.text;
        socketManager_Ref.ConnectToServer(() =>
        {
            Debug.Log("Now creating room after connection established." + roomId);
            socketManager_Ref.JoinRoom(roomId);
        });
    }

    public void OnRoomCreated(string roomId)
    {
        roomDetailsPanel.SetActive(true);
        roomId_TMP.text = roomId;
        GameLiveData.instance.playersIdList.Add(BootstrapLobbyAdapter.GetUserId());
    }

    public void OnExitCreatedRoomClicked()
    {
        roomDetailsPanel.SetActive(false);
    }

    public void RoomJoined(string roomId, Dictionary<string, int> profileData)
    {
        Debug.Log("Room joined successfully: " + roomId);
        foreach (var p in profileData)
        {
            Debug.Log("Player ID: " + p.Key);
            Debug.Log("Profile Index: " + p.Value);
        }
        roomDetailsPanel.SetActive(true);
        roomId_TMP.text = roomId;
        joinRoomPanel.SetActive(false);
        startGameButton.SetActive(false);

        foreach (var p in profileData)
        {
            Debug.Log("Player ID: " + p.Key);
            Debug.Log("Profile Index: " + p.Value);
            if (p.Key == BootstrapLobbyAdapter.GetUserId()) continue;
            GameLiveData.instance.playersIdList.Add(p.Key);
            GameLiveData.instance.playersProfileIndexList.Add(p.Value);
        }
        UpdatePlayers();
    }


    public void UpdatePlayers()
    {
        Debug.Log("Updating players in the room.");
        for (int i = 0; i < 3; i++)
        {
            custom_Room_PlayerImages[i].sprite = unknownPlayerSprite;
        }

        int index = 0;
        for (int i = 0; i < GameLiveData.instance.playersIdList.Count; i++)
        {
            Debug.Log("Player ID: " + GameLiveData.instance.playersIdList[i]);
            Debug.Log("Player Profile Index: " + GameLiveData.instance.playersProfileIndexList[index]);
            if (GameLiveData.instance.playersIdList[i] == BootstrapLobbyAdapter.GetUserId())
            {
                Debug.Log("Skipping my own player image.");
                continue;
            }
            Debug.Log("nss: " + playerProfileImages[10].name);
            Debug.Log("n: " + playerProfileImages[GameLiveData.instance.playersProfileIndexList[index]].name);

            custom_Room_PlayerImages[index].sprite = playerProfileImages[GameLiveData.instance.playersProfileIndexList[index]];
            index++;
        }
    }

    public void OnExitCustomRoomClicked()
    {
        roomDetailsPanel.SetActive(false);
        ServerRequest.instance.ExitRoom();
    }
    public void OnStartGameInRoom()
    {
        ServerRequest.instance.StartGameInRoom(roomId_TMP.text);
    }
    public void CopyTMP_to_clipbord(TextMeshProUGUI tmp)
    {
        ShowLog.instance.Show("Copied");
        GUIUtility.systemCopyBuffer = tmp.text;
    }

    public void PlayWithBot()
    {
        OfflineLudoData._botMode = true;
        SceneManager.LoadScene("Offline");
    }

    public void PlayOffline()
    {
        OfflineLudoData._botMode = false;
        SceneManager.LoadScene("Offline");
    }
    public void OnStartMatchmakingClicked(int amount)
    {
        if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < amount)
        {
            ShowLog.instance.Show("Not enough Wallet.");
            return;
        }
        OnStartMatchmakingC(amount);
        //StartCoroutine(CanPlayGame(amount));  
    }

    public IEnumerator CanPlayGame(int amount)
    {
        string url = Const.Server_Url + "api/player/canPlay/" + amount;

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-player-id", BootstrapLobbyAdapter.GetUserId());
        www.SetRequestHeader("x-session-token", BootstrapLobbyAdapter.GetUserId());

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Can Play Game Response: " + www.downloadHandler.text);
            bool canPlay = JsonUtility.FromJson<CanPlayGameResponse>(www.downloadHandler.text).canJoin;
            if (!canPlay)
            {
                ShowLog.instance.Show("Insufficient balance to play the game.");
            }
            else
            {
                OnStartMatchmakingC(amount);
            }
        }
        else
        {
            ShowLog.instance.Show("Error checking if player can play game: " + www.error);
            Debug.LogError("Error checking if player can play game: " + www.error);
        }
    }


    [Serializable]

    public class CanPlayGameResponse
    {
        public bool canJoin;
    }
    public void OnStartMatchmakingC(int amount)
    {
        if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < amount)
        {
            ShowLog.instance.Show("Not enough Wallet.");
            return;
        }
        matchmakingPanel.SetActive(true);

        socketManager_Ref.ConnectToServer(() =>
        {
            socketManager_Ref.Start_Matchmaking(amount);
        });

        matchmakingAnimation.StartAnim();
    }



    public void SelectPlayerCount(int playerCount)
    {
        GameLiveData.instance.roomLength = playerCount;
    }

    public void OnShareButtonClicked()
    {
        string message = "Download the App now.\n" + Const.landingPageURL + "\n\n\nJoin me in this game! Room ID: " + roomId_TMP.text + "\n\n\n\n" + Const.landingPageURL + "?game=ludo?code=" + roomId_TMP.text;
        ShareManager.instance.ShareText(message);
    }


    public void ShareRoomCode(string mode)
    {
        switch (mode)
        {
            case "WhatsApp":
                Application.OpenURL("whatsapp://send?text=Your Friend " + UserDetail.UserName + " has inivited you to join a private table on The Crown Empire. \n\nRoom ID: " + roomId_TMP.text);
                break;
            case "Telegram":
                Application.OpenURL("https://t.me/share/url?url=Your Friend " + UserDetail.UserName + " has inivited you to join a private table on The Crown Empire. \n\nRoom ID: " + roomId_TMP.text);
                break;
            case "Copy":
                GUIUtility.systemCopyBuffer = "Your Friend " + UserDetail.UserName + " has inivited you to join a private table on The Crown Empire. \n\nRoom ID: " + roomId_TMP.text;
                ShowLog.instance.Show("Room ID copied to clipboard");
                break;
        }
    }
    public void OpenLink(string url = "https://ludob.win4cash.in/")
    {
        Debug.Log("wanna open url : " + url);
        Application.OpenURL(url);
    }

    public void CloseMatchmakingPanel()
    {
        matchmakingAnimation.StopAnimation();
        SocketManager.Instance.Disconnect();
    }

    public void OnStartMatchMaking(int amount)
    {
        GameLiveData.instance.roomLength = 2;


        if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < amount)
        {
            ShowLog.instance.Show("Not enough Wallet.");
            return;
        }
        matchmakingPanel.SetActive(true);

        socketManager_Ref.ConnectToServer(() =>
        {
            socketManager_Ref.Start_Matchmaking(amount);
        });

        matchmakingAnimation.StartAnim();
    }

    public void OnPrivateRoomPlayer(int amount, int count)
    {
        Debug.Log("radhey");
        GameLiveData.instance.roomLength = count;
        if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < amount)
        {
            ShowLog.instance.Show("Not enough Wallet.");
            return;
        }
        create_join_room_panel.SetActive(true);


    }

    public bool _canRefresh;
    public GameObject refreshBtn;


    void ResetRefreshTimer()
    {
        _canRefresh = true;
    }
}



