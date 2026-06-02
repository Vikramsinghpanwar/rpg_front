using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class IndianSystemMoney : MonoBehaviour
{
    public static string FormatNumberToIndianSystem(int number)
    {
        string numberStr = number.ToString(CultureInfo.InvariantCulture);
        int len = numberStr.Length;

        if (len <= 3)
            return numberStr;

        string result = string.Empty;
        int counter = 0;

        for (int i = len - 1; i >= 0; i--)
        {
            result = numberStr[i] + result;
            counter++;

            if (counter == 3 && i > 0)
            {
                result = ',' + result;
                counter = 0;
            }
            else if (counter == 2 && i > 1)
            {
                result = ',' + result;
                counter = 0;
            }
        }

        return result;
    }
}
