using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class FinishPanelRummy2Player : MonoBehaviour
{
    // Start is called before the first frame update
    public PLayerManagerR playerManagerRef;
    public TextMeshProUGUI p1Name;
    public TextMeshProUGUI p2Name;
    public GameObject winImg1;
    public GameObject winImg2;
    string jokerSymbol;
    Sprite jokerSprite;

    public TextMeshProUGUI p1ScoreTxt;
    public TextMeshProUGUI p2ScoreTxt;
    
    public TextMeshProUGUI p1BonusTxt;
    public TextMeshProUGUI p2BonusTxt;
    ConnectionRummy connectionRef;

    public int p1Score;
    public int p2Score;
    public int myScore = 0;

    public float p1Bonus;
    public float p2Bonus;



    public ManagerRummy2Player managerRef;
    public GameObject blockPrefab;
    public GameObject p1cards;
    public GameObject p2cards;

    public InitialCardDistributor2Player initialCardDistributor2Player;
    public List<Sprite> deck = new List<Sprite>();
    private void Start()
    {
        connectionRef = FindObjectOfType<ConnectionRummy>();
        p1Score = 0;
        p2Score = 0;
        jokerSprite = initialCardDistributor2Player.JokerCard.GetComponent<Image>().sprite;
        playerManagerRef = FindObjectOfType<PLayerManagerR>();

        initialCardDistributor2Player = FindObjectOfType<InitialCardDistributor2Player>();
        managerRef = FindObjectOfType<ManagerRummy2Player>();
        deck.Clear();
        p2Name.text = playerManagerRef.p2Name.text;


    }

    bool _isTimOut = false;
    public void TimeOut()
    {

        jokerSymbol = initialCardDistributor2Player.JokerCard.GetComponent<Image>().sprite.name;

        for (int i = 0; i < initialCardDistributor2Player.gameDeck.Count; i++)
        {
            deck.Add(initialCardDistributor2Player.gameDeck[i]);
        }

        _isTimOut = true;
        StartCoroutine(GenerateCardspos1());
        GeneratePlayerCards(2);
        p2Score = 20;
        p2Bonus = p2Score * (BootValueDecider.rummyBootValue);

        p1ScoreTxt.text = p2Score.ToString();
        p1BonusTxt.text = "+" + p2Bonus.ToString("F1");

        p2ScoreTxt.text = "-" + p2Score.ToString();
        p2BonusTxt.text = "-" + p2Bonus.ToString("F1");


       
        p1Name.text = playerManagerRef.p2Name2Player.text;
        p2Name.text = "YOU";

    }

    public void ShowCards(int myscore)
    {
        myScore = myscore;
        jokerSymbol = initialCardDistributor2Player.JokerCard.GetComponent<Image>().sprite.name;

        for (int i = 0; i < initialCardDistributor2Player.gameDeck.Count; i++)
        {
            deck.Add(initialCardDistributor2Player.gameDeck[i]);
        }
        int k;
        List<string> remainingPlayer = new List<string>();
        remainingPlayer.Add(playerManagerRef.p2Name2Player.text);



        if (myscore == 0 && !managerRef._isDeclared)
        {
            // y to kabhi lana nahi h
            p1Name.text = "YOU";
            GeneratePlayerCards(1);
            StartCoroutine(GenerateCardspos2());
            p1Score = myscore;
            p1Bonus= myscore * (BootValueDecider.rummyBootValue);
            p1ScoreTxt.text = p1Score.ToString();
            p1BonusTxt.text = p1Bonus.ToString("F1");
            p2Name.text = playerManagerRef.p2Name2Player.text;
            for (int i = 0; i < p2cards.transform.childCount; i++)
            {
                p2Score += p2cards.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
            }
        }
        else
        {
            // position 2
            if (managerRef._isDeclared)
            {
            StartCoroutine(GenerateCardspos1_declaredByBot());

            }
            else
            StartCoroutine(GenerateCardspos1());

            if (ConnectionRummy._isDroped)
            {
                GenerateDummyCards(2);
                p2Score = 20;
                p2ScoreTxt.text = p2Score.ToString();
                p2Bonus = connectionRef.DropValue;
                p2BonusTxt.text = p2Bonus.ToString("F1");

            }
            else
            {
                GeneratePlayerCards(2);
                p2Score = myscore;
                p2ScoreTxt.text = p2Score.ToString();
                p2Bonus = myscore * (BootValueDecider.rummyBootValue);
                p2BonusTxt.text = p2Bonus.ToString("F1");

                

            }

            p1Name.text = playerManagerRef.p2Name2Player.text;

            p2Name.text = "YOU";


        }
    }


    void GenerateDummyCards(int k)
    {
        Debug.Log("Generating Dummy Cards");
        Transform t = initialCardDistributor2Player.mainBlock.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            GameObject g = null;
            switch (k)
            {

                case 1:
                    g = Instantiate(blockPrefab, p1cards.transform);
                    break;
                case 2:
                    g = Instantiate(blockPrefab, p2cards.transform);
                    break;

            }
            int j;
            for (j = 0; j < t.GetChild(i).transform.GetChild(0).childCount; j++)
            {

                GameObject card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                card.transform.SetParent(g.transform.GetChild(0).transform);

            }
            if (j > 2)
            {
                g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + (j * 20f), g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);

            }
        }
    }

    public IEnumerator GenerateCardspos1()
    {
        yield return new WaitForSeconds(0.2f);
        int l = 0;

        Transform initialTrans = initialCardDistributor2Player.mainBlock.transform;
        for (int i = 0; i < initialTrans.childCount; i++)
        {
            l += initialTrans.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }

        Debug.Log("Generating cards for position 1");
        Debug.Log("user score : " + l);
        List<Sprite> cardToPickFromList = new List<Sprite>();

        for (int i = 0; i < initialCardDistributor2Player.gameDeck.Count + initialCardDistributor2Player.openCardsList.Count - 1; i++)
        {
            if (i >= initialCardDistributor2Player.gameDeck.Count)
            {
                cardToPickFromList.Add(initialCardDistributor2Player.openCardsList[i - initialCardDistributor2Player.gameDeck.Count]);
            }
            else
            {
                cardToPickFromList.Add(initialCardDistributor2Player.gameDeck[i]);
            }
        }


        GameObject card;
        GameObject g;
        //g.transform.SetParent(p1cards.transform);
        int t;




        /////block 1

        g = Instantiate(blockPrefab, p1cards.transform);
        if (l > 100)
        {
            for (int i = 0; i < 3; i++)
            {
                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                Sprite s;
                do
                {
                    t = Random.Range(1, 14);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null);
                card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }
        }
        else
        {
            var seq1 = GetPureSequence(cardToPickFromList, 3);
            if (seq1 != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq1[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq1[i]);

                }
            }

            else
            {
                var iseq = GetSequenceWithJoker(cardToPickFromList);
                if (iseq != null && iseq.Count != 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Debug.Log(iseq[i].name);
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = seq1[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(seq1[i]);

                    }
                }
                else
                {
                    var trail = GetTrail(cardToPickFromList);
                    if (trail != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }
                    }
                }


            }
        }
       








        ///////////////////////block 2

        g = Instantiate(blockPrefab, p1cards.transform);

        if (l > 80)
        {
            for (int i = 0; i < 4; i++)
            {
                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                Sprite s;
                do
                {
                    t = Random.Range(1, 14);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null);
                card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }
        }
        else
        {
            int k = Random.Range(0, 10);////////////////////////////////////////////////////////////////////////////////////////////////0 - 10 krna h
            if (k < 5)
            {


                var iseq = GetSequenceWithJoker(cardToPickFromList, 4);
                if (iseq != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = iseq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(iseq[i]);

                    }
                }
                else
                {

                    var seq2 = GetPureSequence(cardToPickFromList, 4);
                    if (seq2 != null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq2[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq2[i]);

                        }
                    }

                    else
                    {




                        var trail = GetTrail(cardToPickFromList, 4);
                        if (trail != null)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                //trail
                                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                                card.GetComponent<Image>().sprite = trail[i];
                                card.transform.SetParent(g.transform.GetChild(0).transform);
                                cardToPickFromList.Remove(trail[i]);


                            }
                        }
                        else
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                //trail
                                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                                card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                                card.transform.SetParent(g.transform.GetChild(0).transform);
                            }
                        }
                    }



                }

            }
            else
            {
                var seq2 = GetPureSequence(cardToPickFromList, 4);
                if (seq2 != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = seq2[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(seq2[i]);

                    }
                }

                else
                {
                    var iseq = GetSequenceWithJoker(cardToPickFromList, 4);
                    if (iseq != null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = iseq[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(iseq[i]);

                        }
                    }
                    else
                    {
                        var trail = GetTrail(cardToPickFromList, 4);
                        if (trail != null)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                //trail
                                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                                card.GetComponent<Image>().sprite = trail[i];
                                card.transform.SetParent(g.transform.GetChild(0).transform);
                                cardToPickFromList.Remove(trail[i]);


                            }
                        }
                        else
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                //trail
                                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                                card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                                card.transform.SetParent(g.transform.GetChild(0).transform);
                            }
                        }
                    }


                }

            }

        }








        /////////block 3
        g = Instantiate(blockPrefab, p1cards.transform);


        if(l > 50)
        {
            for (int i = 0; i < 3; i++)
            {
                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                Sprite s;
                do
                {
                    t = Random.Range(1, 14);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null);
                card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }
        }
        else
        {
            int k = Random.Range(0, 10);////////////////////////////////////////////////////////////////////////////////////////////////0 - 10 krna h
            if (k < 5)
            {
                var iseq = GetSequenceWithJoker(cardToPickFromList);
                if (iseq != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Debug.Log(iseq[i].name);
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = iseq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(iseq[i]);

                    }
                }
                else
                {
                    var trail = GetTrail(cardToPickFromList);
                    if (trail != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {

                        var seq3 = GetPureSequence(cardToPickFromList, 3);
                        if (seq3 != null)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                //sequenc k liye
                                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                                card.GetComponent<Image>().sprite = seq3[i];
                                card.transform.SetParent(g.transform.GetChild(0).transform);
                                cardToPickFromList.Remove(seq3[i]);

                            }
                        }

                        else
                        {


                            for (int i = 0; i < 3; i++)
                            {
                                //trail
                                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                                card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                                card.transform.SetParent(g.transform.GetChild(0).transform);
                            }


                        }
                    }

                }

            }
            else
            {

                var trail = GetTrail(cardToPickFromList);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {

                    var iseq = GetSequenceWithJoker(cardToPickFromList);
                    if (iseq != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Debug.Log(iseq[i].name);
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = iseq[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(iseq[i]);

                        }
                    }
                    else
                    {
                        var seq3 = GetPureSequence(cardToPickFromList, 3);
                        if (seq3 != null)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                //sequenc k liye
                                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                                card.GetComponent<Image>().sprite = seq3[i];
                                card.transform.SetParent(g.transform.GetChild(0).transform);
                                cardToPickFromList.Remove(seq3[i]);

                            }
                        }

                        else
                        {


                            for (int i = 0; i < 3; i++)
                            {
                                //trail
                                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                                card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                                card.transform.SetParent(g.transform.GetChild(0).transform);
                            }


                        }
                    }



                }








            }
        }




        ////////block 4



        g = Instantiate(blockPrefab, p1cards.transform);

        if (l <= 10)
        {
            var seq = GetPureSequence(cardToPickFromList, 4);
            if (seq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq[i]);

                }
            }

            else
            {
                var trail = GetTrail(cardToPickFromList);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                    }
                }

                

            }
        }


        else if (l <= 20)
        {



            for (int i = 0; i < 3; i++)
            {
                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                Sprite s;
                do
                {
                    t = Random.Range(1, 6);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null); card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }


        }



        else if (l > 20)
        {



            for (int i = 0; i < 3; i++)
            {
                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                Sprite s;
                do
                {
                    t = Random.Range(1, 14);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null);
                card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }
        }

        yield return new WaitForSeconds(0.3f);
        /////////total sum
        ///
        if (!_isTimOut)
        {
            for (int i = 0; i < p1cards.transform.childCount; i++)
            {
                p1Score += p1cards.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
            }
            Debug.Log("idhar text ");
            if (ConnectionRummy._isDroped)
            {
                p1ScoreTxt.text = "20";
                p1Bonus = connectionRef.DropValue;
                p1BonusTxt.text = "+" + p1Bonus.ToString("F1");
            }
            else
            {
                p1ScoreTxt.text = p1Score.ToString();
                p1Bonus = p1Score * BootValueDecider.rummyBootValue;
                p1BonusTxt.text = p1Bonus.ToString("F1");
            }
            
        }
       

        if (ConnectionRummy._isDroped == false)
        { 
            float z = BootValueDecider.rummyBootValue * (l - p1Score);
            Wallet.DeductAmount(z);
            connectionRef.DeductAmount(z);
        }


    }


    public IEnumerator GenerateCardspos1_declaredByBot()
    {
        yield return new WaitForSeconds(0.2f);
        int l = 0;

        Transform initialTrans = initialCardDistributor2Player.mainBlock.transform;
        for (int i = 0; i < initialTrans.childCount; i++)
        {
            l += initialTrans.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }

        Debug.Log("Generating cards for position 1");
        Debug.Log("user score : " + l);
        List<Sprite> cardToPickFromList = new List<Sprite>();

        for (int i = 0; i < initialCardDistributor2Player.gameDeck.Count + initialCardDistributor2Player.openCardsList.Count - 1; i++)
        {
            if (i >= initialCardDistributor2Player.gameDeck.Count)
            {
                cardToPickFromList.Add(initialCardDistributor2Player.openCardsList[i - initialCardDistributor2Player.gameDeck.Count]);
            }
            else
            {
                cardToPickFromList.Add(initialCardDistributor2Player.gameDeck[i]);
            }
        }


        GameObject card;
        GameObject g;
        //g.transform.SetParent(p1cards.transform);
        int t;




        /////block 1

        g = Instantiate(blockPrefab, p1cards.transform);
        var seq1 = GetPureSequence(cardToPickFromList, 3);
        if (seq1 != null)
        {
            for (int i = 0; i < 3; i++)
            {
                //sequenc k liye
                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                card.GetComponent<Image>().sprite = seq1[i];
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(seq1[i]);

            }
        }

        else
        {
            var iseq = GetSequenceWithJoker(cardToPickFromList);
            if (iseq != null && iseq.Count != 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Debug.Log(iseq[i].name);
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq1[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq1[i]);

                }
            }
            else
            {
                var trail = GetTrail(cardToPickFromList);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                    }
                }
            }


        }








        ///////////////////////block 2

        g = Instantiate(blockPrefab, p1cards.transform);

        int k = Random.Range(0, 10);////////////////////////////////////////////////////////////////////////////////////////////////0 - 10 krna h
        if (k < 5)
        {


            var iseq = GetSequenceWithJoker(cardToPickFromList, 4);
            if (iseq != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = iseq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(iseq[i]);

                }
            }
            else
            {

                var seq2 = GetPureSequence(cardToPickFromList, 4);
                if (seq2 != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = seq2[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(seq2[i]);

                    }
                }

                else
                {




                    var trail = GetTrail(cardToPickFromList, 4);
                    if (trail != null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }
                    }
                }



            }

        }
        else
        {
            var seq2 = GetPureSequence(cardToPickFromList, 4);
            if (seq2 != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq2[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq2[i]);

                }
            }

            else
            {
                var iseq = GetSequenceWithJoker(cardToPickFromList, 4);
                if (iseq != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = iseq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(iseq[i]);

                    }
                }
                else
                {
                    var trail = GetTrail(cardToPickFromList, 4);
                    if (trail != null)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }
                    }
                }


            }

        }







        /////////block 3
        g = Instantiate(blockPrefab, p1cards.transform);


        k = Random.Range(0, 10);////////////////////////////////////////////////////////////////////////////////////////////////0 - 10 krna h
        if (k < 5)
        {
            var iseq = GetSequenceWithJoker(cardToPickFromList);
            if (iseq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Debug.Log(iseq[i].name);
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = iseq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(iseq[i]);

                }
            }
            else
            {
                var trail = GetTrail(cardToPickFromList);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {

                    var seq3 = GetPureSequence(cardToPickFromList, 3);
                    if (seq3 != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {


                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }


                    }
                }

            }

        }
        else
        {

            var trail = GetTrail(cardToPickFromList);
            if (trail != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //trail
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = trail[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(trail[i]);


                }
            }
            else
            {

                var iseq = GetSequenceWithJoker(cardToPickFromList);
                if (iseq != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Debug.Log(iseq[i].name);
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = iseq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(iseq[i]);

                    }
                }
                else
                {
                    var seq3 = GetPureSequence(cardToPickFromList, 3);
                    if (seq3 != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {


                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }


                    }
                }



            }








        }



        /////////block 4
        g = Instantiate(blockPrefab, p1cards.transform);
        Debug.Log("declared by bot");

        k = Random.Range(0, 10);////////////////////////////////////////////////////////////////////////////////////////////////0 - 10 krna h
        if (k < 5)
        {
            var iseq = GetSequenceWithJoker(cardToPickFromList);
            if (iseq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Debug.Log(iseq[i].name);
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = iseq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(iseq[i]);

                }
            }
            else
            {
                var trail = GetTrail(cardToPickFromList);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {

                    var seq3 = GetPureSequence(cardToPickFromList, 3);
                    if (seq3 != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {


                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }


                    }
                }

            }

        }
        else
        {

            var trail = GetTrail(cardToPickFromList);
            if (trail != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //trail
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = trail[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(trail[i]);


                }
            }
            else
            {

                var iseq = GetSequenceWithJoker(cardToPickFromList);
                if (iseq != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Debug.Log(iseq[i].name);
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = iseq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(iseq[i]);

                    }
                }
                else
                {
                    var seq3 = GetPureSequence(cardToPickFromList, 3);
                    if (seq3 != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {


                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }


                    }
                }



            }








        }

        p1Score = 0;
        Debug.Log("idhar text ");
        float Bonus = myScore * BootValueDecider.rummyBootValue;


        p1ScoreTxt.text = myScore.ToString();
        p1BonusTxt.text = Bonus.ToString("F1");

        p2ScoreTxt.text = "-" + myScore;
        p2BonusTxt.text = "-" + Bonus.ToString("F1");

        if (ConnectionRummy._isDroped == false)
        {
            float z = BootValueDecider.rummyBootValue * (l - p1Score);
            Wallet.DeductAmount(z);
            connectionRef.DeductAmount(z);
        }
    }



    public IEnumerator GenerateCardspos2()
    {
        int l = initialCardDistributor2Player.totalScore;

        List<Sprite> cardToPickFromList = new List<Sprite>();
        for (int i = 0; i < initialCardDistributor2Player.gameDeck.Count + initialCardDistributor2Player.openCardsList.Count - 1; i++)
        {
            if (i >= initialCardDistributor2Player.gameDeck.Count)
            {
                cardToPickFromList.Add(initialCardDistributor2Player.openCardsList[i - initialCardDistributor2Player.gameDeck.Count]);
            }
            else
            {
                cardToPickFromList.Add(initialCardDistributor2Player.gameDeck[i]);
            }
        }
        GameObject card;
        GameObject g = Instantiate(blockPrefab, p2cards.transform);
        //g.transform.SetParent(p1cards.transform);
        int t;
        var seq1 = GetPureSequence(cardToPickFromList, 3);
        if (seq1 != null)
        {
            for (int i = 0; i < 3; i++)
            {
                //sequenc k liye
                card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                card.GetComponent<Image>().sprite = seq1[i];
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(seq1[i]);

            }
        }

        else
        {
            var trail = GetTrail(cardToPickFromList);
            g = Instantiate(blockPrefab, p2cards.transform);
            if (trail != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //trail
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = trail[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(trail[i]);


                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    //trail
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);


                }
            }

        }



        ///////////////////////block 2

        g = Instantiate(blockPrefab, p2cards.transform);

        int k = Random.Range(0, 10);////////////////////////////////////////////////////////////////////////////////////////////////0 - 10 krna h
        if (k < 5)
        {


            var iseq = GetSequenceWithJoker(cardToPickFromList);
            if (iseq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = iseq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(iseq[i]);

                }
            }
            else
            {

                var seq2 = GetPureSequence(cardToPickFromList, 3);
                if (seq2 != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = seq2[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(seq2[i]);

                    }
                }

                else
                {




                    var trail = GetTrail(cardToPickFromList);
                    if (trail != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }
                    }
                }



            }

        }
        else
        {
            var seq2 = GetPureSequence(cardToPickFromList, 3);
            if (seq2 != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq2[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq2[i]);

                }
            }

            else
            {
                var iseq = GetSequenceWithJoker(cardToPickFromList);
                if (iseq != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = iseq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(iseq[i]);

                    }
                }
                else
                {
                    var trail = GetTrail(cardToPickFromList);
                    if (trail != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }
                    }
                }


            }

        }







        /////////block 3
        g = Instantiate(blockPrefab, p2cards.transform);


        k = Random.Range(0, 10);////////////////////////////////////////////////////////////////////////////////////////////////0 - 10 krna h
        if (k < 5)
        {
            var iseq = GetSequenceWithJoker(cardToPickFromList);
            if (iseq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Debug.Log(iseq[i].name);
                    //sequenc k liye
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = iseq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(iseq[i]);

                }
            }
            else
            {
                var trail = GetTrail(cardToPickFromList);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {

                    var seq3 = GetPureSequence(cardToPickFromList, 3);
                    if (seq3 != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {


                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }


                    }
                }

            }

        }
        else
        {

            var trail = GetTrail(cardToPickFromList);
            if (trail != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //trail
                    card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = trail[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(trail[i]);


                }
            }
            else
            {

                var iseq = GetSequenceWithJoker(cardToPickFromList);
                if (iseq != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Debug.Log(iseq[i].name);
                        //sequenc k liye
                        card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = iseq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(iseq[i]);

                    }
                }
                else
                {
                    var seq3 = GetPureSequence(cardToPickFromList, 3);
                    if (seq3 != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            //sequenc k liye
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {


                        for (int i = 0; i < 3; i++)
                        {
                            //trail
                            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = managerRef.deckStandard[2 + i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                        }


                    }
                }



            }








        }









        g = Instantiate(blockPrefab, p2cards.transform);

        for (int i = 0; i < 4; i++)
        {
            card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
            Sprite s;
            do
            {
                t = Random.Range(1, 14);
                s = GetCardSpriteByRank(cardToPickFromList, t);
            }
            while (s == null);
            card.GetComponent<Image>().sprite = s;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(s);
        }



        /////////total sum
        ///
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < p2cards.transform.childCount; i++)
        {
            p2Score += p2cards.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }
        p2ScoreTxt.text = "-" + p2Score.ToString();
        p1ScoreTxt.text = p2Score.ToString();

        p2Bonus = p2Score * BootValueDecider.rummyBootValue;
        p1BonusTxt.text = "+" + p2Bonus.ToString("F1");
        p2BonusTxt.text = "-" + p2Bonus.ToString("F1");
        float v = (BootValueDecider.rummyBootValue * p2Score);
        Wallet.AddToWinWallet(v);
        connectionRef.WinAmountUpdate(v);
        if (l < p2Score)
        {
            if (p1Name.text != "YOU")
            {
                winImg1.SetActive(false);
                winImg2.SetActive(true);
            }

        }
    }




    public void GeneratePlayerCards(int k)
    {
        Transform t = initialCardDistributor2Player.mainBlock.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            GameObject g = null;
            switch (k)
            {

                case 1:
                    g = Instantiate(blockPrefab, p1cards.transform);
                    break;
                case 2:
                    g = Instantiate(blockPrefab, p2cards.transform);
                    break;

            }
            int j;
            for (j = 0; j < t.GetChild(i).transform.GetChild(0).childCount; j++)
            {

                GameObject card = Instantiate(initialCardDistributor2Player.cardPrefab, g.transform);
                card.GetComponent<Image>().sprite = t.GetChild(i).transform.GetChild(0).transform.GetChild(j).GetComponent<Image>().sprite;
                card.transform.SetParent(g.transform.GetChild(0).transform);

            }
            if (j > 2)
            {
                g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + (j * 20f), g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);

            }
        }
    }







    private readonly List<string> cardRanks = new List<string>
    {
        "a", "2", "3", "4", "5", "6", "7", "8", "9", "0", "j", "q", "k"
    };



    List<Sprite> GetPureSequence(List<Sprite> cards, int seqLength)
    {
        for (int i = 0; i < cards.Count; i++)
        {
        }
        // Group cards by their symbol
        var cardsBySymbol = cards.GroupBy(card => card.name[0]).ToDictionary(g => g.Key, g => g.ToList());

        // Check each group for a pure sequence of exactly four cards
        foreach (var group in cardsBySymbol)
        {
            List<string> ranks = group.Value.Select(card => card.name.Substring(1)).ToList();
            var sequence = FindConsecutiveSequence(group.Value, ranks, seqLength);
            if (sequence != null)
            {
                return sequence;
            }
        }

        return null;
    }

    List<Sprite> FindConsecutiveSequence(List<Sprite> cards, List<string> ranks, int seqLength)
    {
        // Sort cards by their rank values
        var sortedCards = cards.OrderBy(card => GetCardValue(card.name.Substring(1))).ToList();

        List<Sprite> currentSequence = new List<Sprite>();

        for (int i = 0; i < sortedCards.Count; i++)
        {
            if (currentSequence.Count == 0)
            {
                currentSequence.Add(sortedCards[i]);
            }
            else
            {
                // Check if the current rank is consecutive to the last rank in the current sequence
                int lastValue = GetCardValue(currentSequence.Last().name.Substring(1));
                int currentValue = GetCardValue(sortedCards[i].name.Substring(1));

                if (currentValue == lastValue + 1)
                {
                    currentSequence.Add(sortedCards[i]);
                }
                else
                {
                    currentSequence.Clear();
                    currentSequence.Add(sortedCards[i]);
                }
            }

            // Check if the sequence is exactly seqLength cards long
            if (currentSequence.Count == seqLength)
            {
                return new List<Sprite>(currentSequence);
            }
        }

        return null;
    }


    /* List<Sprite> FindConsecutiveSequence(List<Sprite> cards, List<string> ranks, int seqLength)
     {
         // Sort ranks by their order using the GetCardValue function
         List<int> rankValues = ranks.Select(rank => GetCardValue(rank)).OrderBy(value => value).ToList();

         List<Sprite> currentSequence = new List<Sprite>();

         for (int i = 0; i < rankValues.Count; i++)
         {
             if (currentSequence.Count == 0)
             {
                 currentSequence.Add(cards[i]);
             }
             else
             {
                 // Check if the current rank is consecutive to the last rank in the current sequence
                 int lastValue = GetCardValue(currentSequence.Last().name.Substring(1));
                 if (rankValues[i] == lastValue + 1)
                 {

                     currentSequence.Add(cards[i]);
                 }
                 else
                 {
                     currentSequence.Clear();
                     currentSequence.Add(cards[i]);
                 }
             }

             // Check if the sequence is exactly seqLength cards long
             if (currentSequence.Count == seqLength)
             {
                 return new List<Sprite>(currentSequence);
             }
         }

         return null;
     }*/




    /*    List<Sprite> FindConsecutiveSequence(List<Sprite> cards, List<string> ranks, int seqLength)
        {
            // Sort ranks by their order in the cardRanks list
            List<int> rankIndices = ranks.Select(rank => cardRanks.IndexOf(rank)).OrderBy(index => index).ToList();

            List<Sprite> currentSequence = new List<Sprite>();

            for (int i = 0; i < rankIndices.Count; i++)
            {
                if (currentSequence.Count == 0)
                {
                    currentSequence.Add(cards[i]);
                }
                else
                {
                    // Check if the current rank is consecutive to the last rank in the current sequence
                    int lastIndex = cardRanks.IndexOf(currentSequence.Last().name.Substring(1));
                    if (rankIndices[i] == lastIndex + 1)
                    {
                        currentSequence.Add(cards[i]);
                    }
                    else
                    {
                        currentSequence.Clear();
                        currentSequence.Add(cards[i]);
                    }
                }

                // Check if the sequence is exactly four cards long
                if (currentSequence.Count == seqLength)
                {
                    return new List<Sprite>(currentSequence);
                }
            }

            return null;
        }*/

    List<Sprite> GetSequenceWithJoker(List<Sprite> cards, int seqLength = 3)
    {
        List<Sprite> jokerCards = cards.Where(card => card.name == "JJ").ToList();
        string jokerRank = jokerSymbol.Substring(1);

        Debug.Log("joker rank is : " + jokerRank);
        Debug.Log("joker card is : " + jokerSymbol);

        List<Sprite> declaredJokers = cards.Where(card => card.name.Substring(1) == jokerRank && card.name != jokerSymbol).ToList();

        // Add the declared jokers to the jokerCards list

        jokerCards.AddRange(declaredJokers);
       
        List<Sprite> impureSeq = GetPureSequence(cards, seqLength - 1);
        for (int i = 0; i < impureSeq.Count; i++)
        {
            Debug.Log("2 k pair m : " + impureSeq[i].name);
        }
        Debug.Log("joker cards count : " + jokerCards.Count);
        Debug.Log("declared joker cards count : " + declaredJokers.Count);
        if (jokerCards.Count != 0)
        {

            impureSeq.Add(jokerCards[0]);
            jokerCards.RemoveAt(0);
        }
        else if (declaredJokers.Count != 0)
        {
            impureSeq.Add(jokerCards[0]);
            declaredJokers.RemoveAt(0);
        }
        else return null;
        return impureSeq;
    }

    /* List<Sprite> GetSequenceWithJoker(List<Sprite> cards, int seqLength = 3)
     {
         // Group cards by their symbol
         var cardsBySymbol = cards.GroupBy(card => card.name[0]).ToDictionary(g => g.Key, g => g.ToList());

         // Separate the joker cards
         List<Sprite> jokerCards = cards.Where(card => card.name == "JJ").ToList();
         string jokerRank = jokerSymbol.Substring(1);
         List<Sprite> declaredJokers = cards.Where(card => card.name.Substring(1) == jokerRank && card.name != jokerSymbol).ToList();

         // Add the declared jokers to the jokerCards list
         jokerCards.AddRange(declaredJokers);

         // Check each group for a sequence with joker
         foreach (var group in cardsBySymbol)
         {
             List<string> ranks = group.Value.Select(card => card.name.Substring(1)).ToList();
             var sequence = FindConsecutiveSequenceWithJoker(group.Value, ranks, jokerCards, seqLength);
             if (sequence != null)
             {
                 return sequence;
             }
         }

         return null;
     }

     List<Sprite> FindConsecutiveSequenceWithJoker(List<Sprite> cards, List<string> ranks, List<Sprite> jokers, int seqLength)
     {
         ranks = ranks.OrderBy(rank => GetCardValue(rank)).ToList();
         List<Sprite> sequence = new List<Sprite>();
         int jokerCount = jokers.Count;

         for (int i = 0; i < ranks.Count; i++)
         {
             sequence.Clear();
             int neededJokers = 0;

             for (int j = 0; j < seqLength; j++)
             {
                 if (i + j < ranks.Count && (GetCardValue(ranks[i + j]) == GetCardValue(ranks[i]) + j))
                 {
                     sequence.Add(cards[i + j]);
                 }
                 else
                 {
                     neededJokers++;
                     if (neededJokers > jokerCount)
                     {
                         break;
                     }
                 }
             }

             if (sequence.Count + neededJokers == seqLength)
             {
                 sequence.AddRange(jokers.Take(neededJokers));
                 return sequence;
             }
         }

         return null;
     }

 */


    int GetCardValue(string rank)
    {
        switch (rank)
        {
            case "a": return 1;
            case "2": return 2;
            case "3": return 3;
            case "4": return 4;
            case "5": return 5;
            case "6": return 6;
            case "7": return 7;
            case "8": return 8;
            case "9": return 9;
            case "0": return 10;
            case "j": return 11;
            case "q": return 12;
            case "k": return 13;
            default: return 0;
        }
    }




    List<Sprite> GetTrail(List<Sprite> cards, int count = 3)
    {
        // Group cards by their rank
        var cardsByRank = cards.GroupBy(card => card.name.Substring(1)).ToDictionary(g => g.Key, g => g.ToList());

        // Check each group for a trail of exactly three cards
        foreach (var group in cardsByRank)
        {
            if (group.Value.Count >= count)
            {
                return group.Value.Take(3).ToList();
            }
        }

        return null;
    }

    Sprite GetCardSpriteByRank(List<Sprite> cards, int rank)
    {
        // Check if the rank is within the valid range
        if (rank < 1 || rank > 14)
        {
            return null;
        }

        // Convert the rank to its corresponding rank string
        string rankString = rank == 14 ? "a" : rank == 11 ? "j" : rank == 12 ? "q" : rank == 13 ? "k" : rank.ToString();

        // Find the card sprite with the matching rank
        Sprite cardSprite = cards.FirstOrDefault(card => card.name.Substring(1) == rankString);
        return cardSprite;
    }


}
