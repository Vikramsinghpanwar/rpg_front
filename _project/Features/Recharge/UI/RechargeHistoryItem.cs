using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Recharge.Controllers;

namespace Features.Recharge.UI
{
    public class RechargeHistoryItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text providerText;
        [SerializeField] private TMP_Text bonusText;
        [SerializeField] private Image statusBadge;
        
        public void Setup(Models.RechargeRecord record)
        {
            if (amountText != null)
                amountText.text = RechargeController.FormatAmount(record.amount);
            
            if (statusText != null)
            {
                statusText.text = RechargeController.GetStatusDisplay(record.status);
                statusText.color = RechargeController.GetStatusColor(record.status);
            }
            
            if (statusBadge != null)
                statusBadge.color = RechargeController.GetStatusColor(record.status);
            
            if (dateText != null && !string.IsNullOrEmpty(record.created_at))
            {
                if (System.DateTime.TryParse(record.created_at, out var date))
                {
                    dateText.text = date.ToLocalTime().ToString("dd MMM yyyy HH:mm");
                }
                else
                {
                    dateText.text = record.created_at;
                }
            }
            
            if (providerText != null)
                providerText.text = record.provider ?? "Unknown";
            
            if (bonusText != null && record.bonus_amount > 0)
            {
                bonusText.gameObject.SetActive(true);
                bonusText.text = $"+{RechargeController.FormatAmount(record.bonus_amount)} bonus";
            }
            else if (bonusText != null)
            {
                bonusText.gameObject.SetActive(false);
            }
        }
    }
}