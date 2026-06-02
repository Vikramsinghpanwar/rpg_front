using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class TableData
{
    public int tableId;
    public float entryFee;
    public float commission;
    public float prizePool;
    public float onlinePlayers;
}

[System.Serializable]
public class TablesResponse
{
    public List<TableData> tables;
}

public class TableListPopulator : MonoBehaviour
{
    public GameObject tablePrefab;
    public GameObject privateTable;
    public Transform contentParent;
    public Transform contentParent_PT2;
    public Transform contentParent_PT4;

    void Start()
    {
        StartCoroutine(FetchTableData());
    }

    IEnumerator FetchTableData()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(Const.Server_Url + "api/admin/tableData"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                TablesResponse data = JsonUtility.FromJson<TablesResponse>(request.downloadHandler.text);
                Debug.Log("table data : " + request.downloadHandler.text);

                foreach (var table in data.tables)
                {
                    int eFee = (int)table.entryFee;
                    GameObject prefab = Instantiate(tablePrefab, contentParent);
                    GameObject prefab_PT2 = Instantiate(privateTable, contentParent_PT2);
                    GameObject prefab_PT4 = Instantiate(privateTable, contentParent_PT4);
                    prefab.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"Online Player: {Random.Range(10, 40)}";
                    prefab.transform.GetChild(2).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"₹{table.entryFee}";
                    prefab_PT2.transform.GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"₹{table.entryFee}";
                    prefab_PT4.transform.GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"₹{table.entryFee}";
                    prefab.transform.GetChild(1).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"₹{table.prizePool}";
                    prefab_PT2.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"₹{table.prizePool}";
                    prefab_PT4.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = $"₹{2 * table.prizePool}";
                    prefab.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => LobbyUI.instance.OnStartMatchmakingClicked(eFee));
                    prefab_PT2.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => LobbyUI.instance.OnPrivateRoomPlayer(eFee, 2));
                    prefab_PT4.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => LobbyUI.instance.OnPrivateRoomPlayer(eFee, 4));
                }
            }
            else
            {
                Debug.LogError("Failed to fetch data: " + request.error);
            }
        }
    }
}
