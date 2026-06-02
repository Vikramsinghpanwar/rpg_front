using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PLayerManagerBRoullete : MonoBehaviour
{
    public Image p1Img, p2Img, p3Img, p4Img, p5Img, p6Img;
    public TextMeshProUGUI p1Name, p2Name, p3Name, p4Name, p5Name, p6Name;
    // Start is called before the first frame update
    public void PlayerDeclaration()
    {
        List<int> excludedValues = new List<int>();

        for(int j = 0; j<6; j++)
        {
            int k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count );
            do
            {
                k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);
            }
            while (excludedValues.Contains(k));
            excludedValues.Add(k);
            print("k = " + k);
            switch (j)
            {
                case 0:
                    p6Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p6Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;
              
                    break;
                case 1:
                    p1Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p1Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;

                    break;
                case 2:
                    p2Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p2Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;

                    break;
                case 3:
                    p3Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p3Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;

                    break;
                case 4:
                    p4Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p4Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;

                    break;
                case 5:
                    p5Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p5Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;

                    break;
                
            }
           


        }
    }
}
