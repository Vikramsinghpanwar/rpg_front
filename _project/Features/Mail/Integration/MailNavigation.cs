// using System;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using Features.Mail.Controllers;
// using Features.Mail.UI;
// using Core.Bootstrap;

// namespace Features.Mail.Integration
// {
//     public class MailNavigation : MonoBehaviour
//     {
//         [Header("Navigation Button")]
//         [SerializeField] private Button openMailButton;
//         [SerializeField] private GameObject unreadBadgeRoot;
//         [SerializeField] private TextMeshProUGUI unreadBadgeText;

//         [Header("Mail Inbox Screen")]
//         [SerializeField] private MailInboxScreen mailInboxScreenPrefab;
//         [SerializeField] private Transform screenParent;

//         [Header("Controllers")]
//         [SerializeField] private MailController mailController;

//         GameObject currentScreenInstance;
//         MailInboxScreen currentScreenUI;
//         int lastKnownUnreadCount = -1;

//         void Start()
//         {
//             if (openMailButton != null)
//             {
//                 openMailButton.onClick.AddListener(OpenMailInboxScreen);
//             }

//             UpdateUnreadBadge(0);
//         }

//         void UpdateUnreadBadge(int count)
//         {
//             lastKnownUnreadCount = count;

//             if (unreadBadgeRoot != null)
//             {
//                 unreadBadgeRoot.SetActive(count > 0);
//             }

//             if (unreadBadgeText != null)
//             {
//                 if (count > 99)
//                 {
//                     unreadBadgeText.text = "99+";
//                 }
//                 else if (count > 0)
//                 {
//                     unreadBadgeText.text = count.ToString();
//                 }
//                 else
//                 {
//                     unreadBadgeText.text = string.Empty;
//                 }
//             }
//         }

//         public async void OpenMailInboxScreen()
//         {
//             if (currentScreenInstance != null)
//             {
//                 CloseMailInboxScreen();
//                 return;
//             }

//             if (mailInboxScreenPrefab == null) return;

//             var component = Instantiate(mailInboxScreenPrefab, screenParent != null ? screenParent : transform);
//             currentScreenInstance = component.gameObject;
//             currentScreenUI = component;

//             if (currentScreenUI != null && mailController != null)
//             {
//                 if (mailController.inboxScreenUI == null)
//                 {
//                     mailController.inboxScreenUI = currentScreenUI;
//                 }

//                 currentScreenUI.ShowLoading();

//                 if (!mailController.HasMails)
//                 {
//                     if (BootstrapService.Instance != null && !BootstrapService.Instance.HasData)
//                     {
//                         await BootstrapService.Instance.Refresh();
//                     }

//                     await mailController.LoadInbox(true);
//                 }
//                 else
//                 {
//                     currentScreenUI.SetMailList(mailController.AllMails);
//                     currentScreenUI.UpdateLoadMoreState(mailController.CanLoadMore());
//                 }

//                 currentScreenUI.HideLoading();
//             }

//             var closeBtn = FindCloseButton(currentScreenInstance);
//             if (closeBtn != null)
//             {
//                 closeBtn.onClick.AddListener(CloseMailInboxScreen);
//             }
//         }

//         static Button FindCloseButton(GameObject root)
//         {
//             var buttons = root.GetComponentsInChildren<Button>(true);
//             foreach (var b in buttons)
//             {
//                 if (b != null && b.name != null && b.name.ToLower().Contains("close"))
//                 {
//                     return b;
//                 }
//             }

//             return null;
//         }

//         public void CloseMailInboxScreen()
//         {
//             if (currentScreenInstance == null) return;

//             Destroy(currentScreenInstance);
//             currentScreenInstance = null;
//             currentScreenUI = null;

//             if (mailController != null)
//             {
//                 _ = RefreshUnreadBadge();
//             }
//         }

//         private async Task RefreshUnreadBadge()
//         {
//             if (mailController != null)
//             {
//                 int count = await mailController.LoadUnreadCount();
//                 UpdateUnreadBadge(count);
//             }
//         }

//         private void OnDestroy()
//         {
//             if (openMailButton != null)
//             {
//                 openMailButton.onClick.RemoveListener(OpenMailInboxScreen);
//             }

//             CloseMailInboxScreen();
//         }
//     }
// }
