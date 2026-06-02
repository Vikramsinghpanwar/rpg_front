using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teenpatti;

public class GiftSystem : MonoBehaviour
{

    public AudioSource giftAudioSource;
    public AudioClip[] audioClipArray;
    public Transform gift_anim_panel_Parent;
    public static GiftSystem instance;
    [Header("Profile Buttons (Assign 5)")]
    public Button[] profileButtons;

    [Header("Gift Panel")]
    public GameObject giftPanel;

    [Header("Gift Prefabs")]
    public GameObject[] giftPrefabs;

    // [Header("Gift Container")]
    // public Transform giftContainer;

    [Header("Animation Settings")]
    public float moveDuration = 1f;
    public float stayDuration = 1f;

    private string currentTargetId;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        // Attach click listeners to profile buttons
        for (int i = 0; i < profileButtons.Length; i++)
        {
            int index = i;
            profileButtons[i].onClick.AddListener(() => OnProfileClicked(index));
        }

        //PopulateGiftPanel();
    }

    // void PopulateGiftPanel()
    // {
    //     // foreach (Transform child in giftContainer)
    //     // {
    //     //     Destroy(child.gameObject);
    //     // }

    //     for (int i = 0; i < giftPrefabs.Length; i++)
    //     {
    //         int index = i;

    //         GameObject giftObj = Instantiate(giftPrefabs[i], giftContainer);
    //         Button btn = giftObj.GetComponent<Button>();

    //         if (btn == null)
    //             btn = giftObj.AddComponent<Button>();

    //         btn.onClick.AddListener(() => OnGiftClicked(index));
    //     }
    // }

    void OnProfileClicked(int profileIndex)
    {
        PlayerManager player = Teenpatti.GameManager.Instance.playersList[profileIndex];

        if (player == null || player.isVacant || player.isLocalPlayer)
            return;

        currentTargetId = player.myId;
        giftPanel.SetActive(true);
    }

    public async void OnGiftClicked(int giftIndex)
    {
        giftPanel.SetActive(false);
        await SendGiftToServer(currentTargetId, giftIndex);
    }

    // =========================
    // SEND TO SERVER
    // =========================
    public async Task SendGiftToServer(string targetId, int giftIndex)
    {
        if (WebSocketServerRequest.Instance != null)
        {
            await WebSocketServerRequest.Instance.SendAction("send_gift", giftIndex, targetId);
        }
    }

    // =========================
    // RECEIVE FROM SERVER
    // Call this from your WebSocket message handler
    // =========================
    public void OnGiftReceived(string senderId, string targetId, int giftIndex)
    {
        Debug.Log("gift reccevied: " + senderId + " : " + targetId + " : " + giftIndex);
        PlayerManager sender = Teenpatti.GameManager.Instance.GetPlayerByID(senderId);
        PlayerManager target = Teenpatti.GameManager.Instance.GetPlayerByID(targetId);

        if (sender == null || target == null) return;

        StartCoroutine(AnimateGift(sender, target, giftIndex));
        giftAudioSource.PlayOneShot(audioClipArray[giftIndex]);
    }

    System.Collections.IEnumerator AnimateGift(PlayerManager sender, PlayerManager target, int giftIndex)
    {
        Debug.Log("Animating");
        GameObject gift = Instantiate(giftPrefabs[giftIndex], gift_anim_panel_Parent);

        Vector3 startPos = sender.profileImg.transform.position;
        Vector3 endPos = target.profileImg.transform.position;

        gift.transform.position = startPos;

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            gift.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        gift.transform.position = endPos;

        yield return new WaitForSeconds(stayDuration);

        Destroy(gift);
    }
}
