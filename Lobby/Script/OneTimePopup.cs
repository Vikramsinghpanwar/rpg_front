using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Bootstrap;

public class OneTimePopup : MonoBehaviour
{
    public static bool _isFirstTimeLobby;
    public GameObject[] panelsToActivate;
    public GameObject paymentPanel;

    void Start()
    {
        if (_isFirstTimeLobby != true)
        {
            OpenNextPanel(0);
        }
    }

    public void PanelClosed(int val)
    {
        if (!_isFirstTimeLobby)
        {
            if (val < panelsToActivate.Length - 1)
            {
                OpenNextPanel(val + 1);
            }
            else
            {
                _isFirstTimeLobby = true;
            }
        }
    }

    void OpenNextPanel(int index)
    {
        if (panelsToActivate == null || index >= panelsToActivate.Length) return;
        if (PlayerPrefs.GetInt("LobbyNavigation.FirstLobbyOpen", 0) == 1)
        {
            _isFirstTimeLobby = true;
            return;
        }
        if (BootstrapService.Instance == null || !BootstrapService.Instance.HasData)
        {
            _ = DeferUntilBootstrap(index);
            return;
        }
        panelsToActivate[index].SetActive(true);
    }

    async System.Threading.Tasks.Task DeferUntilBootstrap(int index)
    {
        while (BootstrapService.Instance == null || !BootstrapService.Instance.HasData)
        {
            await System.Threading.Tasks.Task.Yield();
        }
        OpenNextPanel(index);
    }
}
