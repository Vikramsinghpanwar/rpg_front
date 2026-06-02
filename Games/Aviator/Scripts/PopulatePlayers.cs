using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PopulatePlayer: MonoBehaviour
{
    public GameObject imagePrefab;
    public List<Sprite> playerDetailList;
    public List<GameObject> pPlayers;
    public Text totalBets;
    public Controller controller;
    public int[] bets = new int[] { 100, 500, 1000, 2000, 5000, 4500, 3500, 3000, 2500, 1500, 8000, 10000, 14000, 13000, 25000, 24000, 12000};
    GridLayoutGroup gridRef;
    public GridLayoutGroup gridRef_mybet;

    private List<botDataAviator> botDataAviatorList;
    private void Start()
    {
        gridRef = gameObject.GetComponent<GridLayoutGroup>();
        playerDetailList = new List<Sprite>();
        for (int i = 0; i< LoadOnlinePlayers.onlinePlayerSpritesList.Count; i++)
        {
            playerDetailList.Add(LoadOnlinePlayers.onlinePlayerSpritesList[i]);
        }
        controller = FindAnyObjectByType<Controller>();
        gridRef.cellSize = new Vector2(gameObject.GetComponentInParent<RectTransform>().rect.width, 60f);
        gridRef_mybet.cellSize = new Vector2(gameObject.GetComponentInParent<RectTransform>().rect.width, 60f);
    }

    public void TotalBets(int totalbets)
    {
        totalBets.text = totalbets.ToString();
    }
    public void Populate(List<botDataAviator> botsArray, double roundStartTime)
    {
        botDataAviatorList = new List<botDataAviator>();
        foreach(botDataAviator b in botsArray)
        {
            botDataAviatorList.Add(b);
        }
        foreach (GameObject g in pPlayers)
        {
            Destroy(g);
        }
        pPlayers.Clear();
        RectTransform contentRectTransform = GetComponent<RectTransform>();
        VerticalLayoutGroup layoutGroup = GetComponent<VerticalLayoutGroup>();

        List<int> exclude = new List<int>();
        List<int> presentPlayers = new List<int>();
       


        for (int i = 0; i < botsArray.Count; i++)
        {

          

            GameObject imageGO = Instantiate(imagePrefab, transform);
            Transform imageTransform = imageGO.transform.Find("ImageP").Find("Image");
            pPlayers.Add(imageGO);
            Transform nameText_Transform = imageGO.transform.Find("nameText");

            Transform betText_Transform = imageGO.transform.Find("betText");

            Text nameText = nameText_Transform.GetComponent<Text>();

            Text betText = betText_Transform.GetComponent<Text>();
            int prob = Random.Range(0, 10);
            betText.text = botsArray[i].botBetAmount.ToString("F2");
            Image image = imageTransform.GetComponent<Image>();
            image.sprite = LoadOnlinePlayers.onlinePlayerSpritesList[botsArray[i].botId];
            nameText.text = botsArray[i].botName;         
        }


    }

    public IEnumerator botWin()
    {
        List<int> toExcludeList = new List<int>();
        do
        {
            if(pPlayers.Count < 1)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            for(int i = 0; i < botDataAviatorList.Count; i++)
            {
                if (toExcludeList.Contains(i))
                {
                    continue;
                }
                if (controller.s >=botDataAviatorList[i].botCashOutMultiplier)
                {
                    toExcludeList.Add(i);
                    pPlayers[i].transform.Find("GreenImg").GetComponent<Image>().enabled = true;
                    float f = float.Parse(pPlayers[i].transform.Find("betText").GetComponent<Text>().text);
                    pPlayers[i].transform.Find("Text3").GetComponent<Text>().text = (f * botDataAviatorList[i].botCashOutMultiplier).ToString("F2");
                    pPlayers[i].transform.Find("Text4").GetComponent<Text>().text = botDataAviatorList[i].botCashOutMultiplier.ToString("F2") + "x";
                }
            }
           
            
            yield return new WaitForSeconds(0.5f);
        }
        while (!controller._crash || controller.gamePhase != "Betting");
    }
}
