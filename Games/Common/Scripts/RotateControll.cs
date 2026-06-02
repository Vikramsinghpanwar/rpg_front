using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateControll : MonoBehaviour
{
    // Start is called before the first frame update
    public bool portrait = false;
    void Start()
    {
        if (portrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
