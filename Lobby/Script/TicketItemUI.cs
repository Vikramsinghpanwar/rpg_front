// using TMPro;
// using Unity.Mathematics;
// using UnityEngine;

// public class TicketItemUI : MonoBehaviour
// {
//     public TMP_Text idText;
//     public TMP_Text subjectText;
//     public TMP_Text messageText;
//     public TMP_Text statusText;
//     public TMP_Text createdAtText;

//     public void SetData(TicketData data)
//     {
//         idText.text = data.id.ToString();
//         subjectText.text = data.subject;
//         string s = data.message;
//         if (data.message.Length > 20)
//         {
//             s = data.message.Substring(0, 20  - 3) + "...";            
//         }
                
//         messageText.text = s;
//         switch (data.status)
//         {
//             case 0:
//                 statusText.text = "Open";
//                 statusText.color = Color.yellow;
//                 break;
//             case 1:
//                 statusText.text = "Closed";
//                 statusText.color = Color.magenta;
//                 break;
//             case 2:
//                 statusText.text = "Resolved";
//                 statusText.color = Color.green;
//                 break;
//             case 3:
//                 statusText.text = "In Progress";
//                 statusText.color = Color.yellow;
//                 break;
//             default:
//                 statusText.text = "Status: " + (data.status == 0 ? "Open" : "Closed");
//                 break;
//         }
//         createdAtText.text = data.created_at;
//     }
// }
