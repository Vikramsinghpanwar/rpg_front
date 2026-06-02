using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PopulatePlayerCrash: MonoBehaviour
{
    public GameObject imagePrefab;
    public int numberOfImages = 50;
    public List<PlayerDetails> playerDetailList;
    public List<GameObject> pPlayers;
    int k;
    public Text totalBets;
    public ControllerCrash controller;

    private void Start()
    {
        controller = FindAnyObjectByType<ControllerCrash>();
    }

    public IEnumerator TotalBets()
    {
        int tb = 0;
        do
        {
            tb += Random.Range(10, 50);
            totalBets.text = tb.ToString();
            yield return new WaitForSeconds(Random.Range(0, 0.5f));
        }
        while (controller._waitingForNextRound);
    }
    public void Populate()
    {
        StartCoroutine(TotalBets());
        foreach(GameObject g in pPlayers)
        {
            Destroy(g);
        }
        pPlayers.Clear();
        RectTransform contentRectTransform = GetComponent<RectTransform>();
        VerticalLayoutGroup layoutGroup = GetComponent<VerticalLayoutGroup>();

        List<int> exclude = new List<int>();
        List<int> presentPlayers = new List<int>();
        float contentHeight = 0f;


        for (int i = 0; i < numberOfImages; i++)
        {

            do
            {
                k = Random.Range(0, playerDetailList.Count);

            } while (exclude.Contains(k));
            exclude.Add(k);

            GameObject imageGO = Instantiate(imagePrefab, transform);
            Transform imageTransform = imageGO.transform.Find("ImageP").Find("Image");
            pPlayers.Add(imageGO);
            Transform text1Transform = imageGO.transform.Find("Text1");

            Transform text2Transform = imageGO.transform.Find("Text2");

            // Change the values of the text objects
            Text text1 = text1Transform.GetComponent<Text>();
            text1.text = ((char)('a' + Random.Range(0, 26))).ToString() + "*****" + Random.Range(0, 10);

            Text text2 = text2Transform.GetComponent<Text>();
            //text2.text = playerDetailList[k].wallet;            // Optionally, you can set the image's size, sprite, etc.
            text2.text = Random.Range(1000, 10000).ToString("F2");            // Optionally, you can set the image's size, sprite, etc.

            Image image = imageTransform.GetComponent<Image>();
            image.sprite = playerDetailList[k].profileImg;

            contentHeight += layoutGroup.spacing + imagePrefab.GetComponent<RectTransform>().rect.height;

        }

        contentHeight += layoutGroup.padding.top + layoutGroup.padding.bottom;
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentHeight);

    }

    public IEnumerator RandomWin()
    {
        yield return new WaitForSeconds(1f);
        do
        {
            
            int k = Random.Range(0, pPlayers.Count);
            pPlayers[k].transform.Find("GreenImg").GetComponent<Image>().enabled = true;
            float f = float.Parse(pPlayers[k].transform.Find("Text2").GetComponent<Text>().text);
            pPlayers[k].transform.Find("Text3").GetComponent<Text>().text =  (f * controller.s).ToString("F2");
            pPlayers[k].transform.Find("Text4").GetComponent<Text>().text =  controller.s.ToString("F2") + "x";
            
            yield return new WaitForSeconds(Random.Range(0, 3f));
        }
        while (!controller._crash);
    }
}
