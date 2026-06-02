// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
// using UnityEngine.Networking;
// public class Level : MonoBehaviour
// {
//     public Image Banner;
//     public Image BannerSport;
//     public GameObject rowPrefab;  // Reference to the row prefab
//     public Transform tableContainer;
//     public GameObject rowPrefabRank;  // Reference to the row prefab
//     public Transform tableContainerRank;
//     public GameObject rowPrefabBonus;  // Reference to the row prefab
//     public Transform tableContainerBonus;
//     public GameObject rowPrefabSelfBonus;  // Reference to the row prefab
//     public GameObject rowPrefabDailyBonus;  // Reference to the row prefab
//     public Transform tableContainerSelfBonus;
//     public Transform tableContainerDailyBonus;

//     void Start()
//     {
//         // Example of initializing data
//         // string data = "[{\"name\":\"John Doe\",\"mobile\":1234567890,\"time\":\"12:30\",\"level\":\"1\"}]";
//         // levedata(data);
//     }

//     public void lavelcall()
//     {
//         Login leve = FindObjectOfType<Login>();
//         if (leve != null)
//         {
//             leve.LevelApi();
//         }
//         else
//         {
//             Debug.LogError("Login instance not found");
//         }
//     }

//     public void levedata(string data)
//     {
//         Debug.Log(data);
//         witharrayda[] witharray = JsonHelper.FromJson<witharrayda>(data);
//         if(witharray.Length == 0)
//         {
//             return;
//         }
//         if (witharray[0].name != null)
//         {
//             PopulateTable(witharray);

//         }
//         else
//         {
//             Debug.Log("jai");

//         }
//     }

//     [System.Serializable]
//     public class witharrayda
//     {
//         public string name;
//         public string username;
//         public string time;
//         public string error;
//         public string mobile;
//         public int level;
//     }

//     void PopulateTable(witharrayda[] data)
//     {
//         for (int i = 0; i < tableContainer.transform.childCount; i++)
//         {
//             Destroy(tableContainer.transform.GetChild(i).gameObject);
//         }
//         foreach (witharrayda rowData in data)
//         {
//             // Instantiate the prefab
//             if (rowPrefab != null && tableContainer != null)
//             {
//                 GameObject newRow = Instantiate(rowPrefab, tableContainer);

//                 // Find the Text component in the new row and set the text
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();

//                 if (textComponents.Length >= 4)
//                 {
//                     textComponents[0].text = rowData.mobile.ToString();
//                     textComponents[1].text = rowData.username.ToString();
//                     textComponents[2].text = rowData.level.ToString();
//                     textComponents[3].text = rowData.time;
//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("rowPrefab or tableContainer is not assigned");
//             }
//         }
//     }



//     public void levelrank()
//     {
//         Login leve = FindObjectOfType<Login>();
//         leve.levelRankapi();
//     }

//     public void levelrankAPI(string Data)
//     {

//         Debug.Log(Data);
//         rankarray[] witharrayRank = JsonHelper.FromJson<rankarray>(Data);
//         if (Data != null)
//         {
//             PopulateTableRank(witharrayRank);
//         }
//         else
//         {
//             Debug.Log("jai");

//         }
//     }

//     [System.Serializable]
//     public class rankarray
//     {
//         public string id;
//         public string amount;
//         public string time;
//         public string error;
//     }

//     void PopulateTableRank(rankarray[] data)
//     {
//         for (int i = 0; i < tableContainerRank.transform.childCount; i++)
//         {
//             Destroy(tableContainerRank.transform.GetChild(i).gameObject);
//         }
//         foreach (rankarray rowData in data)
//         {
//             // Instantiate the prefab
//             if (rowPrefabRank != null && tableContainerRank != null)
//             {
//                 GameObject newRow = Instantiate(rowPrefabRank, tableContainerRank);

//                 // Find the Text component in the new row and set the text
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();

//                 if (textComponents.Length >= 3)
//                 {
//                     textComponents[0].text = rowData.id;
//                     textComponents[1].text = "rs." + rowData.amount;
//                     textComponents[2].text = rowData.time;

//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("rowPrefabRank or tableContainerRank is not assigned");
//             }
//         }
//     }



//     public void mybonus()
//     {
//         Login leve = FindObjectOfType<Login>();
//         leve.mybonusapi();
//     }
//     public void MyBonsuResult(string data)
//     {
//         Debug.Log("data : " + data);

//         BonsuResult[] witharrayRank = JsonHelper.FromJson<BonsuResult>(data);
//         if (witharrayRank[0].sendermobile != null)
//         {
//             MyBonsuResultData(witharrayRank);
//         }
//         else
//         {
//             Debug.Log("jai");

//         }
//     }
//     [System.Serializable]
//     public class BonsuResult
//     {
//         public string sendermobile;
//         public float recharge;
//         public float bonus;
//         public string time;
//         public string error;
//         public string rank;

//     }
//     void MyBonsuResultData(BonsuResult[] data)
//     {
//         for (int i = 0; i < tableContainerBonus.transform.childCount; i++)
//         {
//             Destroy(tableContainerBonus.transform.GetChild(i).gameObject);
//         }
//         foreach (BonsuResult rowData in data)
//         {

//             if (rowPrefabBonus != null && tableContainerBonus != null)
//             {
//                 GameObject newRow = Instantiate(rowPrefabBonus, tableContainerBonus);
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();
//                 if (textComponents.Length >= 4)
//                 {
//                     textComponents[0].text = rowData.sendermobile;
//                     textComponents[1].text = rowData.bonus.ToString();
//                     textComponents[2].text = rowData.rank.ToString();
//                     textComponents[3].text = rowData.recharge.ToString();
//                     textComponents[4].text = rowData.time;
//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("rowPrefabBonus or tableContainerBonus is not assigned");
//             }
//         }
//     }

//     public void SelfBonus()
//     {
//         Login slefbon = FindObjectOfType<Login>();
//         slefbon.selfbonusapi();
//     }
//     public void SelfBounsResult(string data)
//     {
//         ReferalBonus[] SelfBonusDate = JsonHelper.FromJson<ReferalBonus>(data);
//         ReferalBonusShow(SelfBonusDate);

//     }
//     public void DailyBonus()
//     {
//         Login slefbon = FindObjectOfType<Login>();
//         slefbon.dailybonusapi();
//     }
//     public void DailyBounsResult(string data)
//     {
//         DailyBonusData[] SelfBonusDate = JsonHelper.FromJson<DailyBonusData>(data);
//                     DailyBonusShow(SelfBonusDate);

//     }

//     [System.Serializable]
//     public class DailyBonusData
//     {
//         public string id;
//         public int day;
//         public string amount;
//         public string time;
//         public string error;
//     }

//     [System.Serializable]
//     public class ReferalBonus
//     {
//         public int status;
//         public string id;
//         public string referid;
//         public string refer_mo;
//         public float amount;
//         public string time;
//         public string error;
//     }


//     [System.Serializable]
//     public class selfBOs
//     {
//         public int status;
//         public string name;
//         public float amount;
//         public string time;
//         public string error;
//     }

//     void DailyBonusShow(DailyBonusData[] data)
//     {
//         for (int i = 0; i < tableContainerDailyBonus.transform.childCount; i++)
//         {
//             Destroy(tableContainerDailyBonus.transform.GetChild(i).gameObject);
//         }
//         foreach (DailyBonusData rowData in data)
//         {
//             if (rowPrefabDailyBonus != null && tableContainerDailyBonus != null)
//             {
//                 GameObject newRow = Instantiate(rowPrefabDailyBonus, tableContainerDailyBonus);
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();
//                 if (textComponents.Length >= 3)
//                 {
//                     textComponents[0].text = rowData.id;
//                     textComponents[1].text = rowData.amount.ToString();
//                     textComponents[2].text = rowData.time;
//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("rowPrefabBonus or tableContainerSelfBonus is not assigned");
//             }
//         }
//     }


//     void ReferalBonusShow(ReferalBonus[] data)
//     {
//         for (int i = 0; i < tableContainerSelfBonus.transform.childCount; i++)
//         {
//             Destroy(tableContainerSelfBonus.transform.GetChild(i).gameObject);
//         }
//         foreach (ReferalBonus rowData in data)
//         {
//             if (rowPrefabSelfBonus != null && tableContainerSelfBonus != null)
//             {
//                 GameObject newRow = Instantiate(rowPrefabSelfBonus, tableContainerSelfBonus);
//                 TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();
//                 if (textComponents.Length >= 3)
//                 {
//                     textComponents[0].text = rowData.id;
//                     textComponents[1].text = rowData.referid;
//                     textComponents[2].text = rowData.refer_mo;
//                     textComponents[3].text = rowData.amount.ToString();
//                     textComponents[4].text = rowData.time;
//                 }
//                 else
//                 {
//                     Debug.LogError("Not enough TextMeshProUGUI components in the row prefab");
//                 }
//             }
//             else
//             {
//                 Debug.LogError("rowPrefabBonus or tableContainerSelfBonus is not assigned");
//             }
//         }
//     }


//     public void OpenImage()
//     {
//         string refer_image = UserDetail.ReferImage;
//         string imageUrl = ServerConfig.mainUrl + refer_image;
//         StartCoroutine(DownloadImage(imageUrl));
//     }

//     IEnumerator DownloadImage(string imageUrl)
//     {
//         UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
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
//     public void OpenImageSport()
//     {
//         string sport_image = UserDetail.SportImage;
//         string imageUrl = ServerConfig.mainUrl + sport_image;
//         Debug.Log(imageUrl);
//         StartCoroutine(DownloadImageSport(imageUrl));
//     }

//     IEnumerator DownloadImageSport(string imageUrl)
//     {
//         UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
//         yield return www.SendWebRequest();

//         if (www.result != UnityWebRequest.Result.Success)
//         {
//             Debug.Log(www.error);
//         }
//         else
//         {
//             Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
//             BannerSport.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
//         }
//     }
// }
