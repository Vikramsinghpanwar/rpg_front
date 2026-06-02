using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeeplinkManager : MonoBehaviour
{
    public static DeeplinkManager Instance;

    private string _pendingGame   = null;
    private string _pendingRoomId = null;
    private bool   _isProcessing  = false; 

    // ⚠️ Set these to your exact scene names from Build Settings
    private const string LOBBY_SCENE = "Lobby";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        Application.deepLinkActivated += OnDeepLinkActivated;

        // Cold start (app was closed) — auth→lobby flow handles this already
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            StartCoroutine(DelayedHandle(Application.absoluteURL));
        }
    }

    void OnDestroy()
    {
        Application.deepLinkActivated -= OnDeepLinkActivated;
    }

    // ─── Fires when app is ALREADY OPEN and user clicks deeplink ────────
    void OnDeepLinkActivated(string url)
    {
        Debug.Log("[Deeplink] App was open. URL: " + url);
        HandleDeepLink(url);
    }

    IEnumerator DelayedHandle(string url)
    {
        yield return new WaitForSeconds(2f);
        HandleDeepLink(url);
    }

    // ─── Called when any scene finishes loading ──────────────────────────
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If lobby just loaded AND we have a pending deeplink → execute it
        if (scene.name == LOBBY_SCENE && !string.IsNullOrEmpty(_pendingGame) && !_isProcessing)
        {
            Debug.Log("[Deeplink] Lobby loaded with pending deeplink. Executing...");
            StartCoroutine(ExecutePending());
        }
    }

    // ─── Core: parse URL, save pending, navigate if needed ──────────────
    void HandleDeepLink(string url)
    {
        if (SceneManager.GetActiveScene().name == "Teenpatti")
        {
            if(Teenpatti.GameManager.Instance != null)
            {
                            if (Teenpatti.GameManager.Instance.isRecharging)
                {
                    //Teenpatti.GameManager.Instance.FetchWallet();
                    
                }
            }

        }
        
        if (string.IsNullOrEmpty(url) || !url.Contains("?")) return;
        if (_isProcessing)
        {
            Debug.LogWarning("[Deeplink] Already processing a deeplink, ignoring.");
            return;
        }

        var p      = ParseQueryString(url);
        string game   = p.ContainsKey("game")   ? p["game"].ToLower() : null;
        string roomId = p.ContainsKey("roomId") ? p["roomId"]         : null;

        if (string.IsNullOrEmpty(game) || string.IsNullOrEmpty(roomId)) return;

        Debug.Log($"[Deeplink] game={game} roomId={roomId} currentScene={SceneManager.GetActiveScene().name}");
if(SceneManager.GetActiveScene().name == "Teenpatti")
        {
            Debug.Log("Already in public table");
            Logger.Instance.Error("A game is going on please exit first!");
            return;
            //Teenpatti.GameManager.Instance.OnLobby();
        }
        // Save as pending so OnSceneLoaded can pick it up after lobby loads
        _pendingGame   = game;
        _pendingRoomId = roomId;

        if (SceneManager.GetActiveScene().name == LOBBY_SCENE)
        {
            // Already in lobby — execute right away
            StartCoroutine(ExecutePending());
        }else if(SceneManager.GetActiveScene().name == "Teenpatti")
        {
            Debug.Log("Already in public table");
            Logger.Instance.Error("A game is going on please exit first!");
            //Teenpatti.GameManager.Instance.OnLobby();
        }
        else if(SceneManager.GetActiveScene().name != "Teenpatti")
        {
            // In some other scene (game running) — go to lobby first
            // OnSceneLoaded will trigger ExecutePending() once lobby is ready
            Debug.Log("[Deeplink] Not in lobby. Loading lobby scene...");
            SceneManager.LoadScene(LOBBY_SCENE);
        }
    }

    // ─── Runs after lobby scene is confirmed loaded ──────────────────────
    IEnumerator ExecutePending()
    {
        _isProcessing = true;
        string game   = _pendingGame;
        string roomId = _pendingRoomId;

        // Clear immediately so it doesn't re-trigger
        _pendingGame   = null;
        _pendingRoomId = null;

        switch (game)
        {
            case "teenpatti":
                yield return StartCoroutine(JoinTeenpatti(roomId));
                break;
            case "ludo":
                yield return StartCoroutine(JoinLudo(roomId));
                break;
            default:
                Debug.LogWarning("[Deeplink] Unknown game: " + game);
                break;
        }
        _isProcessing = false;
    }

    IEnumerator JoinTeenpatti(string roomId)
    {
        // Wait for TeenpattiGameDataLobby to initialize (max 5s)
        float elapsed = 0f;
        while (TeenpattiGameDataLobby.Instance == null && elapsed < 5f)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        if (TeenpattiGameDataLobby.Instance == null)
        {
            Debug.LogError("[Deeplink] TeenpattiGameDataLobby never ready!");
            yield break;
        }

        Debug.Log("[Deeplink] Joining Teenpatti: " + roomId);
        // TeenpattiGameDataLobby.Instance.tableID_IF.text = roomId;
        TeenpattiGameDataLobby.Instance.JoinPrivateTable(roomId);
    }

    IEnumerator JoinLudo(string roomId)
    {
        // Add your ludo join logic here
        yield return null;
        Debug.Log("[Deeplink] Joining Ludo: " + roomId);
        // LudoManager.Instance.JoinRoom(roomId);
    }

    Dictionary<string, string> ParseQueryString(string url)
    {
        var result = new Dictionary<string, string>();
        if (!url.Contains("?")) return result;

        foreach (string pair in url.Split('?')[1].Split('&'))
        {
            string[] kv = pair.Split('=');
            if (kv.Length == 2)
                result[Uri.UnescapeDataString(kv[0])] = Uri.UnescapeDataString(kv[1]);
        }
        return result;
    }
}