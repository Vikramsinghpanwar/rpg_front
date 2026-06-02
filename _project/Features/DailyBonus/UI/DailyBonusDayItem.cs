using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.DailyBonus.Models;
using Core.Utils;

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
        [SerializeField] private Image backgroundImage;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color claimedColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color todayColor = new Color(1f, 0.9f, 0.5f);
        [SerializeField] private Color availableColor = new Color(0.5f, 0.9f, 0.5f);
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string claimTriggerName = "Claim";

        // Data
        private DailyBonusDayData data;
        
        public bool IsClaimed => data?.isClaimed ?? false;
        public bool IsAvailable => data?.isAvailable ?? false;

        public void Setup(DailyBonusDayData dayData)
        {
            data = dayData;
            
            // Update text
            if (dayNumberText != null)
                dayNumberText.text = $"Day {data.dayNumber}";
            
            if (amountText != null)
                amountText.text = data.amountPaisa > 0 ? MoneyFormatter.FormatPaisa(data.amountPaisa) : "";
            
            if (statusText != null)
            {
                if (data.isClaimed)
                    statusText.text = "Claimed!";
                else if (data.isAvailable)
                    statusText.text = "Claim Now!";
                else
                    statusText.text = "";
            }
            
            // Update visual states
            if (claimedOverlay != null)
                claimedOverlay.SetActive(data.isClaimed);
            
            if (todayIndicator != null)
                todayIndicator.SetActive(data.isToday);
            
            if (availableGlow != null)
                availableGlow.SetActive(data.isAvailable);
            
            // Update background color
            if (backgroundImage != null)
            {
                if (data.isClaimed)
                    backgroundImage.color = claimedColor;
                else if (data.isAvailable)
                    backgroundImage.color = availableColor;
                else if (data.isToday)
                    backgroundImage.color = todayColor;
                else
                    backgroundImage.color = normalColor;
            }
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