using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Betting : MonoBehaviour
{
    public Text UserWalletMain;
    public Text UserWalletP;
    public Text UserBonusP;
    public Text UserBonusMain;
    private float UserWallet;
    private float UserBonus;

    public void BettingAMount(float Wallet){
        float UserWallet = UserDetail.Wallet;      
        float NewUserWallet = UserWallet-Wallet;
        
        PlayerPrefs.SetFloat("Wallet", NewUserWallet);
        UserWalletMain.text = NewUserWallet.ToString();
        UserWalletP.text = NewUserWallet.ToString();
        PlayerPrefs.SetFloat("Wallet", NewUserWallet);
    }  
     
    public void BeetingResult(float NewAMount){
        float UserBonus = UserDetail.Bonus;
        float NewUserBonus = UserBonus+NewAMount;
        UserBonusP.text = NewUserBonus.ToString();
        UserBonusMain.text = NewUserBonus.ToString();
        UserDetail.Bonus = NewUserBonus;
    }

}
