// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using Core.Bootstrap;
// using Features.DailyBonus.Controllers;
// using Features.DailyBonus.Models;
// using Core.Managers;
// using Core.Utils;
// using System.Threading.Tasks;

// public class DailyBonus : MonoBehaviour
// {
//     public Sprite claimedSpr;
//     public Sprite lockedSpr;
//     Image[] status = new Image[7];
//     public GameObject[] obstacleImg;
//     public Sprite GreenClaim;
//     public Sprite YelloClaim;
//     public Sprite DarkClaim;
//     public GameObject claimedPopUp;

//     DailyBonusController bonusController;
//     void Awake()
//     {
//         bonusController = FindObjectOfType<DailyBonusController>();
//     }

//     async void Start()
//     {
//         for (int i = 0; i < obstacleImg.Length; i++)
//         {
//             if (obstacleImg[i] != null)
//                 status[i] = obstacleImg[i].transform.GetChild(0).GetComponent<Image>();
//         }
//         claimedPopUp.SetActive(false);
//         if (bonusController == null)
//         {
//             bonusController = FindObjectOfType<DailyBonusController>();
//         }
//         if (bonusController != null)
//         {
//             bonusController.OnStatusUpdated += OnBonusStatusUpdated;
//             bonusController.OnError += OnBonusError;
//             await bonusController.LoadBonusStatus();
//         }
//         else
//         {
//             Debug.LogWarning("DailyBonusController not found in scene.");
//         }
//     }

//     void OnDestroy()
//     {
//         if (bonusController != null)
//         {
//             bonusController.OnStatusUpdated -= OnBonusStatusUpdated;
//             bonusController.OnError -= OnBonusError;
//         }
//     }

//     void OnBonusStatusUpdated(DailyBonusStatusResponse status)
//     {
//         if (status == null) return;
//         RenderBonusGrid(status);
//         UpdateIndicatorText(status);
//     }

//     void OnBonusError(string error)
//     {
//         Debug.LogWarning($"Daily bonus lobby error: {error}");
//     }

//     void UpdateIndicatorText(DailyBonusStatusResponse status)
//     {
//     }

//     void RenderBonusGrid(DailyBonusStatusResponse bonusStatus)
//     {
//         if (bonusStatus.cycle_days_paisa == null || bonusStatus.cycle_days_paisa.Count == 0) return;
//         var claimed = new HashSet<int>(bonusStatus.claimed_cycle_days ?? new List<int>());
//         int count = Mathf.Min(bonusStatus.cycle_days_paisa.Count, 7);

//         for (int i = 0; i < count; i++)
//         {
//             int day = i + 1;
//             bool isClaimed = claimed.Contains(day);
//             bool isToday = day == bonusStatus.current_cycle_day;

//             if (obstacleImg != null && i < obstacleImg.Length && obstacleImg[i] != null)
//             {
//                 obstacleImg[i].SetActive(isClaimed);
//             }
//             if (i < status.Length && status[i] != null)
//             {
//                 status[i].sprite = isClaimed
//                     ? claimedSpr
//                     : lockedSpr;
//             }
//         }
//     }

//     public void OnBonusButtonClicked()
//     {
//         if (bonusController != null && bonusController.CanClaim())
//         {
//             _ = bonusController.ClaimBonus();
//         }
//     }

//     public void DeactivatePopUP()
//     {
//         claimedPopUp.SetActive(false);
//     }
// }
