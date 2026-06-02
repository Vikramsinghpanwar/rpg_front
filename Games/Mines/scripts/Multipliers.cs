using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multipliers : MonoBehaviour
{
    // Start is called before the first frame update

    public List<float> rewardMultipliers;

    public void UpdateMultipliers(int minesCount)
    {
        rewardMultipliers.Clear();
        for(int i = 0; i<(25 - minesCount); i++)
        {
            rewardMultipliers.Add(CalculateReturnMultiplier(minesCount, 25 - i));
        }

    }


    public static float CalculateReturnMultiplier(int mines, int uncoveredTiles)
    {
        // Base values for different mine counts (adjust these based on your specific data)
        float initialMultiplier;
        float growthRate;

        if (mines % 2 == 0)
        {
            initialMultiplier = 1.05f;
        }
        else
        {
            initialMultiplier = 1.1f;
        }
        growthRate = mines * 0.05f;


        // Calculate the return multiplier using an exponential growth model
        float returnMultiplier = initialMultiplier * (float)Math.Pow(1 + growthRate, uncoveredTiles);

        return returnMultiplier;
    }


}
