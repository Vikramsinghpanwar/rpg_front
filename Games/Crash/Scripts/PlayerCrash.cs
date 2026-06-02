using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;
using Features.Lobby.Integration;

public class PlayerCrash : MonoBehaviour
{


    public float wallet;
    public Text walletText;
    // Start is called before the first frame update
    void Start()
    {
        wallet = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
        walletText.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateWallet(float val)
    {

        Debug.Log("Updated");
        wallet += val;
        walletText.text = "₹ " + wallet.ToString("F2");
        Wallet.AddToWinWallet(val);
    }

}
