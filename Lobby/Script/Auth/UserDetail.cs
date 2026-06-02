using System;
using UnityEngine;

public static class UserDetail
{
    public static string Message { get; set; }
    public static string Token { get; set; }
    public static int Status { get; set; }
    public static string UserId { get; set; }
    public static string profileLevel { get; set; }
    public static string UserName { get; set; }
    public static long Mobile { get; set; }
    public static string Email { get; set; }
    public static string EmailC { get; set; }
    public static float Wallet { get; set; }
    public static float Bonus { get; set; }
    public static float WinAmount { get; set; }
    public static int WithdrawalLimit { get; set; }
    public static int WithdrawalCommision { get; set; }
    public static string PromoCode { get; set; }

    public static int withmax { get; set; }
    public static long WhatsApp { get; set; }
    public static string Telegram { get; set; }
    public static int Amount1 { get; set; }
    public static int Level1 { get; set; }
    public static int Level2 { get; set; }
    public static int Level3 { get; set; }
    public static int Level4 { get; set; }
    public static int Friends { get; set; }
    public static string UpiId { get; set; }
    public static string HolderUpi { get; set; }
    public static string Account { get; set; }
    public static string HolderBank { get; set; }
    public static string IfscCode { get; set; }
    public static string BankName { get; set; }
    public static string SportImage { get; set; }
    public static string ReferImage { get; set; }
    public static string refertext { get; set; }
    public static int Day { get; set; }
    public static int Daily { get; set; }
    public static float UnclearWallet { get; set; }
    public static RechargeData[] rechargeData { get; set; }

    public static int profileImageIndex { get; set;} 

    public static int gateway1 { get; set; }
    public static int gateway2 { get; set; }
    public static int gateway3 { get; set; }
    public static int gateway4 { get; set; }

    public static void LoadFromResponse(GameData response, RechargeData[] rData)
    {
        if (response == null) return;
        Message = response.message;
        Token = response.vertoken;
        UserId = response.userid;
        profileLevel = response.profilelevel;
        UserName = response.name;
        Mobile = response.mobile;
        profileImageIndex = response.image_index;
        Email = response.email;
        EmailC = response.emailC;
        Wallet = response.wallet;
        Bonus = response.bonus;
        WinAmount = response.winamount;
        WithdrawalLimit = response.withlimit;
        withmax = response.withmax;
        WithdrawalCommision = response.withcommisson;
        PromoCode = response.promocode;
        WhatsApp = response.whatapp;
        Telegram = response.telegram;
        Amount1 = response.amount1;
        Level1 = response.level1;
        Level2 = response.level2;
        Level3 = response.level3;
        Level4 = response.level4;
        Friends = response.friends;
        UpiId = response.upiid;
        HolderUpi = response.holderupi;
        Account = response.account;
        HolderBank = response.holderbank;
        IfscCode = response.ifsccode;
        BankName = response.bankname;
        SportImage = response.sport_image;
        ReferImage = response.refer_image;
        Day = response.day;
        Daily = response.daily;
        UnclearWallet = response.UnclearWallet;
        rechargeData = rData;
        refertext = response.refertext;  
        gateway1 = response.gateway1;  
        gateway2 = response.gateway2;  
        gateway3 = response.gateway3;  
        gateway4 = response.gateway4;  
    }

    public static void PopulateData(TokenValidationResponse data)
    {
        GameData gData = data.userdata; // Access the first user
        RechargeData[] rechargeData = data.recharge;
        LoadFromResponse(gData, rechargeData);
    }
}

[System.Serializable]
public class GameData
{
    public string refertext;
    public string message;
    public string vertoken;
    public string userid;
    public int withmax;
    public string name;
    public int status;
    public long mobile;
    public int image_index;
    public string email;
    public string emailC;
    public float wallet;
    public float bonus;
    public float winamount;
    public int withlimit;
    public int withcommisson;
    public string promocode;
    public string profilelevel;
    public long whatapp;
    public string telegram;
    public int amount1;
    public int level1;
    public int level2;
    public int level3;
    public int level4;
    public int friends;
    public string upiid;
    public string holderupi;
    public string account;
    public string holderbank;
    public string ifsccode;
    public string bankname;
    public string sport_image;
    public string refer_image;
    public int day;
    public int daily;
    public float UnclearWallet;
    public int gateway1;
    public int gateway2;
    public int gateway3;
    public int gateway4;
}

[System.Serializable]
public class RechargeData{
    public int id;
    public int amount;
    public int bonus;
}

[System.Serializable]
public class TokenValidationResponse
{
    public int status;
    public GameData userdata;
    public RechargeData[] recharge;
}