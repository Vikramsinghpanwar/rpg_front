using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using System;
using Newtonsoft.Json.Linq;
using TMPro;
using Features.Lobby.Integration;

[System.Serializable]
public class ChatMessage
{
    public string type;
    public long id;
    public string playerId;
    public string playerName;
    public string content;
    public int? stickerId;
    public string stickerName;
    public string timestamp;
}

[System.Serializable]
public class PredefinedMessage
{
    public int id;
    public string text;
}

[System.Serializable]
public class Sticker
{
    public int id;
    public string name;
    public string emoji;
}

[System.Serializable]
public class ChatAssets
{
    public PredefinedMessage[] messages;
    public Sticker[] stickers;
}

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;
    public Sprite[] stickerSprites;
    public PopupAnimator[] popupAnimators;

    // UI References
    public GameObject chatPanel;
    public Transform chatContent;
    public GameObject messagePrefab;

    private List<ChatMessage> chatHistory = new List<ChatMessage>();
    private ChatAssets chatAssets;

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
        }
    }

    void Start()
    {
        InitializeChatListeners();
    }

    void InitializeChatListeners()
    {
        if (SocketManager.socket != null)
        {
            Debug.Log("Initializing chat listeners");
            SocketManager.socket.On("new_chat_message", (response) => OnNewChatMessage(response.ToString()));
            SocketManager.socket.On("chat_history", (response) => OnChatHistory(response.ToString()));
            SocketManager.socket.On("chat_assets", (response) => OnChatAssets(response.ToString()));
            SocketManager.socket.On("player_joined_chat", (response) => OnPlayerJoinedChat(response.ToString()));
            SocketManager.socket.On("player_left_chat", (response) => OnPlayerLeftChat(response.ToString()));
        }
    }

    public void OnNewChatMessage(string response)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                Debug.Log("New chat message: " + response);
                JArray jsonArray = JArray.Parse(response);
                JObject messageData = (JObject)jsonArray[0];

                ChatMessage message = new ChatMessage
                {
                    type = (string)messageData["type"],
                    id = (long)messageData["id"],
                    playerId = (string)messageData["playerId"],
                    playerName = (string)messageData["playerName"],
                    content = (string)messageData["content"],
                    timestamp = (string)messageData["timestamp"]
                };

                if (messageData["stickerId"] != null)
                {
                    message.stickerId = (int)messageData["stickerId"];
                    message.stickerName = (string)messageData["stickerName"];
                    if (message.playerId == BootstrapLobbyAdapter.GetUserId())
                    {
                        PopupAnimator popupAnimator = popupAnimators[0];
                        if (popupAnimator != null)
                        {
                            popupAnimator.Play(PopupAnimator.PopupAnimation.Punch, stickerSprites[(int)messageData["stickerId"]]);
                        }
                    }
                    else
                    {
                        PopupAnimator popupAnimator = popupAnimators[1];
                        if (popupAnimator != null)
                        {
                            popupAnimator.Play(PopupAnimator.PopupAnimation.Punch, stickerSprites[(int)messageData["stickerId"]]);
                        }
                    }

                }
                else
                {
                    chatHistory.Add(message);
                    DisplayMessage(message);
                }

            }
            catch (Exception error)
            {
                Debug.LogError("Error processing chat message: " + error.Message);
            }
        });
    }

    public void OnChatHistory(string response)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                Debug.Log("Chat history: " + response);
                JArray jsonArray = JArray.Parse(response);
                List<ChatMessage> history = new List<ChatMessage>();

                foreach (JObject messageData in jsonArray)
                {
                    ChatMessage message = new ChatMessage
                    {
                        type = (string)messageData["type"],
                        id = (long)messageData["id"],
                        playerId = (string)messageData["playerId"],
                        playerName = (string)messageData["playerName"],
                        content = (string)messageData["content"],
                        timestamp = (string)messageData["timestamp"]
                    };

                    if (messageData["stickerId"] != null)
                    {
                        message.stickerId = (int)messageData["stickerId"];
                        message.stickerName = (string)messageData["stickerName"];
                    }

                    history.Add(message);
                }

                chatHistory.AddRange(history);

                foreach (ChatMessage message in history)
                {
                    DisplayMessage(message);
                }
            }
            catch (Exception error)
            {
                Debug.LogError("Error processing chat history: " + error.Message);
            }
        });
    }

    public void OnChatAssets(string response)
    {
        Debug.Log("Chat assets response: " + response);
        MainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                Debug.Log("Chat assets: " + response);
                JArray jsonArray = JArray.Parse(response);
                JObject assetsData = (JObject)jsonArray[0];

                // Parse messages
                JArray messagesArray = (JArray)assetsData["messages"];
                PredefinedMessage[] messages = new PredefinedMessage[messagesArray.Count];
                for (int i = 0; i < messagesArray.Count; i++)
                {
                    messages[i] = new PredefinedMessage
                    {
                        id = (int)messagesArray[i]["id"],
                        text = (string)messagesArray[i]["text"]
                    };
                }

                // Parse stickers
                JArray stickersArray = (JArray)assetsData["stickers"];
                Sticker[] stickers = new Sticker[stickersArray.Count];
                for (int i = 0; i < stickersArray.Count; i++)
                {
                    stickers[i] = new Sticker
                    {
                        id = (int)stickersArray[i]["id"],
                        name = (string)stickersArray[i]["name"],
                        emoji = (string)stickersArray[i]["emoji"]
                    };
                }

                chatAssets = new ChatAssets
                {
                    messages = messages,
                    stickers = stickers
                };

                Debug.Log("Loaded " + chatAssets.messages.Length + " messages and " + chatAssets.stickers.Length + " stickers");
            }
            catch (Exception error)
            {
                Debug.LogError("Error processing chat assets: " + error.Message);
            }
        });
    }

    public void OnPlayerJoinedChat(string response)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                JArray jsonArray = JArray.Parse(response);
                JObject data = (JObject)jsonArray[0];
                string playerName = (string)data["playerName"];

                // Show player joined notification
                Debug.Log("Player joined chat: " + playerName);
                AddSystemMessage($"{playerName} joined the chat");
            }
            catch (Exception error)
            {
                Debug.LogError("Error processing player joined: " + error.Message);
            }
        });
    }

    public void OnPlayerLeftChat(string response)
    {
        MainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                JArray jsonArray = JArray.Parse(response);
                JObject data = (JObject)jsonArray[0];
                string playerName = (string)data["playerName"];

                // Show player left notification
                Debug.Log("Player left chat: " + playerName);
                AddSystemMessage($"{playerName} left the chat");
            }
            catch (Exception error)
            {
                Debug.LogError("Error processing player left: " + error.Message);
            }
        });
    }




























    public void SendPredefinedMessage(int messageId)
    {
        try
        {
            var obj = new Dictionary<string, object>
            {
                { "messageId", messageId },
                { "playerId", BootstrapLobbyAdapter.GetUserId() },
                { "playerName", UserDetail.UserName }
            };
            SocketManager.socket.Emit("send_predefined", obj);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending predefined message: " + e.Message);
        }
    }

    public void SendSticker(int stickerId)
    {

        try
        {
            var obj = new Dictionary<string, object>
            {
                { "stickerId", stickerId },
                    { "playerId", BootstrapLobbyAdapter.GetUserId() },
                { "playerName", UserDetail.UserName }
            };
            SocketManager.socket.Emit("send_sticker", obj);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending sticker: " + e.Message);
        }
    }

    public void SendCustomMessage(TMP_InputField tMP_InputField)
    {
        string text = tMP_InputField.text.Trim();
        tMP_InputField.text = "";
        if (!string.IsNullOrEmpty(text))
        {
            try
            {
                var obj = new Dictionary<string, object>
                {
                    { "text", text },
                        { "playerId", BootstrapLobbyAdapter.GetUserId() },
                { "playerName", UserDetail.UserName }
                };
                SocketManager.socket.Emit("send_message", obj);
            }
            catch (Exception e)
            {
                Debug.LogError("Error sending custom message: " + e.Message);
            }
        }
    }

    void DisplayMessage(ChatMessage message)
    {
        if (messagePrefab == null || chatContent == null) return;

        GameObject messageObj = Instantiate(
            messagePrefab,
            chatContent
        );

        UnityEngine.UI.Text messageText = messageObj.GetComponentInChildren<UnityEngine.UI.Text>();

        if (message.type == "sticker")
        {
            messageText.text = $"{message.playerName}: {message.content}";
            messageText.fontSize = 24; // Larger font for stickers
        }
        else
        {
            messageText.text = $"<color=#00FF17>{message.playerName}</color>: {message.content}";
        }

        // Auto-scroll to bottom
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.ScrollRect scrollRect = chatContent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        // Auto-remove after some time
        //StartCoroutine(AutoRemoveMessage(messageObj, 30f));
    }

    void AddSystemMessage(string text)
    {
        ChatMessage systemMessage = new ChatMessage
        {
            type = "system",
            id = DateTime.Now.Ticks,
            playerName = "System",
            content = text,
            timestamp = DateTime.Now.ToString("HH:mm:ss")
        };

        DisplayMessage(systemMessage);
    }

    IEnumerator AutoRemoveMessage(GameObject messageObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageObj != null)
        {
            Destroy(messageObj);
        }
    }

    public void ToggleChatPanel()
    {
        if (chatPanel != null)
        {
            chatPanel.SetActive(!chatPanel.activeSelf);
        }
    }

    public void RequestChatHistory()
    {
        try
        {
            SocketManager.socket.Emit("get_chat_history");
        }
        catch (Exception e)
        {
            Debug.LogError("Error requesting chat history: " + e.Message);
        }
    }
}