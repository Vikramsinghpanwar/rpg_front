using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Features.Lobby.Integration;

public class ManagerSlot : MonoBehaviour
{
    public List<ReelImages> slotElementsList;
    // Start is called before the first frame update
    public float wallet;
    public TextMeshProUGUI walletText;
    public Jugad jugadRef;

    private void Start()
    {
        wallet = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
        walletText.text = wallet.ToString("F2");
        jugadRef = FindObjectOfType<Jugad>();
    }
    public void WalletUpdate(float val)
    {

        wallet += val;
        walletText.text = wallet.ToString("F2");
        Wallet.AddToWinWallet(val);
        walletText.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");

    }

    public void DeductFromWallet(float val)
    {
        wallet -= val;
        Wallet.DeductAmount(val);
        walletText.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
    }
    public void Lobby()
    {
        SceneManager.LoadScene(1);
    }

    public void AddCash()
    {
        PlayerPrefs.SetInt("_addCash", 1);
        SceneManager.LoadScene(1);
    }
}
