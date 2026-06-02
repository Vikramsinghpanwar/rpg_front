using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CP_histroy : MonoBehaviour{
    public Color purple;
    public Color myRed;
    public Color myGreen;

    public TextMeshProUGUI GameIdShow;
    public Text UserWalletShow;
    public GameObject OneMinPerfab;
    public GameObject OneMinContainer;
    public GameObject WinningPriceOpopup; 
    public TextMeshProUGUI StatusUpdate;
    public TextMeshProUGUI StatusUpdate2;
    public TextMeshProUGUI PeriodPopupIn;
    public TextMeshProUGUI PopupInWinColor;
    public TextMeshProUGUI PopupInWinNumber;
    public TextMeshProUGUI PopupInWinBigSmall;
    public Text WinningAmount;
    public Image ChangeWinPopup;
    public Sprite LosePopup;
    public Sprite WinPopup;
    public  long gameid;
    public TextMeshProUGUI GamePagination;
    public TextMeshProUGUI GamePaginationC;
    private int currentPage = 1;
    private int limit = 10;
    private int totalPages = 1;
    public void CPGameHistroy(string jsonResponse){
        // Deserialize the JSON response into ApiResponse
        ApiResponse responseData = JsonUtility.FromJson<ApiResponse>(jsonResponse);
        // Pass game history to TeamShow method
        TeamShow(responseData.game_history);
        gameid = responseData.game_history[0].gameid + 1;
        totalPages =  responseData.total_pages;
        currentPage =  responseData.current_page;
        GamePagination.text = responseData.current_page +"/"+ responseData.total_pages .ToString();
        GamePaginationC.text = responseData.current_page +"/"+ responseData.total_pages .ToString();
        PlayerPrefs.SetString("GameId", gameid.ToString());
        if(responseData.bettamount != 0 ){
            WinningPriceOpopup.SetActive(true);
            float winn = responseData.winningamount-responseData.bettamount;
            if(winn > 0){
                StatusUpdate.text = "Congratulations";
                StatusUpdate2.text = "Bonus";
                ChangeWinPopup.sprite = WinPopup;
                WinningAmount.text = "₹ " +winn.ToString("00");
            }else{
                StatusUpdate2.text = "Loss";
                ChangeWinPopup.sprite = LosePopup;
                StatusUpdate.text = "Sorry";
            }
            PopupInWinColor.text = responseData.game_history[0].color;
            PeriodPopupIn.text = "Period: Win X 1 "+ responseData.game_history[0].gameid.ToString();
            PopupInWinNumber.text = responseData.game_history[0].number.ToString();
            PopupInWinBigSmall.text = responseData.game_history[0].bigsmall.ToString();
            Invoke("HideWinPopup", 3F);
        }
        GameIdShow.text = gameid.ToString();
        UserWalletShow.text = "₹"+  responseData.wallet.ToString("N2");
    }


    void HideWinPopup(){
         WinningPriceOpopup.SetActive(false);
    }

      void TeamShow(GameHistoryData[] data) {
        if (OneMinPerfab == null || OneMinContainer == null) {
            Debug.LogError("OneMinPerfab or OneMinContainer is not assigned.");
            return;
        }
        // Clear all existing child objects from OneMinContainer
        foreach (Transform child in OneMinContainer.transform) {
            Destroy(child.gameObject);
        }
        if (data == null || data.Length == 0) {
            Debug.LogError("Data array is null or empty.");
            return;
        }
        foreach (GameHistoryData rowData in data) {
            if (rowData == null) {
                Debug.LogError("Row data is null.");
                continue;
            }
            GameObject newRow = Instantiate(OneMinPerfab, OneMinContainer.transform);
            Text[] textComponents = newRow.GetComponentsInChildren<Text>();
            Image[] ImageComponents = newRow.GetComponentsInChildren<Image>();
            if (textComponents.Length >= 3) {
                string color1 = rowData.color;
                textComponents[0].text = rowData.gameid.ToString();
                textComponents[1].text = rowData.number.ToString();
                textComponents[2].text = rowData.bigsmall;
                // textComponents[3].text = rowData.color;
                Color color;
                Debug.Log("row data : " + rowData.number);
                Debug.Log("row data : " + rowData.number.GetType());
                
                
              
                int k = rowData.number;
                Debug.Log("k : " + k);
                if (k == 0)
                {
                    Debug.Log("hum");
                    textComponents[1].color = HexToColor("#c86eff");
                    ImageComponents[1].color = myRed;
                    textComponents[1].text = "";
                    ImageComponents[2].color = purple;
                    ImageComponents[3].gameObject.SetActive(false);
                }

                else if (k == 5)
                {
                    Debug.Log("tum");
                    textComponents[1].text = "";
                    ImageComponents[1].color = myGreen;
                    textComponents[1].color = HexToColor("#c86eff");
                    ImageComponents[2].color = purple;
                    ImageComponents[4].gameObject.SetActive(false);

                }

                else if (k != 0 && k != 5)
                {
                    Debug.Log("eeee");
                    ImageComponents[1].color = HexToColor(rowData.color);
                    textComponents[1].color = HexToColor(rowData.color);
                    ImageComponents[2].gameObject.SetActive(false);
                    ImageComponents[3].gameObject.SetActive(false);
                    ImageComponents[4].gameObject.SetActive(false);
                }


                else
                {
                    Debug.Log("Babu bhaiya");
                }
            } else {
                Debug.LogError("Not enough Text components in the row prefab. Found: " + textComponents.Length);
            }
        }
    }

private Color HexToColor(string hex)
{
    Color newCol;
    if (ColorUtility.TryParseHtmlString(hex, out newCol))
    {
        return newCol;
    }
    return Color.white; // Default to white if parsing fails
}

    public void NextPage(){
        if (currentPage < totalPages){
            currentPage++;
            TimerCP ti = FindObjectOfType<TimerCP>();
            Api ap = FindObjectOfType<Api>();
            ap.StartApI(ti.GameTime,currentPage);
        }
    }

    public void PreviousPage(){
        if (currentPage != 1){
            currentPage--;
            TimerCP ti = FindObjectOfType<TimerCP>();
            Api ap = FindObjectOfType<Api>();
            ap.StartApI(ti.GameTime,currentPage);
        }
    }
}

[System.Serializable]
public class GameHistoryData {
    public long gameid;
    public int number;
    public string bigsmall;
    public string color;
}



[System.Serializable]
public class ApiResponse {
    public float wallet;
    public float bettamount;
    public float winningamount;
    public int current_page;
    public int total_pages;
    public GameHistoryData[] game_history;
}