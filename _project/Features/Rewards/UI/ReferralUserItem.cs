using TMPro;
using UnityEngine;
using Features.Rewards.Controllers;

namespace Features.Rewards.UI
{
    public class ReferralUserItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text userIdText;
        [SerializeField] private TMP_Text usernameText;
        [SerializeField] private TMP_Text createdAtText;
        [SerializeField] private TMP_Text indexText;

        public void Setup(int index, string publicId, string username, string createdAt)
        {
            if (indexText != null) indexText.text = index.ToString();
            if (userIdText != null) userIdText.text = publicId ?? string.Empty;
            if (usernameText != null) usernameText.text = username ?? string.Empty;
            if (createdAtText != null) createdAtText.text = createdAt ?? string.Empty;
        }
    }
}
