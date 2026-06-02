using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Selector : MonoBehaviour
{
    public Transform[] buttonObjects;
    GameObject[] activeImagesArray;
    Button[] buttonsArray;
    public int activeIndex = 0;
    void Start()
    {
        activeImagesArray = new GameObject[buttonObjects.Length];
        buttonsArray = new Button[buttonObjects.Length];
        for(int i = 0; i < buttonObjects.Length; i++)
        {
            int index = i;
            activeImagesArray[i] = buttonObjects[i].GetChild(0).gameObject;
            activeImagesArray[i].SetActive(i == 0);
            buttonsArray[i] = buttonObjects[i].GetChild(1).GetComponent<Button>();
            buttonsArray[i].onClick.AddListener(() => Select(index));
        }
    }
    void Select(int index)
    {
        if (activeIndex != index)
        {
            activeImagesArray[activeIndex].SetActive(false);
            activeImagesArray[index].SetActive(true);
            activeIndex = index;
            GameLiveData.instance.roomLength = index + 2;
        }
    }
}
