using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class CarRoulleteConnection : MonoBehaviour
{
    public CarRoulleteManager csBankRef;
    public AudioSource BGM;
    public void Lobby()
    {
        BGM.Stop();
        SceneManager.LoadScene(1);
    }


}
