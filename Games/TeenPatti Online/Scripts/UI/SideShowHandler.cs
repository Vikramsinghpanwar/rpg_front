using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Teenpatti;
using System.Collections;

public class SideShowHandler : MonoBehaviour
{
            [System.Serializable]
        public class WSMessage<T>
        {
            public string type;   // "join"
            public T data;
        }
    public static SideShowHandler Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject sideShowPanel;
    public TextMeshProUGUI sideShowText;
    public TextMeshProUGUI coutDownText;
    public Image sideShowProfile;
    public Button acceptButton;
    public Button declineButton;

    public Transform targetIMG;
    public Coroutine countDown_coroutine;
        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(AcceptSideShow);
            
        if (declineButton != null)
            declineButton.onClick.AddListener(DeclineSideShow);
    }
    
    public void ShowSideShowRequest(string requesterID, string targetID, int timeout = 10)
    {
        PlayerManager requester = Teenpatti.GameManager.Instance.GetPlayerByID(requesterID);
        PlayerManager target = Teenpatti.GameManager.Instance.GetPlayerByID(targetID);
        if (sideShowPanel != null && sideShowText != null && targetID == Teenpatti.GameManager.Instance.localPlayer.myId)
        {
            sideShowPanel.SetActive(true);
            sideShowText.text = $"{target.nameTxt.text} wants to side show with.\nDo you accept?";
            sideShowProfile.sprite = target.profileImg.sprite; 
            if (countDown_coroutine != null)
            {
                StopCoroutine(countDown_coroutine);
            }
            countDown_coroutine = StartCoroutine(Countdown(timeout));
        }
        else
        {
            StartCoroutine(MoveTarget(requester.transform.GetChild(1).position, target.transform.GetChild(1).position, targetIMG));
        }
    }

    IEnumerator MoveTarget(Vector3 initialPos, Vector3 targetPos, Transform targetObj, float speed = 2000f)
    {
        targetObj.gameObject.SetActive(true);
        targetObj.position = targetPos;
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(initialPos, targetPos);

        while (Time.time - startTime < journeyLength / speed)
        {
            float fraction = (Time.time - startTime) / (journeyLength / speed);
            targetObj.position = Vector3.Lerp(initialPos, targetPos, fraction);
            yield return null;
        }
        targetObj.position = targetPos;
        yield return new WaitForSeconds(2);
        targetObj.gameObject.SetActive(false);

    }

    IEnumerator Countdown(int val = 10)
    {
        for(int i= val; val >=0; i--)
        {
            coutDownText.text = i.ToString();
            yield return new WaitForSeconds(1);
        }
    }
    
    public void HideSideShowPanel()
    {
        if (sideShowPanel != null)
        {
            sideShowPanel.SetActive(false);
        }
    }
    
    public async void AcceptSideShow()
    {
        // Send accept response to server
        if (WebSocketServerRequest.Instance != null)
        {
            await WebSocketServerRequest.Instance.SendAction("side_show_response", 0, "", true);
        }        
        HideSideShowPanel();
    }
    
    public async void DeclineSideShow()
    {
        // Send decline response to server
        if (WebSocketServerRequest.Instance != null)
        {
            await WebSocketServerRequest.Instance.SendAction("side_show_response", 0, "",  false);
        }        
        HideSideShowPanel();
        
    }

            [System.Serializable]
        public class SideShowResponseData
        {
            public string  userID;
            public bool accept;
        }
        
}