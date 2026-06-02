using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpenBett : MonoBehaviour
{
    public Image[] CPRSButton;
    public Image[] XBetButton;
  public Image[] TopImageColor; 
  public Image[] RandomImage; 
   public TextMeshProUGUI SelectButton;
   public TextMeshProUGUI TotalAmountText;
   public GameObject  BettingPanel;
   private int TotalAmount;
   private string SelectBtn;
   private string SelectType;

  public bool activemybetsbtn;
   private bool GameAcive;
   private int Quantity;
   private TMP_InputField QuantityInput;

    public TextMeshProUGUI[] Btn11;

    private int Xamount;
   public TextMeshProUGUI GunShow;
   public GameObject  MyBetsRecords;
   public GameObject  GameRecords;
   public GameObject  ChartRecords;
   public Sprite ColorButton;
   public Sprite TranButton;
   public Image Btn1;
   public Image Btn2;
   public Image Btn3;
   private string colorCode;
   private int XbetBtnNumber;


    public void ButtonTextColorChange(TextMeshProUGUI Btt)
    {
        for (int i = 0; i < Btn11.Length; i++){
            Btn11[i].color = Color.black;
        }
        Btt.color = Color.white;

    }



    void Start(){
    XbetBtnNumber = 0;
    GameAcive = true;
    TotalAmount = 1;
    Xamount = 1;
    SelectType = "A";
    activemybetsbtn = false;
    Quantity = 1;
    Btn1.sprite = ColorButton;
    Btn2.sprite = TranButton;
    Btn3.sprite = TranButton;
  }

   public void CPBetting(string ButtonName){
        SelectBtn = ButtonName;
        Debug.Log("button name"+ButtonName);
        SelectButton.text = ButtonName;
        BettingPanel.SetActive(true);
        ColorChangeBetting(ButtonName);
        Quantity = 1;
        Xamount = 1;
        TotalAmountText.text = (Quantity*Xamount).ToString();
        GunShow.text = Quantity.ToString();
   }
private void ColorChangeBetting(string ButtonName){
    colorCode = takeColor(ButtonName);
    for (int i = 0; i < TopImageColor.Length; i++){
      TopImageColor[i].color = HexToColor(colorCode); 
    }
    QuantityBetAndXamountBtnColorChange();
}
private void QuantityBetAndXamountBtnColorChange(){
   for(int i = 0; i< XBetButton.Length; i++){
      XBetButton[i].color = Color.white;
      XBetButton[XbetBtnNumber].color = HexToColor(colorCode);
    }
    for(int i = 0; i< CPRSButton.Length; i++){
      CPRSButton[i].color = Color.white;
      CPRSButton[0].color = HexToColor(colorCode);
    }
}
public void Randomm(int changeImage){
  XbetBtnNumber = changeImage;
  for(int i = 0;i<RandomImage.Length;i++){
    RandomImage[i].color = Color.white;
    TextMeshProUGUI imageText = RandomImage[i].GetComponentInChildren<TextMeshProUGUI>();
    imageText.color = Color.black;
    if(i == changeImage){
      RandomImage[i].color = HexToColor("#0f9d58");
      imageText.color = Color.white;
    }
  }  
}
public void RandQuantity(int quantity){
  Quantity = quantity;
  TotalAmountText.text = (Quantity*Xamount).ToString();
  GunShow.text = Quantity.ToString();
}
public void RandDomBetting(){
    int xcount = Random.Range(1, 9);
    SelectBtn = xcount.ToString();
    SelectButton.text = SelectBtn;
    BettingPanel.SetActive(true);
    ColorChangeBetting(SelectBtn);
}


// Modify return type to string
private string takeColor(string ButtonName)
{
    switch (ButtonName)
    {
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

// Helper method to convert hex string to Color in Unity
private Color HexToColor(string hex)
{
    Color newCol;
    if (ColorUtility.TryParseHtmlString(hex, out newCol))
    {
        return newCol;
    }
    return Color.white; // Default to white if parsing fails
}


  public void CPRSAmount(int bettinamount){
    Xamount = bettinamount;
    Quantity = 1;
    TotalAmount = Quantity*Xamount;
    TotalAmountText.text = (Quantity*Xamount).ToString();
    GunShow.text = Quantity.ToString();
    for(int i = 0; i< XBetButton.Length; i++){
      XBetButton[i].color = Color.white;
      XBetButton[0].color = HexToColor(colorCode);;
    }
  }
   public void SelectTypE(string selecttyp){
      SelectType = selecttyp;
      MyRecord();
   }
   public void CPRSButtonChange(Image Button){
        for(int i = 0; i< CPRSButton.Length; i++){
            CPRSButton[i].color = Color.white;
        }
        Button.color = HexToColor(colorCode);;
   }
   public void XButtonChange(Image Button){
        for(int i = 0; i< XBetButton.Length; i++){
            XBetButton[i].color = Color.white;
        }
        Button.color = HexToColor(colorCode);;
   }
   public void XAmount(int XAmount){
    Quantity = XAmount;
    TotalAmount  = Quantity*Xamount;
    GunShow.text = Quantity.ToString();
    TotalAmountText.text = (Quantity*Xamount).ToString();
  }
  

   

   public void MyBestRecords(int currentPage){
      Api ai = FindObjectOfType<Api>();
      ai.MyBestRecord(SelectType,currentPage);
      activemybetsbtn = true;
    }

   public void CPBetting(){
     Api be = FindObjectOfType<Api>();
     be.Betting(TotalAmount, SelectBtn, SelectType);
   }



  public void MyRecord(){
      Btn1.sprite = ColorButton;
      Btn2.sprite = TranButton;
      Btn3.sprite = TranButton;
      TextMeshProUGUI text2 = Btn2.GetComponentInChildren<TextMeshProUGUI>();
      TextMeshProUGUI text3 = Btn3.GetComponentInChildren<TextMeshProUGUI>();
      text2.color = Color.black; // Assuming you meant to set it to black.
      text3.color = Color.black; // Assuming you meant to set it to black.

      GameRecords.SetActive(true);
      MyBetsRecords.SetActive(false);
      ChartRecords.SetActive(false);
  }
   public void OpenMyBetsRecords(Image Btn){
      Btn1.sprite = TranButton;
      Btn2.sprite = TranButton;
      Btn3.sprite = TranButton;
      Btn.sprite = ColorButton;
      GameRecords.SetActive(false);
      MyBetsRecords.SetActive(true);
      ChartRecords.SetActive(false);
   }
  public void OpneGameRecords(Image Btn){
      GameRecords.SetActive(true);
      MyBetsRecords.SetActive(false);
      ChartRecords.SetActive(false);
      Btn1.sprite = TranButton;
      Btn2.sprite = TranButton;
      Btn3.sprite = TranButton;
      Btn.sprite = ColorButton;
      activemybetsbtn = false;
  }
  public void OpneChartRecords(Image Btn){
      ChartRecords.SetActive(true);
      GameRecords.SetActive(false);
      MyBetsRecords.SetActive(false);
      Btn1.sprite = TranButton;
      Btn2.sprite = TranButton;
      Btn3.sprite = TranButton;
      Btn.sprite = ColorButton;
      activemybetsbtn = false;
  }

 public void QuantityAddLive(int value){
    Quantity = value;
    TotalAmount = Quantity*Xamount;
    TotalAmountText.text = (Quantity*Xamount).ToString();
    GunShow.text = Quantity.ToString();
  }

  public void QuantityAdd(){
    TotalAmount *= 2;
    Quantity++;
    TotalAmount = Quantity*Xamount;
    TotalAmountText.text = (Quantity*Xamount).ToString();
    GunShow.text = Quantity.ToString();
  }

  public void QuantityLose(){
    if(Quantity > 1){
      TotalAmount /= 2;
      Quantity--;
      TotalAmount = Quantity*Xamount;
      TotalAmountText.text = (Quantity*Xamount).ToString();
      GunShow.text = Quantity.ToString();
    }
  }

}
