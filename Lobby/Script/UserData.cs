using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Lobby.Integration;
using Core.Bootstrap;

public class UserData : MonoBehaviour
{
    public static UserData Instance;
    public Text totalWalletInWithdraw;
    public Text bonus_InWithdraw;

    public Text userLevelText;
    public Text walletInRank;
    public Text mobileNumberTxt;
    public Text UserNameP;
    public Text UserNameMain;
    public Text UserIdP;
    public Text UserIdMain;
    public Text UserWalletMain;
    public Text UserWalletP;
    public Text UserBonusP;
    public Text UserBonusMain;
    public Text friend;
    public Text level1;
    public Text level2;
    public Text level3;
    public Text level4;
    public Text amount1;
    public Text Promocode;
    public TextMeshProUGUI Email;

    private string promo;

    public Text Ruless;
    public Text RulessRe;




    public Image AvImagemain;
    public Image ChooseImage;
    public Image EditPanelImage;


    public Text withdrawWalletTxt;
    void Awake()
    {
        if (Instance != this)
        {
            Instance = this;
        }
    }

    public void WallCha(float wallet, float Bonus, float Winning, float pool)
    {
        // Wallet.SetDepositWallet(wallet);
        // Wallet.SetWinWallet(Winning);
        // Wallet.SetBonus(Bonus);
        // Wallet.SetPool(pool);


        UserWalletMain.text = "₹ " + (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        //walletInRank.text = "₹ " + (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        UserWalletP.text = "₹ " + (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        UserBonusP.text = "₹ " + Bonus.ToString("F2");
        UserBonusMain.text = "₹ " + (Bonus).ToString("F2");
        withdrawWalletTxt.text = "₹ " + (BootstrapService.Instance.Wallet.win_balance / 100f + BootstrapService.Instance.Wallet.deposit_balance / 100f).ToString("F2");
        totalWalletInWithdraw.text = "₹ " + (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        bonus_InWithdraw.text = "₹ " + (BootstrapLobbyAdapter.GetBonusBalance() / 100f).ToString("F2");

    }

    void Start()
    {

        withdrawWalletTxt.text = "₹ " + (BootstrapService.Instance.Wallet.win_balance / 100f + BootstrapService.Instance.Wallet.deposit_balance / 100f).ToString("F2");
        string Username = UserDetail.UserName;
        UserNameP.text = Username;
        UserNameMain.text = Username;
        friend.text = UserDetail.Friends.ToString();
        level1.text = UserDetail.Level1 + "%";
        level2.text = UserDetail.Level2 + "%";
        level3.text = UserDetail.Level3 + "%";
        level4.text = UserDetail.Level4 + "%";
        amount1.text = UserDetail.Amount1.ToString();
        Promocode.text = UserDetail.PromoCode;
        promo = UserDetail.PromoCode;
        UserIdP.text = BootstrapLobbyAdapter.GetUserId();
        UserIdMain.text = BootstrapLobbyAdapter.GetUserId();
        UserWalletMain.text = "₹ " + (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        UserWalletP.text = "₹ " + (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        UserBonusP.text = "₹ " + (BootstrapLobbyAdapter.GetBonusBalance() / 100f).ToString("F2");
        UserBonusMain.text = "₹ " + (BootstrapLobbyAdapter.GetBonusBalance() / 100f).ToString("F2");
        bonus_InWithdraw.text = "₹ " + (BootstrapLobbyAdapter.GetBonusBalance() / 100f).ToString("F2");
        mobileNumberTxt.text = UserDetail.Mobile.ToString();
        // string NewImage = UserDetail.ProfileImage;
        // ProfileImage();
        if (userLevelText != null)
        {
            userLevelText.text = UserDetail.profileLevel;
        }
    }


    public void UserNameChange(string username)
    {
        string UserNamee = username;
        if (username == null)
        {
            Debug.LogError("function is not coming value");
        }
        if (UserNameP == null)
        {
            Debug.LogError("function is not coming value");
        }
        UserNameP.text = username;
        UserNameMain.text = username;
    }

    public void ProfileImage(Sprite s)
    {
        AvImagemain.sprite = s;
        EditPanelImage.sprite = s;
        ChooseImage.sprite = s;
    }


}
