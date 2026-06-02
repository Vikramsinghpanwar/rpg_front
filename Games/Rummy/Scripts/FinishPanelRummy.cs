using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class FinishPanelRummy : MonoBehaviour
{
    string jokerSymbol;
    Sprite jokerSprite;
    public ConnectionRummy connectionRef;
    // Start is called before the first frame update
    public PLayerManagerR playerManagerRef;
    public TextMeshProUGUI p1Name;
    public TextMeshProUGUI p2Name;
    public TextMeshProUGUI p3Name;
    public TextMeshProUGUI p4Name;
    public TextMeshProUGUI p5Name;
    public TextMeshProUGUI p6Name;


    public TextMeshProUGUI p1ScoreTxt;
    public TextMeshProUGUI p2ScoreTxt;
    public TextMeshProUGUI p3ScoreTxt;
    public TextMeshProUGUI p4ScoreTxt;
    public TextMeshProUGUI p5ScoreTxt;
    public TextMeshProUGUI p6ScoreTxt;


    public TextMeshProUGUI p1BonusTxt;
    public TextMeshProUGUI p2BonusTxt;
    public TextMeshProUGUI p3BonusTxt;
    public TextMeshProUGUI p4BonusTxt;
    public TextMeshProUGUI p5BonusTxt;
    public TextMeshProUGUI p6BonusTxt;



    public float p1Bonus;
    public int p1Score;
    public int p2Score;
    public int p3Score;
    public int p4Score;
    public int p5Score;
    public int p6Score;


    public ManagerRummy managerRef;
    public GameObject blockPrefab;
    public GameObject p1cards;
    public GameObject p2cards;
    public GameObject p3cards;
    public GameObject p4cards;
    public GameObject p5cards;
    public GameObject p6cards;
    public InitialCardDistributor initCardDistributor;
    public List<Sprite> deck = new List<Sprite>();

    public List<Sprite> cardToPickFromList = new List<Sprite>();


    private void Start()
    {
        p1Score = 0;
        p2Score = 0;
        p3Score = 0;
        p4Score = 0;
        p5Score = 0;
        p6Score = 0;
        playerManagerRef = FindObjectOfType<PLayerManagerR>();

        initCardDistributor = FindObjectOfType<InitialCardDistributor>();
        managerRef = FindObjectOfType<ManagerRummy>();
        deck.Clear();
        jokerSprite = initCardDistributor.JokerCard.GetComponent<Image>().sprite;
        connectionRef = FindObjectOfType<ConnectionRummy>();

    }

    public IEnumerator WinChk(bool _is, int l)
    {
        yield return new WaitForSeconds(1f);
        if (_is)
        {
            float v = BootValueDecider.rummyBootValue * (p2Score + p3Score + p4Score + p5Score + p6Score);
            Wallet.AddToWinWallet(v);
            connectionRef.WinAmountUpdate(v);

        }
        else
        {
            if (ConnectionRummy._isDroped == false)
            {
                float v = (BootValueDecider.rummyBootValue * (myScore));
                Debug.Log("V is " + v);
                Wallet.DeductAmount(v);
                connectionRef.DeductAmount(v);

            }
        }
   

    }

    int myScore = 0;
    public void ShowCards(int myscore)
    {
        myScore = myscore;
        jokerSymbol = initCardDistributor.JokerCard.GetComponent<Image>().sprite.name;

        for (int i = 0; i < initCardDistributor.gameDeck.Count + initCardDistributor.openCardsList.Count - 1; i++)
        {
            if (i >= initCardDistributor.gameDeck.Count)
            {
                cardToPickFromList.Add(initCardDistributor.openCardsList[i - initCardDistributor.gameDeck.Count]);
            }
            else
            {
                cardToPickFromList.Add(initCardDistributor.gameDeck[i]);
            }
        }
        Debug.LogError("Stop ");
        for (int i = 0; i < initCardDistributor.gameDeck.Count; i++)
        {
            deck.Add(initCardDistributor.gameDeck[i]);
        }

        int k;
        List<string> remainingPlayer = new List<string>();
        remainingPlayer.Add(playerManagerRef.p2Name.text);
        remainingPlayer.Add(playerManagerRef.p3Name.text);
        remainingPlayer.Add(playerManagerRef.p4Name.text);
        remainingPlayer.Add(playerManagerRef.p5Name.text);
        remainingPlayer.Add(playerManagerRef.p6Name.text);


        if (myscore == 0 && connectionRef._isFinishedByPlayer)
        {
            // y to kabhi lana nahi h
            p1Name.text = "You";
            StartCoroutine(GenerateCardspos2());
            StartCoroutine(GenerateCardspos3());
            StartCoroutine(GenerateCardspos4());
            k = Random.Range(0, remainingPlayer.Count);
            p3Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);

            k = Random.Range(0, remainingPlayer.Count);
            p2Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);

            GeneratePlayerCards(1);

            p1Score = 0;
            p1ScoreTxt.text = p1Score.ToString();
            StartCoroutine(WinChk(true, myscore));
        }
        else if (myscore < 5)
        {
            // position 2
            p1Bonus += myscore * BootValueDecider.rummyBootValue;
            p1BonusTxt.text = p1Bonus.ToString("F2");
            p1Score += myscore;
            p1ScoreTxt.text = p1Score.ToString();

            p2BonusTxt.text = "-" + (myscore * BootValueDecider.rummyBootValue).ToString("F2");
            GeneratePlayerCards(2);


            StartCoroutine(GenerateCardspos1());
            StartCoroutine(GenerateCardspos3());
            StartCoroutine(GenerateCardspos4());

            k = Random.Range(0, remainingPlayer.Count);
            p1Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);
            p2Name.text = "You";
            p2Score = myscore;
            p2ScoreTxt.text = p2Score.ToString();
            k = Random.Range(0, remainingPlayer.Count);
            p3Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);
            StartCoroutine(WinChk(false, myscore));


        }

        else if (myscore < 30)
        {
            // position 3
            p1Bonus += myscore * BootValueDecider.rummyBootValue;
            p1BonusTxt.text = p1Bonus.ToString("F2");
            p1Score += myscore;
            p1ScoreTxt.text = p1Score.ToString();
            p3BonusTxt.text = "-" + (myscore * BootValueDecider.rummyBootValue).ToString("F2");

            StartCoroutine(GenerateCardspos1());
            StartCoroutine(GenerateCardspos2());
            StartCoroutine(GenerateCardspos4());

            k = Random.Range(0, remainingPlayer.Count);
            p1Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);


            k = Random.Range(0, remainingPlayer.Count);
            p2Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);

            p3Name.text = "You";
            p3Score = myscore;
            p3ScoreTxt.text = p3Score.ToString();
            k = Random.Range(0, remainingPlayer.Count);
            p4Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);

            GeneratePlayerCards(3);

            StartCoroutine(WinChk(false, myscore));


        }
        else
        {
            p4BonusTxt.text = "-" + (myscore * BootValueDecider.rummyBootValue).ToString("F2");

            p1Bonus += myscore * BootValueDecider.rummyBootValue;
            p1BonusTxt.text = p1Bonus.ToString("F2");
            p1Score += myscore;
            p1ScoreTxt.text = p1Score.ToString();
            StartCoroutine(GenerateCardspos1());
            StartCoroutine(GenerateCardspos2());
            StartCoroutine(GenerateCardspos3());
            GeneratePlayerCards(4);

            //pos4


            k = Random.Range(0, remainingPlayer.Count);
            p1Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);


            k = Random.Range(0, remainingPlayer.Count);
            p2Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);

            p4Name.text = "You";
            p4Score = myscore;
            p4ScoreTxt.text = p4Score.ToString();
            k = Random.Range(0, remainingPlayer.Count);
            p3Name.text = remainingPlayer[k];
            remainingPlayer.RemoveAt(k);
            StartCoroutine(WinChk(false, myscore));

        }


        StartCoroutine(GenerateCardspos5());
        StartCoroutine(GenerateCardspos6());







        k = Random.Range(0, remainingPlayer.Count);
        p5Name.text = remainingPlayer[k];
        remainingPlayer.RemoveAt(k);

        k = Random.Range(0, remainingPlayer.Count);
        p6Name.text = remainingPlayer[k];
        remainingPlayer.RemoveAt(k);
    }

  

    public IEnumerator GenerateCardspos1()
    {
        Debug.LogError("Stop ");

        yield return new WaitForSeconds(0.2f);
        int l = 0;

        Transform initialTrans = initCardDistributor.mainBlock.transform;
        for (int i = 0; i < initialTrans.childCount; i++)
        {
            l += initialTrans.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }

        Debug.Log("Generating cards for position 1");
        Debug.Log("user score : " + l);

    


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
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = s;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {


                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {


                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
                        }


                    }
                }



            }








        }



        /////////block 4
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {

                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
                        }
                    }
                }



            }








        }

        p1Score = 0;
        Debug.Log("idhar text ");
        p1ScoreTxt.text = p1Score.ToString();

        /*if (ConnectionRummy._isDroped == false)
        {
            float z = BootValueDecider.rummyBootValue * (l - p1Score);
            Wallet.DeductAmount(z);
            //connectionRef.DeductAmount(z);
        }*/

    }


    public IEnumerator GenerateCardspos2()
    {
        Debug.LogError("Stop ");

        yield return new WaitForSeconds(0.2f);
        int l = 0;

        Transform initialTrans = initCardDistributor.mainBlock.transform;
        for (int i = 0; i < initialTrans.childCount; i++)
        {
            l += initialTrans.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }

        Debug.Log("Generating cards for position 1");
        Debug.Log("user score : " + l);
  

        GameObject card;
        GameObject g;
        //g.transform.SetParent(p2cards.transform);
        int t;




        /////block 1

        g = Instantiate(blockPrefab, p2cards.transform);
        var seq1 = GetPureSequence(cardToPickFromList, 3);
        if (seq1 != null)
        {
            for (int i = 0; i < 3; i++)
            {
                //sequenc k liye
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = s;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(s);
                    }
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = seq3[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(seq3[i]);

                        }
                    }

                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
                        }

                    }
                }



            }








        }








        g = Instantiate(blockPrefab, p2cards.transform);

        if(l < 2)
        {

            for (int i = 0; i < 3; i++)
            {
                int tt = Random.Range(0, deck.Count);
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                card.GetComponent<Image>().sprite = deck[tt];
                card.transform.SetParent(g.transform.GetChild(0).transform);


            }

            g = Instantiate(blockPrefab, p2cards.transform);

            card = Instantiate(initCardDistributor.cardPrefab, g.transform);

            Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
            if (s == null)
            {
                s = GetCardSpriteByRank(cardToPickFromList, 3);
            }
            card.GetComponent<Image>().sprite = s;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(s);
        }
        else if (l == 2)
        {
            var seq = GetPureSequence(cardToPickFromList, 4);
            if (seq != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                    for (int i = 0; i < 4; i++)
                    {
                        //trail
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }

                    g = Instantiate(blockPrefab, p2cards.transform);

                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                    Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
                    if (s == null)
                    {
                        s = GetCardSpriteByRank(cardToPickFromList, 3);
                    }
                    card.GetComponent<Image>().sprite = s;
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(s);
                }

              

            }
        }
        else if (l < 5)
        {
            var seq = GetPureSequence(cardToPickFromList, 3);
            if (seq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq[i]);

                }
            }

            else
            {
                var trail = GetTrail(cardToPickFromList, 4);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }
                }

                g = Instantiate(blockPrefab, p2cards.transform);

                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
                if (s == null)
                {
                    s = GetCardSpriteByRank(cardToPickFromList, 3);
                }
                card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);

            }
        }

        else if (l <= 30)
        {

            var trail = GetTrail(cardToPickFromList);
            if (trail != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //trail
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = trail[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(trail[i]);


                }
            }
            else
            {
                var seq = GetPureSequence(cardToPickFromList, 3);
                if (seq != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //sequence
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = seq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(seq[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }
                }



            }


            g = Instantiate(blockPrefab, p2cards.transform);


            card = Instantiate(initCardDistributor.cardPrefab, g.transform);

            Sprite s;
            do
            {
                t = Random.Range(1, l - 2);
                s = GetCardSpriteByRank(cardToPickFromList, t);

            }
            while (s == null);
            card.GetComponent<Image>().sprite = s;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(s);
        }
        else if (l > 30 && l <= 50)
        {



            for (int i = 0; i < 4; i++)
            {
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s;
                do
                {
                    t = Random.Range(1, 8);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null); card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }


        }


        else if (l > 50 && l <= 70)
        {



            for (int i = 0; i < 4; i++)
            {
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s;
                do
                {
                    t = Random.Range(1, 12);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null); card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }


        }

        else if (l > 70)
        {



            for (int i = 0; i < 4; i++)
            {
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
        for (int i = 0; i < p2cards.transform.childCount; i++)
        {
            p2Score += p2cards.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }
        p2ScoreTxt.text = p2Score.ToString();
        p2BonusTxt.text = "-" + (p2Score * BootValueDecider.rummyBootValue).ToString("F2");
        p1Bonus += p2Score * BootValueDecider.rummyBootValue;
        p1BonusTxt.text = "+" + p1Bonus.ToString("F2");
        p1Score += p2Score;
        p1ScoreTxt.text = p1Score.ToString();
    }


    public IEnumerator GenerateCardspos3()
    {
        Debug.LogError("Stop ");

        bool _is4 = false;
        yield return new WaitForSeconds(0.2f);
        int l = 0;

        Transform initialTrans = initCardDistributor.mainBlock.transform;
        for (int i = 0; i < initialTrans.childCount; i++)
        {
            l += initialTrans.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }

        Debug.Log("Generating cards for position 1");
        Debug.Log("user score : " + l);



        GameObject card;
        GameObject g;
        //g.transform.SetParent(p3cards.transform);
        int t;




        /////block 1

        g = Instantiate(blockPrefab, p3cards.transform);
        var seq1 = GetPureSequence(cardToPickFromList, 3);
        if (seq1 != null)
        {
            for (int i = 0; i < 3; i++)
            {
                //sequenc k liye
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = s;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(s);
                    }
                }
            }


        }








        ///////////////////////block 2

        g = Instantiate(blockPrefab, p3cards.transform);

        int k = Random.Range(0, 10);////////////////////////////////////////////////////////////////////////////////////////////////0 - 10 krna h
        if (k < 5)
        {


            var iseq = GetSequenceWithJoker(cardToPickFromList);
            if (iseq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
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
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = trail[i];
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(trail[i]);


                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Sprite s = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                            card.GetComponent<Image>().sprite = s;
                            card.transform.SetParent(g.transform.GetChild(0).transform);
                            cardToPickFromList.Remove(s);
                        }
                    }
                }


            }

        }







        /////////block 3
        g = Instantiate(blockPrefab, p3cards.transform);

        if (l == 2)
        {
            var seq = GetPureSequence(cardToPickFromList, 4);
            if (seq != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    _is4 = true;
                    //sequenc k liye
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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
                    for (int i = 0; i < 4; i++)
                    {
                        _is4 = true;
                        //trail
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }

                    g = Instantiate(blockPrefab, p3cards.transform);

                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                    Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
                    if (s == null)
                    {
                        s = GetCardSpriteByRank(cardToPickFromList, 3);
                    }
                    card.GetComponent<Image>().sprite = s;
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(s);
                }

               
            }
        }
        else if (l < 5)
        {
            var seq = GetPureSequence(cardToPickFromList, 3);
            if (seq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq[i]);

                }
            }

            else
            {
                var trail = GetTrail(cardToPickFromList, 3);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }
                }

                g = Instantiate(blockPrefab, p3cards.transform);

                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
                if (s == null)
                {
                    s = GetCardSpriteByRank(cardToPickFromList, 3);
                }
                card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);

            }
        }

        else if (l <= 30)
        {
            var trail = GetTrail(cardToPickFromList);
            if (trail != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //trail
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = trail[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(trail[i]);


                }
            }
            else
            {
                var seq = GetPureSequence(cardToPickFromList, 3);
                if (seq != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //sequence
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = seq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(seq[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }
                }



            }


            g = Instantiate(blockPrefab, p3cards.transform);


            card = Instantiate(initCardDistributor.cardPrefab, g.transform);

            Sprite s;
            do
            {
                t = Random.Range(1, l - 2);
                s = GetCardSpriteByRank(cardToPickFromList, t);

            }
            while (s == null);
            card.GetComponent<Image>().sprite = s;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(s);
        }



        else if (l > 30 && l <= 50)
        {

            _is4 = true;

            for (int i = 0; i < 4; i++)
            {
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s;
                do
                {
                    t = Random.Range(1, 8);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null); card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }


        }


        else if (l > 50 && l <= 70)
        {


            _is4 = true;

            for (int i = 0; i < 4; i++)
            {
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s;
                do
                {
                    t = Random.Range(1, 12);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null); card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }


        }

        else if (l > 70)
        {



            for (int i = 0; i < 4; i++)
            {
                _is4 = true;
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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


        ////////////////////from block 4



        g = Instantiate(blockPrefab, p3cards.transform);


        if (l < 2)
        {

            for (int i = 0; i < 3; i++)
            {
                int tt = Random.Range(0, deck.Count);
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                card.GetComponent<Image>().sprite = deck[tt];
                card.transform.SetParent(g.transform.GetChild(0).transform);


            }
            g = Instantiate(blockPrefab, p3cards.transform);

            card = Instantiate(initCardDistributor.cardPrefab, g.transform);

            Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
            if (s == null)
            {
                s = GetCardSpriteByRank(cardToPickFromList, 3);
            }
            card.GetComponent<Image>().sprite = s;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(s);
        }
        else if (l == 2)
        {
            int tmpp = (_is4) ? 3 : 4;

            var seq = GetPureSequence(cardToPickFromList, tmpp);
            if (seq != null)
            {
                for (int i = 0; i < tmpp; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq[i]);

                }

                g = Instantiate(blockPrefab, p3cards.transform);

                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
                if (s == null)
                {
                    s = GetCardSpriteByRank(cardToPickFromList, 3);
                }
                card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);

            }

            else
            {
                var trail = GetTrail(cardToPickFromList);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }
                }
                if (!_is4)
                {

                    g = Instantiate(blockPrefab, p3cards.transform);

                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                    Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
                    if (s == null)
                    {
                        s = GetCardSpriteByRank(cardToPickFromList, 3);
                    }
                    card.GetComponent<Image>().sprite = s;
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(s);

                }

            }
        }
        else if (l < 5)
        {
            var seq = GetPureSequence(cardToPickFromList, 3);
            if (seq != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //sequenc k liye
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = seq[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(seq[i]);

                }
            }

            else
            {
                int tmpp = (_is4) ? 3 : 4;

                var trail = GetTrail(cardToPickFromList, tmpp);
                if (trail != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //trail
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = trail[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(trail[i]);


                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }
                }

                g = Instantiate(blockPrefab, p3cards.transform);

                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s = GetCardSpriteByRank(cardToPickFromList, 2);
                if (s == null)
                {
                    s = GetCardSpriteByRank(cardToPickFromList, 3);
                }
                card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);

            }
        }

        else if (l <= 30)
        {

            var trail = GetTrail(cardToPickFromList);
            if (trail != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    //trail
                    card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                    card.GetComponent<Image>().sprite = trail[i];
                    card.transform.SetParent(g.transform.GetChild(0).transform);
                    cardToPickFromList.Remove(trail[i]);


                }
            }
            else
            {
                var seq = GetPureSequence(cardToPickFromList, 3);
                if (seq != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        //sequence
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = seq[i];
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(seq[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
                        card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                        card.GetComponent<Image>().sprite = ss;
                        card.transform.SetParent(g.transform.GetChild(0).transform);
                        cardToPickFromList.Remove(ss);
                    }
                }



            }


            g = Instantiate(blockPrefab, p3cards.transform);


            card = Instantiate(initCardDistributor.cardPrefab, g.transform);

            Sprite s;
            do
            {
                t = Random.Range(1, l - 2);
                s = GetCardSpriteByRank(cardToPickFromList, t);

            }
            while (s == null);
            card.GetComponent<Image>().sprite = s;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(s);
        }
        else if (l > 30 && l <= 50)
        {
            for (int i = 0; i < 3; i++)
            {
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s;
                do
                {
                    t = Random.Range(1, 8);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null); card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }


        }


        else if (l > 50 && l <= 70)
        {


            int tmpp = (_is4) ? 3 : 4;

            for (int i = 0; i < tmpp; i++)
            {
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);

                Sprite s;
                do
                {
                    t = Random.Range(1, 12);
                    s = GetCardSpriteByRank(cardToPickFromList, t);
                }
                while (s == null); card.GetComponent<Image>().sprite = s;
                card.transform.SetParent(g.transform.GetChild(0).transform);
                cardToPickFromList.Remove(s);
            }
        }

        else if (l > 70)
        {


            int tmpp = (_is4) ? 3 : 4;

            for (int i = 0; i < tmpp; i++)
            {
                card = Instantiate(initCardDistributor.cardPrefab, g.transform);
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

        yield return new WaitForSeconds(0.2f);
        /////////total sum
        ///
        for (int i = 0; i < p3cards.transform.childCount; i++)
        {
            p3Score += p3cards.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }
        p3ScoreTxt.text = p3Score.ToString();
        p3BonusTxt.text = "-" + (p3Score * BootValueDecider.rummyBootValue).ToString("F2");
        p1Bonus += p3Score * BootValueDecider.rummyBootValue;
        p1BonusTxt.text = "+" + p1Bonus.ToString("F2");
        p1Score += p3Score;
        p1ScoreTxt.text = p1Score.ToString();
    }




    public IEnumerator GenerateCardspos4()
    {
        Debug.LogError("Stop ");

        GameObject card;
        GameObject g = Instantiate(blockPrefab, p4cards.transform);
        int t = Random.Range(0, 9);
        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);


        }




        g = Instantiate(blockPrefab, p4cards.transform);
        g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + 20f, g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);

        for (int i = 0; i < 4; i++)
        {


            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);


        }


        g = Instantiate(blockPrefab, p4cards.transform);

        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);

        }


        g = Instantiate(blockPrefab, p4cards.transform);

        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }

        p4cards.transform.GetChild(0).GetComponent<CardsChecker>().CheckforCardsChange();

        yield return new WaitForSeconds(0.5f);
        /////////total sum
        ///
        for (int i = 0; i < p4cards.transform.childCount; i++)
        {
            p4Score += p4cards.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }
        p4ScoreTxt.text = p4Score.ToString();
        p4BonusTxt.text = "-" + (p4Score * BootValueDecider.rummyBootValue).ToString("F2");
        p1Bonus += p4Score * BootValueDecider.rummyBootValue;
        p1BonusTxt.text = "+" + p1Bonus.ToString("F2");
        p1Score += p4Score;
        p1ScoreTxt.text = p1Score.ToString();
    }





    public IEnumerator GenerateCardspos5()
    {
        Debug.LogError("Stop ");

        GameObject card;
        GameObject g = Instantiate(blockPrefab, p5cards.transform);
        //g.transform.SetParent(p1cards.transform);
        int t = Random.Range(0, 9);
        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }





        g = Instantiate(blockPrefab, p5cards.transform);
        g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + 20f, g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);

        for (int i = 0; i < 4; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }


        g = Instantiate(blockPrefab, p5cards.transform);

        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }

        g = Instantiate(blockPrefab, p5cards.transform);

        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }
        p5cards.transform.GetChild(0).GetComponent<CardsChecker>().CheckforCardsChange();

        yield return new WaitForSeconds(0.5f);
        /////////total sum
        ///
        for (int i = 0; i < p5cards.transform.childCount; i++)
        {
            p5Score += p5cards.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }
        p5ScoreTxt.text = p5Score.ToString();
        p5BonusTxt.text = "-" + (p5Score * BootValueDecider.rummyBootValue).ToString("F2");
        p1Bonus += p5Score * BootValueDecider.rummyBootValue;
        p1BonusTxt.text = "+" + p1Bonus.ToString("F2");
        p1Score += p5Score;
        p1ScoreTxt.text = p1Score.ToString();
    }





    public IEnumerator GenerateCardspos6()
    {
        GameObject card;
        GameObject g;
        //g.transform.SetParent(p1cards.transform);


        g = Instantiate(blockPrefab, p6cards.transform);
        g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + 20f, g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);
        for (int i = 0; i < 4; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }

        g = Instantiate(blockPrefab, p6cards.transform);
        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }

        g = Instantiate(blockPrefab, p6cards.transform);
        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }

        g = Instantiate(blockPrefab, p6cards.transform);
        for (int i = 0; i < 3; i++)
        {
            card = Instantiate(initCardDistributor.cardPrefab, g.transform);
            Sprite ss = cardToPickFromList[Random.Range(0, cardToPickFromList.Count)];
            card.GetComponent<Image>().sprite = ss;
            card.transform.SetParent(g.transform.GetChild(0).transform);
            cardToPickFromList.Remove(ss);
        }

        p6cards.transform.GetChild(0).GetComponent<CardsChecker>().CheckforCardsChange();

        yield return new WaitForSeconds(0.5f);

        /////////total sum
        ///
        for (int i = 0; i < p6cards.transform.childCount; i++)
        {
            p6Score += p6cards.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }
        p6ScoreTxt.text = p6Score.ToString();
        p6BonusTxt.text = "-" + (p6Score * BootValueDecider.rummyBootValue).ToString("F2");
        p1Bonus += p6Score * BootValueDecider.rummyBootValue;
        p1BonusTxt.text = "+" + p1Bonus.ToString("F2");
        p1Score += p6Score;
        p1ScoreTxt.text = p1Score.ToString();

    }


    public void GeneratePlayerCards(int k)
    {
        Transform t = initCardDistributor.mainBlock.transform;
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
                case 3:
                    g = Instantiate(blockPrefab, p3cards.transform);
                    break;
                case 4:
                    g = Instantiate(blockPrefab, p4cards.transform);
                    break;
            }
            int j;
            for (j = 0; j < t.GetChild(i).transform.GetChild(0).childCount; j++)
            {

                GameObject card = Instantiate(initCardDistributor.cardPrefab, g.transform);
                card.GetComponent<Image>().sprite = t.GetChild(i).transform.GetChild(0).transform.GetChild(j).GetComponent<Image>().sprite;
                card.transform.SetParent(g.transform.GetChild(0).transform);

            }
            if (j > 2)
            {
                g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x + (j * 20f), g.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y);

            }
        }
    }

















    List<Sprite> GetPureSequence(List<Sprite> cards, int seqLength)
    {
       
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



    List<Sprite> GetSequenceWithJoker(List<Sprite> cards, int seqLength = 3)
    {

        List<Sprite> jokerCards = cards.Where(card => card.name == "JJ").ToList();
        string jokerRank = jokerSymbol.Substring(1);
        List<Sprite> declaredJokers = cards.Where(card => card.name.Substring(1) == jokerRank && card.name != jokerSymbol).ToList();
        jokerCards.AddRange(declaredJokers);

        List<Sprite> impureSeq = GetPureSequence(cards, seqLength - 1);
        
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



    // old trail for single deck
    /*    List<Sprite> GetTrail(List<Sprite> cards, int count = 3)
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
        }*/


    List<Sprite> GetTrail(List<Sprite> cards, int count = 3)
    {
        // Group cards by their rank
        var cardsByRank = cards.GroupBy(card => card.name.Substring(1)).ToDictionary(g => g.Key, g => g.ToList());

        // Check each group for a trail of exactly three cards from different decks
        foreach (var group in cardsByRank)
        {
            var cardList = group.Value;

            // Create a dictionary to track the cards by their name and deck
            var uniqueCards = new Dictionary<string, List<Sprite>>();
            foreach (var card in cardList)
            {
                // Extract the deck identifier from the card name (assuming card name has deck info)
                var deckIdentifier = card.name.Substring(0, 1); // Modify this based on your naming convention

                if (!uniqueCards.ContainsKey(deckIdentifier))
                {
                    uniqueCards[deckIdentifier] = new List<Sprite>();
                }
                uniqueCards[deckIdentifier].Add(card);
            }

            // Check if we have enough unique cards from different decks
            if (uniqueCards.Keys.Count >= count)
            {
                var result = new List<Sprite>();
                foreach (var deck in uniqueCards.Keys.Take(count))
                {
                    result.Add(uniqueCards[deck].First());
                }
                return result;
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
