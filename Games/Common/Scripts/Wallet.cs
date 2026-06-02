using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet
{
    static float depositWallet;
    static float winWallet;
    static float bonus;
    // Start is called before the first frame update
    static float pool;
    public static float slots_Pool;
    public static float teenpattiV_Pool;

    static float UnclearWallet;

    public static void SetUnclearWallet(float val)
    {
        UnclearWallet = val;
    }
    public static float GetUnclearWallet()
    {
        return UnclearWallet;
    }
    public static void AddToUnclearWallet(float val)
    {
        UnclearWallet += val;
    }

    public static void AddATD(float val)
    {
        PlayerPrefs.SetFloat("AmountToDeduct", PlayerPrefs.GetFloat("AmountToDeduct") + val);
    }
    public static void DeductATD(float val)
    {
        PlayerPrefs.SetFloat("AmountToDeduct", PlayerPrefs.GetFloat("AmountToDeduct") - val);
    }
    public static void SetBonus(float val)
    {
        bonus = val;
    }


    public static void SetPool(float val)
    {
        pool = val;
    }
    public static float GetPool()
    {
        return pool;
    }

    public static void DeductFromPool(float val)
    {
        pool -= val;
    }
    public static void AddToPool(float val)
    {
        pool += val;
    }


    public static float GetBonus()
    {
        Debug.Log("bonus is : " + bonus);
        return bonus;
    }
    public static void SetDepositWallet(float val)
    {
        depositWallet = val;
    }

    public static void SetWinWallet(float val)
    {
        winWallet = val;
    }
    public static void AddToMainWallet(float val)
    {
        depositWallet += val;
    }
    public static void AddToWinWallet(float val)
    {
        winWallet += val;
    }


    public static void AddToBonus(float val)
    {
        bonus += val;
    }


    public static void DeductAmount(float val)
    {
        if (depositWallet > val)
        {
            depositWallet -= val;

        }
        else
        {
            if (depositWallet + winWallet >= val)
            {
                winWallet -= (val - depositWallet);
                depositWallet = 0;

            }
            else if (depositWallet + winWallet + bonus >= val)
            {
                bonus -= (val - depositWallet - winWallet);

                depositWallet = 0;
                winWallet = 0;

            }
            else
            {
            }
        }
    }

    public static float GetTotalWallet()
    {
        return depositWallet + winWallet + bonus;
    }


    public static float GetDepositWallet()
    {
        return depositWallet;
    }

    public static float GetWinWallet()
    {
        return winWallet;

    }

}
