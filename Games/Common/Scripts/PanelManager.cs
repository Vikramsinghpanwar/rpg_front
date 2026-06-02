using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public AudioSource tapSound;
    public InGameRecharge inGameRechargeRef;
    
    public void InGameRecharge()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        inGameRechargeRef.OpenPanel();
    }
    private void Start()
    {
        inGameRechargeRef = FindFirstObjectByType<InGameRecharge>();
        Application.targetFrameRate = 200;
    }
    // Start is called before the first frame update
    public void OpenPanel(GameObject obj)
    {
        obj.SetActive(true);
        TapSoundFunc();
    }

    public void ClosePanel(GameObject obj)
    {
        obj.SetActive(false);
        TapSoundFunc();
    }

    public void TapSoundFunc()
    {
        if (tapSound != null)
        {
            tapSound.Play();

        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            Wallet.AddToWinWallet(1000);
        }
    }
}
