using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowLog : MonoBehaviour
{
    public GameObject LogObject;
    TextMeshProUGUI textObj;
    public static ShowLog instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = new ShowLog();
            DontDestroyOnLoad(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        textObj = LogObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        LogObject.SetActive(false);

    }

    public void Show(string msg, float time = 3f)
    {
#if UNITY_ANDROID || PLATFORM_ANDROID
                    Handheld.Vibrate();
                #endif

        Debug.Log("ShowLog: " + msg);
        textObj.text = msg;
        LogObject.SetActive(true);
        Invoke("off", time);
    }
    void off()
    {
        LogObject.SetActive(false);
    }
    
    public void CopyError()
    {
        GUIUtility.systemCopyBuffer = textObj.text;
    }
}
