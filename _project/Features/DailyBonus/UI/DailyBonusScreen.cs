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
        [Tooltip("Assign in inspector. Will FindObjectOfType then auto-create if missing.")]
        [SerializeField] private DailyBonusController controller;

        [Header("Day Grid")]
        [SerializeField] private Transform daysContainer;
        [SerializeField] private DailyBonusDayItem dayItemPrefab;

        [Header("Info Panel")]
        [SerializeField] private TextMeshProUGUI streakText;
        [SerializeField] private TextMeshProUGUI nextBonusText;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Claim Button")]
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonText;
        [SerializeField] private GameObject claimButtonLoadingIndicator;

        [Header("Status Icons")]
        [SerializeField] private GameObject availableIndicator;
        [SerializeField] private GameObject claimedIndicator;

        [Header("Close Button")]
        [SerializeField] private Button closeButton;

        readonly List<DailyBonusDayItem> dayItems = new List<DailyBonusDayItem>();

        void Awake()
        {
            if (controller == null) controller = FindObjectOfType<DailyBonusController>();
            if (controller == null)
            {
                var go = new GameObject("DailyBonusController");
                controller = go.AddComponent<DailyBonusController>();
            }

            controller.OnStatusUpdated += OnStatusUpdated;
            controller.OnBonusClaimed += OnBonusClaimed;
            controller.OnError += OnError;
        }

        void Start()
        {
            if (claimButton != null) claimButton.onClick.AddListener(OnClaimClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
            RefreshUI();
        }

        void OnEnable() => RefreshUI();

        void OnDestroy()
        {
            if (controller == null) return;
            controller.OnStatusUpdated -= OnStatusUpdated;
            controller.OnBonusClaimed -= OnBonusClaimed;
            controller.OnError -= OnError;
        }

        void RefreshUI()
        {
            if (controller == null) return;
            if (controller.GetCurrentStatus() == null) _ = controller.LoadBonusStatus();
            else UpdateUI(controller.GetCurrentStatus(), controller.GetDayData());
            UpdateClaimButtonState();
        }

        void UpdateUI(DailyBonusStatusResponse status, List<DailyBonusDayData> dayData)
        {
            if (status == null) return;

            if (streakText != null)
                streakText.text = $"Current Streak: {status.current_streak} Day{(status.current_streak != 1 ? "s" : "")}";

            if (nextBonusText != null)
                nextBonusText.text = status.can_claim
                    ? $"Next Bonus: {MoneyFormatter.FormatPaisa(status.bonus_amount_paisa)}"
                    : "";

            if (messageText != null) messageText.text = status.message;

            BuildDaysUI(dayData);

            if (availableIndicator != null) availableIndicator.SetActive(status.can_claim);
            if (claimedIndicator != null) claimedIndicator.SetActive(!status.can_claim && status.current_streak > 0);
        }

        void BuildDaysUI(List<DailyBonusDayData> dayData)
        {
            foreach (var item in dayItems) if (item != null) Destroy(item.gameObject);
            dayItems.Clear();

            if (dayData == null || daysContainer == null || dayItemPrefab == null) return;

            foreach (var data in dayData)
            {
                var item = Instantiate(dayItemPrefab, daysContainer);
                item.Setup(data);
                dayItems.Add(item);
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
            UpdateUI(status, controller.GetDayData());
            UpdateClaimButtonState();
        }

        void OnBonusClaimed(DailyBonusClaimResponse response)
        {
            UpdateClaimButtonState();
            HighlightClaimedDay();
        }

        void OnError(string error) => UpdateClaimButtonState();

        async void OnClaimClicked()
        {
            if (controller != null && controller.CanClaim()) await controller.ClaimBonus();
        }

        void OnCloseClicked() => gameObject.SetActive(false);

        void HighlightClaimedDay()
        {
            foreach (var item in dayItems)
            {
                if (item.IsAvailable && !item.IsClaimed)
                {
                    item.PlayClaimAnimation();
                    break;
                }
            }
        }
    }
}
