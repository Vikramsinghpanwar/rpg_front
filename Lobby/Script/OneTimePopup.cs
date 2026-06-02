using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimePopup : MonoBehaviour
{
    public static bool _isFirstTimeLobby;
    public GameObject[] panelsToActivate;
    public GameObject paymentPanel;
    // Start is called before the first frame update
    void Start()
    {
        if(_isFirstTimeLobby != true)
        {
            panelsToActivate[0].SetActive(true);
        }
        

  
    }

    public void PanelClosed(int val)
    {
        if (!_isFirstTimeLobby)
        {
            if(val < panelsToActivate.Length -1)
            {
                panelsToActivate[val + 1].SetActive(true);
            }
            else
            {
                _isFirstTimeLobby = true;
            }
        }
       
    }


}
