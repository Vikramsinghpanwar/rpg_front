using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CP_Chart : MonoBehaviour{

    public GameObject Container;
    public GameObject ChartPerfab;
    HistoryGenerator_CP historyRef;
    public Color myGreen;
    public Color myRed;
    public Color myPurple;

    public Color yellow;
    public Color skyBlue;

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        historyRef = FindObjectOfType<HistoryGenerator_CP>();
    }
    public void chartdata(string json){
        ApiResponses data = JsonUtility.FromJson<ApiResponses>(json);
        ChartdataShow(data.game_history);
    }
    void ChartdataShow(GameHistoryDatas[] data){
        foreach (Transform child in Container.transform){
            Destroy(child.gameObject);
        }

        int k = 0;
        historyRef.Clear();
        foreach (GameHistoryDatas rowdata in data){
            k++;
            if(rowdata == null){
                Debug.Log("data not found");
                continue;
            }
            if(k < 6)
            {
                historyRef.AddCoin(rowdata.number, true);
            }
            GameObject newRow = Instantiate(ChartPerfab, Container.transform);
            Text[] textComponents = newRow.GetComponentsInChildren<Text>();
            Image[] ImageComponents = newRow.GetComponentsInChildren<Image>();
            textComponents[0].text = rowdata.gameid.ToString();
            if(rowdata.number == 0){
                ImageComponents[3].color= myRed;
                ImageComponents[4].color= myPurple;
            }else if(rowdata.number == 5){
                ImageComponents[18].color= myGreen;
                ImageComponents[19].color= myPurple;
            }else if(rowdata.number == 1){
                ImageComponents[6].color= myGreen;
                ImageComponents[7].color= myGreen;
            }else if(rowdata.number == 2){
                ImageComponents[9].color= myRed;
                ImageComponents[10].color= myRed;
            }else if(rowdata.number == 3){
                ImageComponents[12].color= myGreen;
                ImageComponents[13].color= myGreen;
            }else if(rowdata.number == 4){
                ImageComponents[15].color= myRed;
                ImageComponents[16].color= myRed;
            }else if(rowdata.number == 6){
                ImageComponents[21].color= myRed;
                ImageComponents[22].color= myRed;
            }else if(rowdata.number == 7){
                ImageComponents[24].color= myGreen;
                ImageComponents[25].color= myGreen;
            }else if(rowdata.number == 8){
                ImageComponents[27].color= myRed;
                ImageComponents[28].color= myRed;
            }else if(rowdata.number == 9){
                ImageComponents[30].color= myGreen;
                ImageComponents[31].color= myGreen;
            }


            // ImageComponents[6].color= myRed;
            // ImageComponents[7].color= myRed;
            // ImageComponents[9].color= myGreen;
            // ImageComponents[10].color= myGreen;
            TextMeshProUGUI childText = ImageComponents[35].GetComponentInChildren<TextMeshProUGUI>();
            if (childText != null) {
                string big = rowdata.bigsmall.Substring(0, 1);
                if(big == "B"){
                    childText.text = big;
                    ImageComponents[36].color= yellow;
                    ImageComponents[37].color= yellow;
                }else{
                    childText.text = big;
                    ImageComponents[36].color= skyBlue;
                    ImageComponents[37].color= skyBlue;
                }
            }
        }
    }
}



[System.Serializable]
public class GameHistoryDatas {
    public long gameid;
    public int number;
    public string bigsmall;
    public string color;
}



[System.Serializable]
public class ApiResponses {
    public int wallet;
    public float bettamount;
    public float winningamount;
    public int current_page;
    public int total_pages;
    public GameHistoryDatas[] game_history;
}