using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.DailyBonus.Models;
using Core.Utils;
using System;

namespace Features.DailyBonus.UI
{
    public class DailyBonusDayItem : MonoBehaviour
    {
        [Header("Day UI")]
        [SerializeField] private TextMeshProUGUI dayNumberText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Visual States")]
        [SerializeField] private GameObject claimedOverlay;
        [SerializeField] private GameObject todayIndicator;
        [SerializeField] private GameObject availableGlow;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color claimedColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color todayColor = new Color(1f, 0.9f, 0.5f);
        [SerializeField] private Color availableColor = new Color(0.5f, 0.9f, 0.5f);
        [SerializeField] private Color lockedColor = new Color(0.65f, 0.65f, 0.65f);

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string claimTriggerName = "Claim";

        [Header("Interaction")]
        [SerializeField] private Button dayButton;

        public event Action<DailyBonusDayItem> OnDayClicked;

        // Data
        private DailyBonusDayData data;

        public bool IsClaimed => data?.isClaimed ?? false;
        public bool IsAvailable => data?.isAvailable ?? false;
        public int DayNumber => data?.dayNumber ?? 0;

        public void Setup(DailyBonusDayData dayData)
        {
            data = dayData;
            bool isClaimed = data.isClaimed;
            bool isAvailable = data.isAvailable;
            bool isToday = data.isToday && !isClaimed && !isAvailable;
            bool isLocked = !isClaimed && !isAvailable;

            if (dayNumberText != null)
                dayNumberText.text = $"Day {data.dayNumber}";

            if (amountText != null)
                amountText.text = data.amountPaisa > 0 ? MoneyFormatter.FormatPaisa(data.amountPaisa) : "";

            if (statusText != null)
            {
                if (isClaimed)
                    statusText.text = "Claimed";
                else if (isAvailable)
                    statusText.text = "Available";
                else
                    statusText.text = "Locked";
            }

            if (claimedOverlay != null)
                claimedOverlay.SetActive(isClaimed);

            if (todayIndicator != null)
                todayIndicator.SetActive(isToday);

            if (availableGlow != null)
                availableGlow.SetActive(isAvailable);

            if (lockedOverlay != null)
                lockedOverlay.SetActive(isLocked);

            if (dayButton != null)
            {
                dayButton.interactable = isAvailable;
                dayButton.onClick.RemoveAllListeners();
                if (isAvailable)
                {
                    dayButton.onClick.AddListener(HandleDayClick);
                }
            }

            if (backgroundImage != null)
            {
                if (isClaimed)
                    backgroundImage.color = claimedColor;
                else if (isAvailable)
                    backgroundImage.color = availableColor;
                else if (isToday)
                    backgroundImage.color = todayColor;
                else if (isLocked)
                    backgroundImage.color = lockedColor;
                else
                    backgroundImage.color = normalColor;
            }
        }

        void HandleDayClick()
        {
            OnDayClicked?.Invoke(this);
        }

        public void PlayClaimAnimation()
        {
            if (animator != null && !string.IsNullOrEmpty(claimTriggerName))
            {
                animator.SetTrigger(claimTriggerName);
            }

            // Also update visual if needed
            if (claimedOverlay != null)
                claimedOverlay.SetActive(true);

            if (statusText != null)
                statusText.text = "Claimed!";
        }

    }
}