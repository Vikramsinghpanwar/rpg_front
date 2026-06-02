using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    InGameRecharge inGameRecharge;
    private void Start() {
        inGameRecharge = FindObjectOfType<InGameRecharge>();
    }
    public void AddRecharge()
    {
        inGameRecharge.OpenPanel();
    }

}
