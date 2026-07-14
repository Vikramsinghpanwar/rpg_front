using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;
using Features.Lobby.Integration;
using Core.Utils;
using Core.Bootstrap;

public class PlayerCrash : MonoBehaviour
{


    public float wallet;
    public Text walletText;
    // Start is called before the first frame update
    void Start()
    {
        wallet = BootstrapService.Instance.Wallet != null ? BootstrapService.Instance.Wallet.available_balance : 0;
        walletText.text = MoneyFormatter.FormatPaisa((long)(wallet * 100));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateWallet(float val)
    {
        wallet += val;
        walletText.text = MoneyFormatter.FormatPaisa((long)(wallet * 100));
    }

}
