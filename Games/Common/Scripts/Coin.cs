using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{

    public int val;
    public bool _isSelected;
    // Start is called before the first frame update
    void Start()
    {
        if(val != 10)
        {
            _isSelected = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
