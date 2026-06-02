using UnityEngine;
using UnityEngine.UI;
using Features.Announcement.Controllers;
using Features.Announcement.UI;
using Core.Bootstrap;

namespace Features.Announcement.Integration
{
    // Drop-in component for a "Announcements" button anywhere in the UI. On click it spawns
    // the screen prefab, wires the controller, and pulls announcements from BootstrapService
    // (with optional public-endpoint fallback handled inside AnnouncementController).
    public class AnnouncementNavigation : MonoBehaviour
    {
        [Header("Navigation Button")]
        [SerializeField] private Button openAnnouncementButton;

        [Header("Announcement Screen")]
        [SerializeField] private GameObject announcementScreenPrefab;
        [SerializeField] private Transform screenParent;

        [Header("Controllers")]
        [SerializeField] private AnnouncementController announcementController;

        GameObject currentScreenInstance;
        AnnouncementScreenUI currentScreenUI;

        void Start()
        {
            if (openAnnouncementButton != null)
                openAnnouncementButton.onClick.AddListener(OpenAnnouncementScreen);
        }

        public async void OpenAnnouncementScreen()
        {
            if (currentScreenInstance != null) { CloseAnnouncementScreen(); return; }
            if (announcementScreenPrefab == null) return;

            currentScreenInstance = Instantiate(announcementScreenPrefab, screenParent != null ? screenParent : transform);
            currentScreenUI = currentScreenInstance.GetComponent<AnnouncementScreenUI>();

            if (currentScreenUI != null && announcementController != null)
            {
                currentScreenUI.SetOnAnnouncementSelected(announcementController.SelectAnnouncementById);
                currentScreenUI.ShowLoading();

                if (!announcementController.HasAnnouncements)
                {
                    // Make sure bootstrap has at least been attempted, then map the lobby slice.
                    if (BootstrapService.Instance != null && !BootstrapService.Instance.HasData)
                    {
                        await BootstrapService.Instance.Refresh();
                    }
                    await announcementController.InitializeFromBootstrap();
                }

                currentScreenUI.HideLoading();
            }

            var closeBtn = FindCloseButton(currentScreenInstance);
            if (closeBtn != null) closeBtn.onClick.AddListener(CloseAnnouncementScreen);
        }

        static Button FindCloseButton(GameObject root)
        {
            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b != null && b.name != null && b.name.ToLower().Contains("close")) return b;
            }
            return null;
        }

        public void CloseAnnouncementScreen()
        {
            if (currentScreenInstance == null) return;
            Destroy(currentScreenInstance);
            currentScreenInstance = null;
            currentScreenUI = null;
        }

        void OnDestroy()
        {
            if (openAnnouncementButton != null)
                openAnnouncementButton.onClick.RemoveListener(OpenAnnouncementScreen);
            CloseAnnouncementScreen();
        }
    }
}
