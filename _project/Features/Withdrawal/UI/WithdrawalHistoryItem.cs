using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Withdrawal.Models;
using Features.Withdrawal.Controllers;
using Core.Managers;
using Core.Utils;

namespace Features.Withdrawal.UI
{
    public class WithdrawalHistoryItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI methodText;
        [SerializeField] private TextMeshProUGUI netAmountText;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Image statusBadge;
        
        private WithdrawalItem _withdrawal;
        private WithdrawalController _controller;
        
        public void Setup(WithdrawalItem withdrawal, WithdrawalController controller)
        {
            _withdrawal = withdrawal;
            _controller = controller;
            
            amountText.text = MoneyFormatter.FormatPaisa(withdrawal.requested_amount);
            netAmountText.text = $"Net: {MoneyFormatter.FormatPaisa(withdrawal.net_payout_amount)}";
            
            // Status
            string status = withdrawal.status;
            statusText.text = WithdrawalStatus.GetDisplayText(status);
            statusBadge.color = GetColorFromHex(WithdrawalStatus.GetBadgeColor(status));
            
            // Date
            if (!string.IsNullOrEmpty(withdrawal.requested_at))
            {
                if (System.DateTime.TryParse(withdrawal.requested_at, out System.DateTime date))
                {
                    dateText.text = date.ToString("dd MMM yyyy, hh:mm tt");
                }
                else
                {
                    dateText.text = withdrawal.requested_at;
                }
            }
            
            // Method
            methodText.text = withdrawal.payout_method ?? "N/A";
            if (!string.IsNullOrEmpty(withdrawal.masked_account))
            {
                methodText.text += $" ({withdrawal.masked_account})";
            }
            
            // Cancel button
            if (cancelButton != null)
            {
                bool canCancel = controller != null && controller.CanCancelWithdrawal(withdrawal);
                cancelButton.gameObject.SetActive(canCancel);
                if (canCancel)
                {
                    cancelButton.onClick.RemoveAllListeners();
                    cancelButton.onClick.AddListener(OnCancelClicked);
                }
            }
        }
        
        private void OnCancelClicked()
        {
            if (_controller == null || _withdrawal == null) return;

            PopupManager.Instance?.ShowConfirm(
                "Cancel withdrawal?",
                $"This will cancel your withdrawal of {MoneyFormatter.FormatPaisa(_withdrawal.requested_amount)} and refund the amount.",
                "Cancel withdrawal",
                "Keep",
                onConfirm: async () =>
                {
                    var ok = await _controller.CancelWithdrawal(_withdrawal.id);
                    if (!ok) return;
                    statusText.text = WithdrawalStatus.GetDisplayText(WithdrawalStatus.CANCELLED_BY_USER);
                    statusBadge.color = GetColorFromHex(WithdrawalStatus.GetBadgeColor(WithdrawalStatus.CANCELLED_BY_USER));
                    cancelButton.gameObject.SetActive(false);
                });
        }
        
        private Color GetColorFromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;
            return Color.white;
        }
    }
}