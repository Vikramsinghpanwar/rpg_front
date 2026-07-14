using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Rewards.Models;
using Core.Utils;

namespace Features.Rewards.UI
{
    public class RewardHistoryRow : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text statusText;

        public void Setup(RewardHistoryItem record)
        {
            if (titleText != null)
                titleText.text = record.title ?? string.Empty;

            if (amountText != null)
                amountText.text = MoneyFormatter.FormatPaisa(record.amount);

            if (dateText != null && !string.IsNullOrEmpty(record.createdAt))
            {
                if (System.DateTime.TryParse(record.createdAt, out var dt))
                    dateText.text = dt.ToLocalTime().ToString("dd MMM yyyy HH:mm");
                else
                    dateText.text = record.createdAt;
            }

            if (statusText != null)
                statusText.text = record.status ?? string.Empty;
        }
    }
}
