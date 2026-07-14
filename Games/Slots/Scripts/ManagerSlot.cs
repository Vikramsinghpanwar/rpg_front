using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Features.Lobby.Integration;
using Core.Utils;
using Core.Bootstrap;

public class ManagerSlot : MonoBehaviour
{
    public List<ReelImages> slotElementsList;
    // Start is called before the first frame update
    public float wallet;
    public TextMeshProUGUI walletText;
    public Jugad jugadRef;

    private void Start()
    {
        wallet = (BootstrapService.Instance.Wallet != null ? BootstrapService.Instance.Wallet.available_balance : 0);
        walletText.text = MoneyFormatter.FormatPaisa((long)(wallet * 100));
        jugadRef = FindObjectOfType<Jugad>();
    }
    public void WalletUpdate(float val)
    {

        wallet += val;
        walletText.text = MoneyFormatter.FormatPaisa((long)(wallet * 100));
        Wallet.AddToWinWallet(val);
        walletText.text = MoneyFormatter.FormatPaisa((long)(wallet * 100));

    }

    public void DeductFromWallet(float val)
    {
        wallet -= val;
        Wallet.DeductAmount(val);
        walletText.text = MoneyFormatter.FormatPaisa((long)(wallet * 100));
    }
    public void Lobby()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void AddCash()
    {
        PlayerPrefs.SetInt("_addCash", 1);
        SceneManager.LoadScene("Lobby");
    }
}
