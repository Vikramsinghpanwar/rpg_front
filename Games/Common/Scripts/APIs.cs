using System;
using System.Collections;
using System.Collections.Generic;
using Core.Config;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class APIs : MonoBehaviour
{
    public event System.Action<float> OnWalletFetched;

    public void FetchWallet()
    {
        //StartCoroutine(RechResultRech());
    }

    internal void FetchtWallet()
    {
        throw new NotImplementedException();
    }
}

[System.Serializable]
public class WalletData
{
    public float wallet;
    public float WinAmount;
    public float bonus;
    public float pool_teenpatti;
    public int status;
}
