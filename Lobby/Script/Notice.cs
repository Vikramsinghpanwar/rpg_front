// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Networking;
// using TMPro;
// public class Notice : MonoBehaviour
// {
//     public Image Banner;
//     public GameObject BannerPrefab;
//     private int noticenumber;
//     private string AllBanner;
//     public Sprite ChageBtn;
//     public Sprite BGRemoveBtn;
//     private string AvitceBtn;

//     public GameObject itemPrefab; // Corrected the typo from 'itemperfab' to 'itemPrefab'
//     public GameObject container;
//     public Sprite TranBtn;
//     public Sprite ColorBtn;

//     public void AllBannerName(string AllBanner)
//     {
//         this.AllBanner = AllBanner;
//         NameEditApiData(AllBanner);
//         NameEdit(AllBanner);
//     }

//     void Start()
//     {
//         noticenumber = 0;
//         AvitceBtn = "Notice-btn-1";
//         Invoke("DelayedFunctionCall", 1f);
//     }


//  void NameEditApiData(string jsonResponse)
//     {
//         EditNotice[] nameEditData = JsonHelper.FromJson<EditNotice>(jsonResponse);
//         if (nameEditData.Length != null)
//         {
//             string name = nameEditData[0].name;
//             string image = nameEditData[0].image;
//             string imageUrl = ServerConfig.mainUrl + nameEditData[noticenumber].image;
//             StartCoroutine(DownloadImage(imageUrl));
//         }
//         else
//         {
//             Debug.LogError("Invalid response format or empty response");
//         }
//     }
//     public void NameEdit(string data)
//     {
//         EditNotice[] nameEditData = JsonHelper.FromJson<EditNotice>(data);
//         for (int i = 0; i < nameEditData.Length; i++)
//         {
//             nameChage(nameEditData[i].name, i);
//         }
//     }

//     private void nameChage(string name, int number)
//     {
//         float templateHeight = 100f;
//         GameObject entryObject = Instantiate(itemPrefab, container.transform);
//         RectTransform entryRectTransform = entryObject.GetComponent<RectTransform>();
//         // Set the anchored position of the RectTransform
//         entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * 20);
//         TextMeshProUGUI[] textTyping = entryObject.GetComponentsInChildren<TextMeshProUGUI>();
//         textTyping[0].text = name;
//         textTyping[1].text = number.ToString();
//     }



//     public void ChageNotice(int notice)
//     {
//         Debug.Log("notice num : " + notice);
//         noticenumber = notice;
//         Debug.Log(AllBanner);
//         NameEditApiData(AllBanner);
//         ChangeBtn(notice);
//     }


//     void DelayedFunctionCall()
//     {
//         ChangeBtn(0);
//     }


//     public void OpenCompanySet()
//     {
//         string url = "https://maxwayinfotech.com";
//         Application.OpenURL(url);
//     }
//     public void ChangeButton(string NewBtn)
//     {
//         GameObject OldButton = GameObject.Find(AvitceBtn);
//         Image OldBtn = OldButton.GetComponent<Image>();
//         OldBtn.sprite = BGRemoveBtn;

//         GameObject NewButton = GameObject.Find(NewBtn);
//         Image NewButtonBtn = NewButton.GetComponent<Image>();
//         NewButtonBtn.sprite = ChageBtn;
//         AvitceBtn = NewBtn;
//     }
//     IEnumerator DownloadImage(string url)
//     {
//         UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
//         yield return www.SendWebRequest();

//         if (www.result != UnityWebRequest.Result.Success)
//         {
//             Debug.Log(www.error);
//         }
//         else
//         {
//             Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
//             Banner.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
//         }
//     }





//     public void ChangeBtn(int val)
//     {
//         for (int i = 0; i < container.transform.childCount; i++)
//         {
//             container.transform.GetChild(i).gameObject.GetComponent<Image>().sprite = TranBtn;
//         }
//         container.transform.GetChild(val).gameObject.GetComponent<Image>().sprite = ColorBtn;
//     }

//     [System.Serializable]
//     public class EditNotice
//     {
//         public string name;
//         public string image;
//     }



// }


