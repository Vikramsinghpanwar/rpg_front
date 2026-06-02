using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class ConnectionCrash : MonoBehaviour
{
    public PlayerCrash csBankRef;
    public AudioSource BGM;


    // Start is called before the first frame update
    private void Start()
    {
        csBankRef = FindAnyObjectByType<PlayerCrash>();
        
    }









    public void Lobby()
    {
        BGM.Stop();
        SceneManager.LoadScene(1);
    }
}
