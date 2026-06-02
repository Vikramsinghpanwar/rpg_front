using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Const
{
#if UNITY_EDITOR
   //public static string Server_Url = "http://172.18.128.1:3000/"; // Production URL
   // public static string Server_Url = "https://dtexch.online/"; // Production URL
   public static string Server_Url = "https://d2ludo.demotele.online/"; // Production URL
   //public static string Server_Url = "http://localhost:8000/"; // Localhost URL
      // public static string Server_Url = "https://www.thecrownempire.live/"; // Production URL

#elif UNITY_ANDROID
   public static string Server_Url = "https://d2ludo.demotele.online/"; // Production URL
   // public static string Server_Url = "https://www.thecrownempire.live/"; // Production URL

   //public const string Server_Url = "https://dtexch.online/"; // Production URL for Android
#else
   public const string Server_Url = "http://localhost:8000/"; // Production URL
#endif

    public static string landingPageURL = "https://thecrownempire.live"; // Production URL
    public static string gameRulesURL = "";

    public static string termsAndConditionsURL = "";
    public static string aboutUsURL = "";
    public static string privacyPolicyURL = "";
    public static string SupportURL = "";
    public static string whatsAppURL = "";
    public static string telegramURL = "";

    public static string shareApkText = "";
    public static string roomCodeShareLink = "";
    //public static string teenpattiURL = "https://d2t.demotele.online";
    //public static string teenpattiURL = "http://localhost:3000";

 public static bool isSecure = true;
    public static string teenpattiHost = "d2t.thecrownempire.live"; // Your server IP
   //  public static string teenpattiHost = "localhost"; // Your server IP
   //  public static string teenpattiPort = "8080";
    public static string teenpattiPort = "";
    public static string teenpattiURL => $"{(isSecure ? "wss://" : "ws://")}{teenpattiHost}:{teenpattiPort}/ws";
   public static string gatewayURL = "https://google.com";


    public static string gameVersion = "1.0.7";
}
