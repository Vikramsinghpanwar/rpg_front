using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CP_Betting : MonoBehaviour
{
    public GameObject MyBetsPerfab;
    public GameObject MyBetsContainer;
    public Image displayImage;
    public TextMeshProUGUI PageShow;
    public TextMeshProUGUI SelectMyBetShow;
    public TextMeshProUGUI PurchaseAmount;
    public TextMeshProUGUI AmountAfterTax;
    public TextMeshProUGUI ResultMyBetShow;
    public TextMeshProUGUI TexMyBetShow;
    private string jsonData;
    private int currentPage = 1;
    private int limit = 10;
    private int totalPages = 1;
    public GameObject ActivePanel;

    public void ChangePage(){
        currentPage =  1;
        // OpenBett bee = FindObjectOfType<OpenBett>();
        // bee.MyBestRecords(currentPage);
    }

    public void CPBetting(string jsonResponse){
        this.jsonData = jsonResponse;
        MyBetsData[] MyBetData = JsonHelper.FromJson<MyBetsData>(jsonResponse);
        int pageLenth = MyBetData.Length-1;
        totalPages = MyBetData[pageLenth].total_pages;
        PageShow.text = currentPage + "/" + totalPages;
        if(MyBetData[0].status == 5 ){
            Debug.Log(MyBetData[0].message);
            MyBestsDel();
        }else{
            TeamShow(MyBetData);
        }
    }

    private void MyBestsDel(){
        foreach (Transform child in MyBetsContainer.transform) {
            Destroy(child.gameObject);
        }
        displayImage.gameObject.SetActive(true);
    }
    void TeamShow(MyBetsData[] data) {
        displayImage.gameObject.SetActive(false);
        foreach (Transform child in MyBetsContainer.transform) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < data.Length-1; i++) {
            MyBetsData rowData = data[i];
            if (rowData == null) {
                Debug.LogError("Row data is null.");
                continue;
            }
            GameObject newRow = Instantiate(MyBetsPerfab, MyBetsContainer.transform);
            TextMeshProUGUI[] textComponents = newRow.GetComponentsInChildren<TextMeshProUGUI>();
            Image[] ImageComponents = newRow.GetComponentsInChildren<Image>();
            
            if (textComponents.Length >= 6) {
                 textComponents[0].text = rowData.id.ToString();
                 textComponents[1].text = rowData.select;
                 textComponents[2].text = rowData.gameid.ToString();
                 textComponents[3].text = rowData.time;
                 string gameIdString = PlayerPrefs.GetString("GameId", "0"); // Default to "0" if not found
                long oldGameId = long.Parse(gameIdString);
                string Colorr = takeColor(rowData.select);
                ImageComponents[1].color = HexToColor(Colorr); 
                
                if(rowData.betamount < rowData.winamount){
                    textComponents[4].text = "+" + rowData.winamount.ToString();
                    textComponents[4].color = HexToColor("#0f9d58");
                    textComponents[5].color = HexToColor("#0f9d58");
                    textComponents[5].text = "Success";
                    ImageComponents[2].color = HexToColor("#0f9d58"); 
                 }else{
                    textComponents[4].text = "-" + rowData.betamount.ToString();
                    textComponents[4].color = HexToColor("#ff5f5e");
                    textComponents[5].color = HexToColor("#ff5f5e");
                    textComponents[5].text = "Failed";
                    ImageComponents[2].color = HexToColor("#ff5f5e"); 
                 }
                   if(oldGameId == rowData.gameid){
                    textComponents[4].text = rowData.betamount.ToString();
                    textComponents[4].color = HexToColor("#0f9d58");
                    textComponents[5].color = HexToColor("#0f9d58");
                    textComponents[5].text = "Pending";
                 }
                 ToString();
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
    // open Mybets Records In
    public void OpenMyBets(int iddd){
        ActivePanel.SetActive(true);
        MyBetsData[] MyBetData = JsonHelper.FromJson<MyBetsData>(jsonData);
        Debug.Log("" + jsonData);
        foreach (MyBetsData bet in MyBetData){
            if (bet.id == iddd){
                SelectMyBetShow.text = bet.select;
                PurchaseAmount.text = bet.Purchase.ToString();
                ResultMyBetShow.text = bet.result;
                TexMyBetShow.text = bet.tex.ToString();
                AmountAfterTax.text = (bet.Purchase-bet.tex).ToString();
            }
        }
    }


    public void NextPage(){
        if (currentPage < totalPages){
            currentPage++;
            OpenBett ap = FindObjectOfType<OpenBett>();
            ap.MyBestRecords(currentPage);
        }
    }

    public void PreviousPage(){
        if (currentPage != 1){
            currentPage--;
            OpenBett ap = FindObjectOfType<OpenBett>();
            ap.MyBestRecords(currentPage);
        }
    }
    private string takeColor(string ButtonName){
        switch (ButtonName)        {
            case "Big":
                return "#feaa57";
            case "Small":
                return "#6EA8F4";
            case "Green":
                return "#0f9d58";
            case "Violet":
                return "#c86eff";
            case "Red":
                return "#ff5f5e";
            default:
                // Check if the input is a number
                if (int.TryParse(ButtonName, out int number))
                {
                    switch (number)
                    {
                        case 0:
                        case 5:
                            return "#c86eff";
                        case 1:
                        case 3:
                        case 7:
                        case 9:
                            return "#0f9d58";
                        case 2:
                        case 4:
                        case 6:
                        case 8:
                            return "#ff5f5e";
                        default:
                            return "#ff5f5e"; // default color
                    }
                }
                else
                {
                    return "#ff5f5e"; // default color for unknown cases
                }
        }
    }
}

   [System.Serializable]
    public class MyBetsData {
    public int status;
    public int id;
    public string message;
    public long gameid;
    public float betamount;
    public float Purchase;
    public float winamount;
    public string select;
    public float tex;
    public string time;
    public string result;
    public int total_pages;
}