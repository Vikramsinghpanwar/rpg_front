using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject loadingObject;
    public static Loader Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLoading()
    {
        loadingObject.SetActive(true);
    }

    public void HideLoading()
    {
        loadingObject.SetActive(false);
    }


}
