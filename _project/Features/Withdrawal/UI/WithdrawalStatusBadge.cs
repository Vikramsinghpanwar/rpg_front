using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Withdrawal.Models;

namespace Features.Withdrawal.UI
{
    public class WithdrawalStatusBadge : MonoBehaviour
    {
        [SerializeField] private Image badgeImage;
        [SerializeField] private TextMeshProUGUI statusText;
        
        public void SetStatus(string status)
        {
            statusText.text = WithdrawalStatus.GetDisplayText(status);
            if (badgeImage != null)
            {
                ColorUtility.TryParseHtmlString(WithdrawalStatus.GetBadgeColor(status), out Color color);
                badgeImage.color = color;
            }
        }
    }
}