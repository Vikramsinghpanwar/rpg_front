using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.DailyBonus.Controllers;
using Features.DailyBonus.Models;
using Core.Utils;

namespace Features.DailyBonus.UI
{
    public class DailyBonusScreen : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private DailyBonusController controller;

        [Header("Reward Preview")]
        [SerializeField] private TextMeshProUGUI rewardTitleText;
        [SerializeField] private TextMeshProUGUI rewardAmountText;

        [Header("Day Blocks")]
        [SerializeField] private DailyBonusDayItem[] dayItems;

        [Header("Claim Button")]
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;
        [SerializeField] private GameObject claimButtonLoadingIndicator;

        [Header("Status Icons")]
        [SerializeField] private GameObject availableIndicator;
        [SerializeField] private GameObject claimedIndicator;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        [Header("Claim Popup")]
        [SerializeField] private GameObject claimConfirmationPopup;
        [SerializeField] private TextMeshProUGUI claimConfirmationText;

        void Awake()
        {
            Debug.Log("[DailyBonusScreen] Awake: name=" + gameObject.name
                      + ", activeSelf=" + gameObject.activeSelf
                      + ", activeInHierarchy=" + gameObject.activeInHierarchy
                      + ", parent=" + (transform.parent != null ? transform.parent.gameObject.name : "null"));
            if (controller == null) controller = FindObjectOfType<DailyBonusController>();
            if (controller == null)
            {
                var go = new GameObject("DailyBonusController");
                controller = go.AddComponent<DailyBonusController>();
            }

            controller.OnStatusUpdated += OnStatusUpdated;
            controller.OnBonusClaimed += OnBonusClaimed;
            controller.OnError += OnError;

            if (dayItems != null)
            {
                foreach (var item in dayItems)
                {
                    if (item != null)
                    {
                        item.OnDayClicked += OnAvailableDayClicked;
                    }
                }
            }
        }

        void OnEnable()
        {
            Debug.Log("[DailyBonusScreen] OnEnable: activeInHierarchy=" + gameObject.activeInHierarchy);
            RefreshUI();
        }

        void Start()
        {
            if (claimButton != null) claimButton.onClick.AddListener(OnClaimClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        void OnDestroy()
        {
            if (controller == null) return;
            controller.OnStatusUpdated -= OnStatusUpdated;
            controller.OnBonusClaimed -= OnBonusClaimed;
            controller.OnError -= OnError;

            if (dayItems != null)
            {
                foreach (var item in dayItems)
                {
                    if (item != null)
                    {
                        item.OnDayClicked -= OnAvailableDayClicked;
                    }
                }
            }
        }

        void RefreshUI()
        {
            if (controller == null)
            {
                Debug.LogWarning("[DailyBonusScreen] Controller is null.");
                return;
            }
            if (controller.GetCurrentStatus() == null)
            {
                Debug.Log("[DailyBonusScreen] Loading bonus status...");
                _ = controller.LoadBonusStatus();
            }
            else
            {
                Debug.Log("[DailyBonusScreen] Updating UI from cached status.");
                UpdateUI(controller.GetCurrentStatus(), controller.GetDayData());
            }
            UpdateClaimButtonState();
        }

        void UpdateUI(DailyBonusStatusResponse status, List<DailyBonusDayData> dayData)
        {
            if (status == null)
            {
                Debug.LogWarning("[DailyBonusScreen] Status is null in UpdateUI.");
                return;
            }

            // Debug.Log($"[DailyBonusScreen] UpdateUI: can_claim={status.can_claim}, current_cycle_day={status.current_cycle_day}, streak={status.current_streak}");

            UpdateClaimButtonState();
            UpdateDayItems(dayData);

            if (availableIndicator != null) availableIndicator.SetActive(status.can_claim);
            if (claimedIndicator != null) claimedIndicator.SetActive(!status.can_claim && status.current_streak > 0);
        }

        void UpdateDayItems(List<DailyBonusDayData> dayData)
        {
            if (dayData == null || dayItems == null)
            {
                Debug.LogWarning("[DailyBonusScreen] dayData or dayItems is null.");
                return;
            }

            int count = Mathf.Min(dayData.Count, dayItems.Length);
            // Debug.Log($"[DailyBonusScreen] Updating {count} day items.");

            for (int i = 0; i < count; i++)
            {
                if (dayItems[i] != null)
                {
                    dayItems[i].Setup(dayData[i]);
                }
            }
        }

        void UpdateClaimButtonState()
        {
            if (claimButton == null) return;

            bool canClaim = controller != null && controller.CanClaim();
            claimButton.interactable = canClaim;

            if (claimButtonText != null)
            {
                if (controller != null && controller.IsClaimingInProgress()) claimButtonText.text = "Claiming...";
                else if (canClaim) claimButtonText.text = "Claim Now!";
                else if (controller != null && controller.GetCurrentStatus() != null) claimButtonText.text = "Already Claimed";
                else claimButtonText.text = "Check Status";
            }

            if (claimButtonLoadingIndicator != null)
                claimButtonLoadingIndicator.SetActive(controller != null && controller.IsClaimingInProgress());
        }

        void OnStatusUpdated(DailyBonusStatusResponse status)
        {
            Debug.Log("[DailyBonusScreen] OnStatusUpdated fired.");
            UpdateUI(status, controller.GetDayData());
            UpdateClaimButtonState();
        }

        void OnBonusClaimed(DailyBonusClaimResponse response)
        {
            Debug.Log("[DailyBonusScreen] OnBonusClaimed fired.");
            UpdateClaimButtonState();
            ShowClaimConfirmation(response);
            HighlightClaimedDay();
        }

        void OnError(string error)
        {
            Debug.LogWarning($"[DailyBonusScreen] OnError: {error}");
            UpdateClaimButtonState();
        }

        async void OnClaimClicked()
        {
            Debug.Log("[DailyBonusScreen] Main claim button clicked.");
            if (controller != null && controller.CanClaim())
            {
                await controller.ClaimBonus();
            }
            else
            {
                Debug.Log("[DailyBonusScreen] Cannot claim via main button.");
            }
        }

        void OnCloseClicked() => gameObject.SetActive(false);

        void OnAvailableDayClicked(DailyBonusDayItem dayItem)
        {
            // Debug.Log($"[DailyBonusScreen] Day block clicked. item={dayItem?.DayNumber}, available={dayItem?.IsAvailable}, claimed={dayItem?.IsClaimed}");
            if (controller != null && controller.CanClaim() && dayItem != null && dayItem.IsAvailable)
            {
                // Debug.Log($"[DailyBonusScreen] Triggering claim from day {dayItem.DayNumber}.");
                _ = controller.ClaimBonus();
            }
            else
            {
                Debug.Log("[DailyBonusScreen] Block click ignored — not claimable.");
            }
        }

        void ShowClaimConfirmation(DailyBonusClaimResponse response)
        {
            // Debug.Log($"[DailyBonusScreen] ShowClaimConfirmation: response={(response != null ? "not null" : "null")}");
            if (response == null) return;

            if (claimConfirmationPopup != null)
            {
                claimConfirmationPopup.SetActive(true);
                Debug.Log("[DailyBonusScreen] Claim confirmation popup activated.");
            }
            else
            {
                Debug.LogWarning("[DailyBonusScreen] claimConfirmationPopup is not assigned.");
            }

            if (claimConfirmationText != null)
            {
                string amount = !string.IsNullOrEmpty(response.currency) && response.currency == "COINS"
                    ? $"{response.amount_paisa / 100} Coins"
                    : MoneyFormatter.FormatPaisa(response.amount_paisa, response.currency);
                claimConfirmationText.text = $"Claimed: {amount}";
                // Debug.Log($"[DailyBonusScreen] Confirmation text set: {claimConfirmationText.text}");
            }
        }

        void HighlightClaimedDay()
        {
            Debug.Log("[DailyBonusScreen] HighlightClaimedDay.");
            if (dayItems == null) return;
            foreach (var item in dayItems)
            {
                if (item != null && item.IsAvailable && !item.IsClaimed)
                {
                    item.PlayClaimAnimation();
                    Debug.Log("[DailyBonusScreen] Played claim animation on available day.");
                    break;
                }
            }
        }
    }
}
