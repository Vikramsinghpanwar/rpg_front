using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class ConnectionBRoullete : MonoBehaviour
{
    public ManagerBRoullete csBankRef;
    public float walletAmount;
    public AudioSource BGM;
    



    // Start is called before the first frame update
    private void Start()
    {
        csBankRef = FindAnyObjectByType<ManagerBRoullete>();
    }


    public void Lobby()
    {
        BGM.Stop();

        SceneManager.LoadScene(1);
    }

       public void AddCash()
    {
        BGM.Stop();
        PlayerPrefs.SetInt("_addCash", 1);
        SceneManager.LoadScene(1);
    }




   
}
