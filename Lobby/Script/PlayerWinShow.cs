using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerWinShow : MonoBehaviour
{
    public Animator ImgAnim;
    public TextMeshProUGUI winAmnt;
    public TextMeshProUGUI game;
    public TextMeshProUGUI playerName;

    public Image profileImg;
    public List<string> gameNameList;
    public List<Sprite> playerPic;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AnimStart());
    }

    IEnumerator AnimStart()
    {
        if(Random.Range(0, 3) == 0)
        {
            profileImg.sprite = playerPic[Random.Range(0, playerPic.Count)];
        }
        else profileImg.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[Random.Range(0, LoadOnlinePlayers.onlinePlayerSpritesList.Count)];
        winAmnt.text = "Rs." + Random.Range(10000, 100000);
        game.text = gameNameList[Random.Range(0, gameNameList.Count)];
        playerName.text =  "User" + Random.Range(999, 9999);
        ImgAnim.SetBool("_is",true);
        yield return new WaitForSeconds(4f);
        ImgAnim.SetBool("_is", false);
        yield return new WaitForSeconds(1f);
        StartCoroutine(AnimStart());

    }
}
