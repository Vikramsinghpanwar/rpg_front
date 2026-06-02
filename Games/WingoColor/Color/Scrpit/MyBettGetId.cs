using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyBettGetId : MonoBehaviour{
   
    public void OpneRecords(TextMeshProUGUI Idd){
        Debug.Log(Idd.text);
        if (int.TryParse(Idd.text, out int iddValue)){
            CP_Betting pa = FindObjectOfType<CP_Betting>();
            pa.OpenMyBets(iddValue);
            Debug.Log(iddValue);
        }
    }
}
