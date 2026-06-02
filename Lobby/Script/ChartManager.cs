using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Lobby.Integration;
public class ChartManager : MonoBehaviour
{

    public BootValueDecider bootminEntryRef;
    public List<ChartDataObj> chartDataList;
    public Sprite addCash;
    public Sprite playNow;
    public List<TextMeshProUGUI> onlinePlayerList;
    public List<int> onlinePlayerCountList;
    // Start is called before the first frame update

    private void OnEnable()
    {
        RandomOnlinePlayers();
        for (int i = 0; i < chartDataList.Count; i++)
        {
            if (chartDataList[i].minEntry <= BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f)
            {
                chartDataList[i].img.sprite = playNow;

            }
            else
            {
                chartDataList[i].img.sprite = addCash;
            }
        }
    }
    private void Start()
    {

        bootminEntryRef = FindObjectOfType<BootValueDecider>();

    }
    public void OpenTeenpatti(int btnIndex)
    {
        if (chartDataList[btnIndex].img.sprite == playNow)
        {
            GameMode.tableEntryFee = chartDataList[btnIndex].bootVal;
            bootminEntryRef.OpenTeenpatti(chartDataList[btnIndex].bootVal);
        }
        else
        {
            bootminEntryRef.addCashPanel.SetActive(true);
        }
    }



    public void OpenRummy(int btnIndex)
    {
        if (chartDataList[btnIndex].img.sprite == playNow)
        {
            bootminEntryRef.OpenRummy(chartDataList[btnIndex].bootVal);

        }
        else
        {
            bootminEntryRef.addCashPanel.SetActive(true);
        }


    }

    public void OpenLudo(int btnIndex)
    {
        if (chartDataList[btnIndex].img.sprite == playNow)
        {
            bootminEntryRef.LudoEntryFee(chartDataList[btnIndex].minEntry);
        }
        else
        {
            bootminEntryRef.addCashPanel.SetActive(true);
        }

    }
    public IEnumerator VariableRandomPlayerEnum()
    {
        int k = Random.Range(0, onlinePlayerList.Count);
        onlinePlayerCountList[k] += Random.Range(10, 100);
        onlinePlayerList[k].text = onlinePlayerCountList[k].ToString();
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        StartCoroutine(VariableRandomPlayerEnum());
    }
    public void RandomOnlinePlayers()
    {
        for (int i = 0; i < onlinePlayerList.Count; i++)
        {
            onlinePlayerCountList.Add(Random.Range(999, 9999));
            onlinePlayerList[i].text = onlinePlayerCountList[i].ToString(); ;
        }

        StartCoroutine(VariableRandomPlayerEnum());

    }
}

[System.Serializable]
public class ChartDataObj
{
    public int minEntry;
    public int bootVal;
    public Image img;
}