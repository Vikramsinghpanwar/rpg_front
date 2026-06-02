using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
        public InGameRecharge inGameRechargeRef;


        private void Start()
    {
        inGameRechargeRef = FindFirstObjectByType<InGameRecharge>();
        Application.targetFrameRate = 200;
    }
   public void Lobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void AddCash()
    {
        inGameRechargeRef.OpenPanel();
    }
}
