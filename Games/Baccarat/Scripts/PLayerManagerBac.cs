using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class PLayerManagerBac : MonoBehaviour
{
    public Image p1Img, p2Img, p3Img, p4Img, p5Img, p6Img;
    public Text p1Name, p2Name, p3Name, p4Name, p5Name, p6Name;

    // Start is called before the first frame update
    public Text p1WalletTxt;
    public Text p2WalletTxt;
    public Text p3WalletTxt;
    public Text p4WalletTxt;
    public Text p5WalletTxt;
    public Text p6WalletTxt;

    public int[] playerSeq = new int[] { 0, 0, 0, 0, 0, 0 };
    // Start is called before the first frame update
    public void PlayerDeclaration()
    {
        List<int> excludedValues = new List<int>();

        for (int j = 0; j < 6; j++)
        {
            int k;
            do
            {
                k = Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count);
            }
            while (excludedValues.Contains(k));

            excludedValues.Add(k);
            switch (j)
            {
                case 0:
                    playerSeq[0] = k;

                    p1Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p1Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;
                    p1WalletTxt.text = "₹" + Random.Range(15, 100) + "000";
                    break;
                case 1:
                    playerSeq[1] = k;

                    p2Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p2Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;
                    p2WalletTxt.text = "₹" + Random.Range(15, 100) + "000";

                    break;
                case 2:
                    playerSeq[2] = k;

                    p3Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p3Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;
                    p3WalletTxt.text = "₹" + Random.Range(15, 100) + "000";

                    break;
                case 3:
                    playerSeq[3] = k;

                    p4Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p4Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;
                    p4WalletTxt.text = "₹" + Random.Range(15, 100) + "000";

                    break;

                case 4:
                    playerSeq[4] = k;

                    p5Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p5Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;
                    p5WalletTxt.text = "₹" + Random.Range(15, 100) + "000";

                    break;

                case 5:
                    playerSeq[5] = k;

                    p6Img.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[k];
                    p6Name.text = LoadOnlinePlayers.onlinePlayerSpritesList[k].name;
                    p6WalletTxt.text = "₹" + Random.Range(15, 100) + "000";

                    break;
            }

        }
    }



}