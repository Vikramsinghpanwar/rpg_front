using System.Collections;
using System.Collections.Generic;
using Teenpatti;
using TMPro;
using UnityEngine;

public class EmojiManager : MonoBehaviour
{
    public static EmojiManager instance;
    public GameObject[] emojiPrefabs;
    public string[] react_msgs;
    public GameObject msgPrefab;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }


    public async void ReactWithEmoji(int value)
    {
        if (WebSocketServerRequest.Instance != null)
        {
            await WebSocketServerRequest.Instance.SendAction("react_emoji", value);
        }
    }
    public async void ReactWithMessage(int value)
    {
        if (WebSocketServerRequest.Instance != null)
        {
            await WebSocketServerRequest.Instance.SendAction("react_msg", value);
        }
    }
}
