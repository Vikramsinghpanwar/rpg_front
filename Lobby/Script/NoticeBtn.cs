// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class NoticeBtn : MonoBehaviour
// {
//     public PopupOpen popupRef;
//     private void Start()
//     {
//         popupRef = FindObjectOfType<PopupOpen>();
//     }
//     public void Change(TextMeshProUGUI number)
//     {
//         popupRef.TuSound();
//         // Find the Notice component in the scene
//         Notice notice = FindObjectOfType<Notice>();

//         // Extract the number from the TextMeshProUGUI component
//         int numberValue;
//         bool parseSuccess = int.TryParse(number.text, out numberValue);

//         if (parseSuccess)
//         {
//             // Call the method in the Notice component to change the notice
//             notice.ChageNotice(numberValue);
//         }
//         else
//         {
//             Debug.LogError("Failed to parse number from TextMeshProUGUI.");
//         }


//     }
// }
