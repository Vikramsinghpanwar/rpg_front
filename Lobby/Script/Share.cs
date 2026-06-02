using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Core.Config;
public class Share : MonoBehaviour
{
    Error errorRef;
    UserData userDataRef;
    public GameObject tablePanel;
    public GameObject cellPrefab;
    public Image ChangePanel;
    public Image DisActive1;
    public Image DisActive2;
    public Image DisActive3;
    public Image DisActive4;
    public Image DisActive5;
    public string toShareTxt;
    public Image DisActive1R;
    public Image DisActive2R;
    public Image DisActive3R;
    public Image DisActive4R;
    public Image DisActive5R;

    public Image tranBtn;
    public Image BgtranBtn;
    public int columns = 3;
    private string ActiveScreen;
    private string ActiveButton;

    

     void Start()
    {
        errorRef = FindObjectOfType<Error>();
        toShareTxt = " Click to download APP\n" + ServerConfig.BaseUrl + "?" + UserDetail.PromoCode;
        ActiveScreen = "ActiveScreen-1";
        ActiveButton = "Refer-Button-1";
        userDataRef = FindObjectOfType<UserData>();
        // GameObject newButton = GameObject.Find(ActiveButton);
        // Image newButtonImage = newButton.GetComponent<Image>();
        // newButtonImage.sprite = BgtranBtn.sprite;
    }

    public void ChabeAcy(Image  NewTaIma){ 
        DisActive1.gameObject.SetActive(false);
        DisActive2.gameObject.SetActive(false);
        DisActive3.gameObject.SetActive(false);
        DisActive4.gameObject.SetActive(false);
        DisActive5.gameObject.SetActive(false);

        DisActive1R.gameObject.SetActive(false);
        DisActive2R.gameObject.SetActive(false);
        DisActive3R.gameObject.SetActive(false);
        DisActive4R.gameObject.SetActive(false);
        DisActive5R.gameObject.SetActive(false);
        NewTaIma.gameObject.SetActive(true);
    }
      public void AddRow(string[] cellContents)
    {
        for (int i = 0; i < columns; i++)
        {
            GameObject cell = Instantiate(cellPrefab, tablePanel.transform);
        }
    }
    public void ChangeButtonColor(string NewButtonC){
        GameObject OldBtn = GameObject.Find(ActiveButton);
        Image OleBtnImage = OldBtn.GetComponent<Image>();
        OleBtnImage.sprite = tranBtn.sprite;

       GameObject newButton = GameObject.Find(NewButtonC);
       Image newButtonImage = newButton.GetComponent<Image>();
       newButtonImage.sprite = BgtranBtn.sprite;
       ActiveButton = NewButtonC;
    }

    /// <summary>
    /// y function support k liye h
    /// </summary>

    ///Support
    public void OpenWhatsApp()
    {
        string url = "https://wa.me/" + UserDetail.WhatsApp;
        Application.OpenURL(url);
    }

    public void OpenTelegram()
    {
        string url = "https://t.me/" + UserDetail.Telegram;
        Application.OpenURL(url);
    }



    /// <summary>
    /// //////////Win Share
    /// </summary>

    public void ShareOnWhatsApp()
    {
        string url = "https://api.whatsapp.com/send?text=" + WWW.EscapeURL(UserDetail.refertext + "\n\nReferal Code : " + UserDetail.PromoCode + "\n\n" + toShareTxt);
        Application.OpenURL(url);

    }



    public void ShareOnTelegram()
    {
        string url = "https://telegram.me/share/url?url=" + WWW.EscapeURL(UserDetail.refertext + "\n\nReferal Code : " + UserDetail.PromoCode + "\n\n" + toShareTxt);
        Application.OpenURL(url);
        Debug.Log("url is  \n" + url);
    }

    public void CopyUrlToClipboard()
    {
        string urlToCopy;
        urlToCopy = UserDetail.refertext + "\n\nReferal Code : " + UserDetail.PromoCode + "\n\n" + toShareTxt;
        GUIUtility.systemCopyBuffer = urlToCopy;
        errorRef.Show("Copied !!!");
    }
}
