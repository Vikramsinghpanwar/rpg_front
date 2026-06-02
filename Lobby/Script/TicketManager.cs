// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Networking;
// using UnityEngine.UI;

// public class TicketManager : MonoBehaviour
// {

//     public Button myTicketsBtn, generateTicketBtn;
//     public Transform myTicketsCon;
//     public GameObject myTicketsPanel, generateTicketPanel;
//     public Sprite unselected, selected;
//     public GameObject ticketPrefab;
//     public TMP_InputField descriptionTMP;
//     public TMP_Dropdown categoryDD;
//     public Image proofIMG;

//     void Start()
//     {
//         myTicketsBtn.onClick.AddListener(()=>MyTickets());
//         generateTicketBtn.onClick.AddListener(()=>GenerateTicket());
//         MyTickets();
//     }

//     public void FetchMyTickets()
//     {
//                 StartCoroutine(FetchMyTicketCoroutine());

//     }

//     void MyTickets()
//     {
//         myTicketsPanel.SetActive(true);
//         generateTicketPanel.SetActive(false);
//         myTicketsBtn.GetComponent<Image>().sprite = selected;
//         generateTicketBtn.GetComponent<Image>().sprite = unselected;
//     }
//     void GenerateTicket()
//     {
//         myTicketsPanel.SetActive(false);
//         generateTicketPanel.SetActive(true);
//         myTicketsBtn.GetComponent<Image>().sprite = unselected;
//         generateTicketBtn.GetComponent<Image>().sprite = selected;
//     }


//     public void AddImg()
//     {
        
//     }

//     public void SubmitTicket()
//     {
//         StartCoroutine(GenerateTicket(categoryDD.options[categoryDD.value].text, descriptionTMP.text, SpriteToTexture2D(proofIMG.sprite)));
//     }







//     // image upload

//     public static Texture2D SpriteToTexture2D(Sprite sprite)
// {
//     if (sprite == null) return null;

//     // Case 1: sprite already uses full texture
//     if (sprite.rect.width == sprite.texture.width &&
//         sprite.rect.height == sprite.texture.height)
//     {
//         return sprite.texture;
//     }

//     // Case 2: sprite is a sub-rect (atlas, trimmed)
//     Texture2D newTex = new Texture2D(
//         (int)sprite.rect.width,
//         (int)sprite.rect.height,
//         TextureFormat.RGBA32,
//         false
//     );

//     Color[] pixels = sprite.texture.GetPixels(
//         (int)sprite.textureRect.x,
//         (int)sprite.textureRect.y,
//         (int)sprite.textureRect.width,
//         (int)sprite.textureRect.height
//     );

//     newTex.SetPixels(pixels);
//     newTex.Apply();

//     return newTex;
// }


//     public void SelectPicture()
//      {
//          // Check if permission is granted
//          if (NativeGallery.IsMediaPickerBusy())
//              return;
//          NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
//          {
//              if (path != null)
//              {
//                  // Load image from the selected path
//                  StartCoroutine(LoadImage(path));
//              }
//          }, "Select a profile picture");

//          if (permission == NativeGallery.Permission.Denied)
//          {
//              Debug.Log("Gallery access denied");
//          }
//      }

//      // Coroutine to load the image from the path
//      private IEnumerator LoadImage(string path)
//      {
//          // Load the image as a texture
//          Texture2D texture = NativeGallery.LoadImageAtPath(path, 512, false); // 512 = maxSize, false = isReadable

//          if (texture == null)
//          {
//              Debug.Log("Couldn't load texture from path: " + path);
//              yield break;
//          }

//          // Check if texture is readable
//          if (!texture.isReadable)
//          {
//              Debug.LogError("Texture is not readable!");
//              yield break;
//          }

//          // Convert Texture2D to Sprite and display it in the UI
//          Rect rect = new Rect(0, 0, texture.width, texture.height);
//          Sprite newProfileSprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
//          proofIMG.sprite = newProfileSprite;
//          uploadImg.SetActive(false);
//      }

// public GameObject uploadImg;

//      //APISSSSSs




//     IEnumerator GenerateTicket(string type, string description, Texture2D image)
//     {
//         string url = ServerConfig.mainUrl + "/api/create_ticket.php";
//         WWWForm form = new WWWForm();
//         form.AddField("token", UserDetail.Token);
//         form.AddField("subject", type);
//         form.AddField("message", description);
//         if (image != null)
//         {
//             byte[] imageBytes = image.EncodeToPNG(); // or EncodeToJPG()
//             form.AddBinaryData(
//                 "image",                // field name (must match backend)
//                 imageBytes,
//                 "ticket.png",            // filename
//                 "image/png"              // mime type
//             );
//         }
//         using (UnityWebRequest www = UnityWebRequest.Post(url, form))
//         {
//             yield return www.SendWebRequest();

//             if (www.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("Network error: " + www.error);
//             }
//             else
//             {
//                 if (www.responseCode == 200)
//                 {
//                     string jsonResponse = www.downloadHandler.text;
//                     Debug.Log("Response: " + jsonResponse);
//                     StartCoroutine(FetchMyTicketCoroutine());
//                     Logger.Instance.Log("Ticket created successfully.");
//                     uploadImg.SetActive(true);
//                     descriptionTMP.text = ""; 
//                     proofIMG.sprite = null;
//                 }
//                 else
//                 {
//                     Debug.Log("Login failed: " + www.downloadHandler.text);
//                 }
//             }
//         }

//     }

//     [System.Serializable]
//     public class MyTicketRequest
//     {
//         public string token;
//     }


//     IEnumerator FetchMyTicketCoroutine()
//     {
//         string url = ServerConfig.mainUrl + "/api/my_tickets.php";

//         MyTicketRequest payload = new MyTicketRequest
//         {
//             token = UserDetail.Token
//         };

//         string json = JsonUtility.ToJson(payload);
//         byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

//         UnityWebRequest www = new UnityWebRequest(url, "POST");
//         www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//         www.downloadHandler = new DownloadHandlerBuffer();

//         www.SetRequestHeader("Content-Type", "application/json");

//         yield return www.SendWebRequest();

//         if (www.result == UnityWebRequest.Result.Success)
//         {
//             Debug.Log("myTickets: " + www.downloadHandler.text);
//             PopulateTickets(www.downloadHandler.text);
//         }
//         else
//         {
//             Debug.LogError("Error: " + www.error);
//         }
//     }


//     [System.Serializable]
// public class TicketResponse
// {
//     public bool status;
//     public TicketData[] data;
// }


// public void PopulateTickets(string json)
// {
//     TicketResponse response =
//         JsonUtility.FromJson<TicketResponse>(json);

//     if (response == null || response.data == null)
//     {
//         Debug.LogError("Invalid ticket data");
//         return;
//     }

//     // Clear old tickets
//     foreach (Transform child in myTicketsCon)
//     {
//         Destroy(child.gameObject);
//     }

//     // Create new ones
//     foreach (TicketData ticket in response.data)
//     {
//         GameObject go = Instantiate(ticketPrefab, myTicketsCon);
//         TicketItemUI ui = go.GetComponent<TicketItemUI>();
//         go.GetComponent<Button>().onClick.AddListener(()=>OpenMessageBox(ticket));

//         if (ui == null)
//         {
//             Debug.LogError("TicketPrefab missing TicketItemUI script");
//             continue;
//         }

//         ui.SetData(ticket);
//     }
// }

// public GameObject msgBox;
//     public TMP_Text idText;
//     public TMP_Text subjectText;
//     public TMP_Text messageText;
//     public TMP_Text statusText;
//     public TMP_Text createdAtText;
//     public TMP_Text replyText;


// void OpenMessageBox(TicketData data)
//     {
//         // idText.text = data.id.ToString();
//         subjectText.text = data.subject;
//         string s = data.message;            
//         messageText.text = s;
//                 switch (data.status)
//         {
//             case 0:
//                 statusText.text = "Status: Open";
//                 statusText.color = Color.yellow;
//                 break;
//             case 1:
//                 statusText.text = "Status: Closed";
//                 statusText.color = Color.magenta;
//                 break;
//             case 2:
//                 statusText.text = "Status: Resolved";
//                 statusText.color = Color.green;
//                 break;
//             case 3:
//                 statusText.text = "Status: In Progress";
//                 statusText.color = Color.yellow;
//                 break;
//             default:
//                 statusText.text = "Status: " + (data.status == 0 ? "Open" : "Closed");
//                 break;
//         }

//         createdAtText.text = data.created_at;
//         replyText.text = data.adminmessage;
//        msgBox.SetActive(true); 
//     }


    

// }

// [System.Serializable]
// public class TicketData
// {
//     public int id;
//     public string subject;
//     public string message;
//     public string image;
//     public string adminmessage;
//     public int status;
//     public string created_at;
// }

