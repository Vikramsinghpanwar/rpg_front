using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PLayerManagerR : MonoBehaviour
{
    public Image p2Img;
    public Image p3Img;
    public Image p4Img;
    public Image p5Img;
    public Image p6Img;
    public TextMeshProUGUI p2Name;
    public TextMeshProUGUI p3Name;
    public TextMeshProUGUI p4Name;
    public TextMeshProUGUI p5Name;
    public TextMeshProUGUI p6Name;


    public Image p2Img2Player;
    public TextMeshProUGUI p2Name2Player;



    // Start is called before the first frame update
    private void Start()
    {
        if(BootValueDecider.rummyPlayerCount == 2)
        {
            PlayerDeclaration2Player();
        }
        else
        {
            PlayerDeclaration();
        }
    }
    public void PlayerDeclaration()
    {
  
        
        List<int> excludedValues = new List<int>();
        int k;
        do
        {
            k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count); 
        }
        while (excludedValues.Contains(k));
        excludedValues.Add(k);
        p2Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
        p2Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;

        do
        {
            k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);
        }
        while (excludedValues.Contains(k));
        excludedValues.Add(k);

        p3Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
        p3Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;


        do
        {
            k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);
        }
        while (excludedValues.Contains(k));
        excludedValues.Add(k);

        p4Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
        p4Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;

        do
        {
            k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);
        }
        while (excludedValues.Contains(k));
        excludedValues.Add(k);

        p5Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
        p5Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;


        do
        {
            k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);
        }
        while (excludedValues.Contains(k));
        excludedValues.Add(k);

        p6Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
        p6Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;

    }



    public void PlayerDeclaration2Player()
    {               
        int k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);        
        p2Img2Player.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
        p2Name2Player.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;  
    }


}
