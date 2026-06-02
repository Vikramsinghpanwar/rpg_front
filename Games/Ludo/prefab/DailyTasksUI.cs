// DailyTasksUI.cs
using UnityEngine;
using TMPro;
using System;

public class DailyTasksUI : MonoBehaviour
{
    public DailyTasksAPI api;
    public string userId;
    public TMP_Text gamesPlayedText;
    public TMP_Text gamesWonText;
    public GameObject play10ClaimBtn;
    public GameObject win5ClaimBtn;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        api.GetStatus(userId, (json) => {
            if (string.IsNullOrEmpty(json)) return;
            var wrapper = JsonUtility.FromJson<StatusWrapper>(json);
            if (wrapper == null || !wrapper.success) return;
            var s = wrapper.status;
            gamesPlayedText.text = $"Played: {s.gamesPlayed}";
            gamesWonText.text = $"Won: {s.gamesWon}";
            play10ClaimBtn.SetActive(s.play10Eligible && !s.play10Claimed);
            win5ClaimBtn.SetActive(s.win5Eligible && !s.win5Claimed);
        });
    }

    public void OnClaimPlay10()
    {
        api.ClaimTask(userId, "PLAY_10", (json) => {
            Debug.Log("Claim response: " + json);
            Refresh();
        });
    }

    public void OnClaimWin5()
    {
        api.ClaimTask(userId, "WIN_5", (json) => {
            Debug.Log("Claim response: " + json);
            Refresh();
        });
    }

    [Serializable]
    class StatusWrapper
    {
        public bool success;
        public Status status;
    }
    [Serializable]
    class Status
    {
        public int gamesPlayed;
        public int gamesWon;
        public bool play10Eligible;
        public bool play10Claimed;
        public bool win5Eligible;
        public bool win5Claimed;
    }
}
