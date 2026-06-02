using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InitialCardDistributor2Player : MonoBehaviour
{
    public Image declareCardImg;
    public GameObject declarePanel;
    public List<Sprite> openCardsList;
    public GameObject initialBlocker;
    public int totalScore;
    public TextMeshProUGUI myScoreTxt;
    public GameObject cardPrefab;
    public List<GameObject> cardsObject;
    public List<Sprite> cardsSprite_Sorted;
    public List<Sprite> cardSprites;
    public Transform posTransforms;
    public Transform originTrnsf;
    public float PlayerCardsOffset = 50f;
    float middlePos;
    public GameObject mainBlock;
    public GameObject blockPrefab;
    List<int> spade = new List<int>();
    List<int> club = new List<int>();
    List<int> heart = new List<int>();
    List<int> diamond = new List<int>();
    public int[] cardsSeqArray = new int[] { 0, 0, 0, 0 };
    public ManagerRummy2Player managerRef;
    public AlignchildrenToCenter alignChildrenRef;
    public GameObject closedCardPrefab;
    public GameObject openCard;
    public GameObject closedCard;
    public GameObject JokerCard;
    public GameObject dropPanel;
    public Sprite jokerSprite;
    public List<Sprite> gameDeck;
    // Start is called before the first frame update
    public int finishAfterRound = 5;

    public void ScoreUpdate()
    {
        totalScore = 0;
        for (int i = 0; i < mainBlock.transform.childCount; i++)
        {
            totalScore += mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
        }
        myScoreTxt.text = "<color=yellow>" + totalScore.ToString() + "</color>";
    }

    public void CheckForJoker()
    {
        foreach (GameObject cardstr in cardsObject)
        {
            string s = cardstr.GetComponent<Image>().sprite.name;
            if (s.Substring(1) == ConnectionRummy.jokerRank)
            {
                cardstr.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    void Start()
    {
        declareCardImg.gameObject.SetActive(false);
        declarePanel.SetActive(false);
        PlayerCardsOffset = (Screen.width * 0.7f) / 13;
        myScoreTxt.text = "";
        dropPanel.SetActive(false);
        initialBlocker.SetActive(true);
        managerRef = FindObjectOfType<ManagerRummy2Player>();

        gameDeck = new List<Sprite>();
        for (int i = 0; i < managerRef.deckStandard.Count; i++)
        {
            gameDeck.Add(managerRef.deckStandard[i]);
        }
        //setting random cards here
        cardSprites.Clear();
        for (int a = 0; a < 13; a++)
        {
            Sprite spriteK = gameDeck[Random.Range(0, gameDeck.Count)];
            cardSprites.Add(spriteK);
            gameDeck.Remove(spriteK);
        }


        for (int i = 0; i < 3; i++)
        {
            if (i == 1)
            {
                closedCard = Instantiate(closedCardPrefab, originTrnsf);
                closedCard.transform.position = originTrnsf.position;
                closedCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                closedCard.transform.parent = managerRef.closedDeck.transform;
            }
            else if (i == 0)
            {
                JokerCard = Instantiate(cardPrefab, originTrnsf);
                JokerCard.transform.position = originTrnsf.position;
                JokerCard.GetComponent<DraggableItem>().enabled = false;
                JokerCard.GetComponent<Button>().enabled = false;
                JokerCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                JokerCard.transform.parent = managerRef.closedDeck.transform;
            }
            else if (i == 2)
            {
                openCard = Instantiate(cardPrefab, originTrnsf);
                openCard.transform.localScale = new Vector3(1.2f, 1.3f, 1.3f);
                openCard.transform.position = originTrnsf.position;
            }
        }



        for (int i = 0; i < 13; i++)
        {
            GameObject g = Instantiate(cardPrefab, originTrnsf);
            g.transform.position = originTrnsf.position;
            g.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

            g.transform.parent = gameObject.transform;
            cardsObject.Add(g);
        }
        StartCoroutine(Animate());

        if (Wallet.GetPool() <= BootValueDecider.rummyBootValue * 200f)
        {
            finishAfterRound = Random.Range(1, 3);
        }
        else
        {
            finishAfterRound = Random.Range(2, 7);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddOpenDecktoClosedDeck();
        }
    }
    public void AddOpenDecktoClosedDeck()
    {
        int cnt = openCardsList.Count;
        for (int i = 0; i < cnt - 1; i++)
        {
            int k = Random.Range(0, openCardsList.Count - 1);
            gameDeck.Add(openCardsList[k]);
            openCardsList.RemoveAt(k);
        }
    }

    public void ClosedDeckCardRequest()
    {
        if (gameDeck.Count == 0)
        {
            AddOpenDecktoClosedDeck();
        }
        Transform lastBlockContainerTrans = mainBlock.transform.GetChild(mainBlock.transform.childCount - 1).transform.GetChild(0).transform;

       /* for (int i = 0; i < lastBlockContainerTrans.childCount; i++)
        {
            if (lastBlockContainerTrans.GetChild(i).GetComponent<DraggableItem>()._isSelected)
            {
                managerRef.selectedCardsList.Clear();
                lastBlockContainerTrans.GetChild(i).GetComponent<DraggableItem>()._isSelected = false;

            }
        }*/

        GameObject newCard = Instantiate(cardPrefab, closedCard.transform);
        Sprite s = gameDeck[Random.Range(0, gameDeck.Count)];
        newCard.GetComponent<Image>().sprite = s;
        gameDeck.Remove(s);
        cardSprites.Add(s);
        newCard.transform.DOMove(lastBlockContainerTrans.position, 0.3f)
           .SetEase(Ease.Linear)
           .OnComplete(() =>
           {
               newCard.transform.parent = lastBlockContainerTrans;
               newCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

               //newCard.transform.localPosition = Vector3.zero;
               alignChildrenRef.AlignChildren();
               managerRef._isTakenCard = true;
               if (s.name.Substring(1) == ConnectionRummy.jokerRank)
               {
                   newCard.transform.GetChild(0).gameObject.SetActive(true);
               }
           });
        managerRef.getCardBarrier.SetActive(true);
        managerRef.roundCount++;

    }

    public void OpenCardRequest()
    {
        Transform lastBlockContainerTrans = mainBlock.transform.GetChild(mainBlock.transform.childCount - 1).transform.GetChild(0).transform;

      /*  for (int i = 0; i < lastBlockContainerTrans.childCount; i++)
        {
            if (lastBlockContainerTrans.GetChild(i).GetComponent<DraggableItem>()._isSelected)
            {
                managerRef.selectedCardsList.Clear();
                lastBlockContainerTrans.GetChild(i).GetComponent<DraggableItem>()._isSelected = false;
            }
        }*/

        Transform newCard = managerRef.openDeck.transform.GetChild(managerRef.openDeck.transform.childCount - 1);
        newCard.transform.DOMove(lastBlockContainerTrans.position, 0.3f)
           .SetEase(Ease.Linear)
           .OnComplete(() =>
           {
               newCard.transform.parent = lastBlockContainerTrans;
               newCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
               cardSprites.Add(newCard.GetComponent<Image>().sprite);
               openCardsList.RemoveAt(openCardsList.Count - 1);

               RectTransform rectTransform = newCard.GetComponent<RectTransform>();
               rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
               rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

               // Set the pivot to the middle
               rectTransform.pivot = new Vector2(0.5f, 0.5f);

               // Reset the position
               rectTransform.anchoredPosition = Vector2.zero;
               alignChildrenRef.AlignChildren();
               managerRef._isTakenCard = true;
               if (newCard.GetComponent<Image>().sprite.name.Substring(1) == ConnectionRummy.jokerRank)
               {
                   newCard.transform.GetChild(0).gameObject.SetActive(true);
               }
           });
        managerRef.getCardBarrier.SetActive(true);
        managerRef.roundCount++;

    }


    public IEnumerator Animate()
    {
        middlePos = (13 * PlayerCardsOffset) / 2 + cardPrefab.GetComponent<RectTransform>().rect.width;
        managerRef.connectionRef.cardSound3.Play();
        for (int i = 0; i < cardsObject.Count; i++)
        {
            AnimateCard(cardsObject[i], posTransforms.position.x + (i * PlayerCardsOffset), i);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);
        SortCardsSprite(cardSprites);

        yield return new WaitForSeconds(1f);
        managerRef.connectionRef.cardshuffle_audio.Play();

        ShuffleCards();

        yield return new WaitForSeconds(2f);
        int tmpp = 0;
        for (int i = 0; i < 4; i++)
        {
            GameObject newBlock = Instantiate(blockPrefab, mainBlock.transform);
            newBlock.transform.parent = mainBlock.transform;

            for (int j = tmpp; j < tmpp + cardsSeqArray[i]; j++)
            {
                cardsObject[j].transform.parent = newBlock.transform.GetChild(0).transform;
            }
            tmpp += cardsSeqArray[i];
        }

        alignChildrenRef.AlignChildren();

        //////yaha se continue krna hai


        openCard.transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.3f)
            .SetEase(Ease.Linear);

        openCard.transform.DOMove(managerRef.openDeck.transform.position, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                openCard.transform.parent = managerRef.openDeck.transform;
                openCard.transform.DOScaleX(0f, 0.3f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {

              openCard.GetComponent<Image>().sprite = jokerSprite;
              openCard.transform.DOScaleX(1.3f, 0.3f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              openCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
              Sprite s = gameDeck[Random.Range(0, gameDeck.Count)];
              openCard.GetComponent<Image>().sprite = s;
              openCardsList.Add(s);
              gameDeck.Remove(s);
          });
          });
            });

        yield return new WaitForSeconds(0.1f);

        JokerCard.transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.3f)
            .SetEase(Ease.Linear);

        JokerCard.transform.DOMove(managerRef.closedDeck.transform.position + new Vector3(-100f, 30, 0), 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                JokerCard.transform.DORotate(new Vector3(0, 0, 40), 0.3f)
                .SetEase(Ease.Linear);

                JokerCard.transform.DOScaleX(0f, 0.3f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              // joker card
              Sprite spriteK = gameDeck[Random.Range(0, gameDeck.Count)];
              JokerCard.GetComponent<Image>().sprite = spriteK;
              gameDeck.Remove(spriteK);
              ConnectionRummy.jokerRank = spriteK.name.Substring(1);
              managerRef.resultJokerImg.sprite = spriteK;
              if (spriteK.name != "JJ")
              {
                  JokerCard.transform.GetChild(0).gameObject.SetActive(true);

              }
              CheckForJoker();
              JokerCard.transform.DOScaleX(1.3f, 0.3f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              closedCard.transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.3f)
            .SetEase(Ease.Linear);

              closedCard.transform.DOMove(managerRef.closedDeck.transform.position, 0.3f)
              .SetEase(Ease.Linear)
              .OnComplete(() =>
              {
                  JokerCardUpdate();
                  initialBlocker.SetActive(false);
                  managerRef.gameCorroutine =  StartCoroutine(managerRef.GameEnum());

              });
          });

          });
            });


    }

    public void AnimateCard(GameObject card, float xPos, int val)
    {

        card.transform.DOMove(new Vector2(xPos, posTransforms.position.y), 0.2f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                card.transform.DOScaleX(0f, 0.1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {


              card.transform.DOScaleY(1.5f, 0.1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {

              card.transform.DOScaleY(1.3f, 0.1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {

          });

          });



              cardsObject[val].GetComponent<Image>().sprite = cardSprites[val];


              card.transform.DOScaleX(1.3f, 0.1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              //shuffle animation
          });


          });

            });

    }

    public void ShuffleCards()
    {
        for (int i = 0; i < cardsObject.Count; i++)
        {
            ShuffleAnimation(cardsObject[i], i);
        }
    }

    // Update is called once per frame
    public void ShuffleAnimation(GameObject card, int val)
    {
        float tmpPosX = card.transform.position.x;
        card.transform.DOMoveX(middlePos, 0.3f)
      .SetEase(Ease.Linear);

        card.transform.GetComponent<Image>().DOFade(0.2f, 0.3f)
              .SetEase(Ease.Linear)

        .OnComplete(() =>
        {
            card.GetComponent<Image>().sprite = cardsSprite_Sorted[val];
            card.transform.DOMoveX(tmpPosX, 0.3f)
       .SetEase(Ease.Linear);

            card.transform.GetComponent<Image>().DOFade(1f, 0.3f)
                  .SetEase(Ease.Linear)

           .OnComplete(() =>
           {

           });

        });
    }

    public void SortCardsSprite(List<Sprite> spriteList)
    {
        spade.Clear();
        heart.Clear();
        club.Clear();
        diamond.Clear();


        for (int k = 0; k < spriteList.Count; k++)
        {
            if (spriteList[k].name.StartsWith("s"))
            {
                spade.Add(k);
            }
            else if (spriteList[k].name.StartsWith("c"))
            {
                club.Add(k);
            }
            else if (spriteList[k].name.StartsWith("d"))
            {
                diamond.Add(k);
            }
            else if (spriteList[k].name.StartsWith("h"))
            {
                heart.Add(k);
            }
            else if (spriteList[k].name.StartsWith("J"))
            {
                heart.Add(k);
            }
        }


        int[] compare = new int[] { spade.Count, club.Count, heart.Count, diamond.Count };

        for (int e = 3; e > 0 - 1; e--)
        {
            for (int j = 0; j < compare.Length - 1; j++)
            {
                if (compare[j] < compare[j + 1])
                {
                    int tmp = compare[j];
                    compare[j] = compare[j + 1];
                    compare[j + 1] = tmp;
                }
            }
        }
        for (int i = 0; i < compare.Length; i++)
        {
        }

        bool _s = false;
        bool _c = false;
        bool _h = false;
        bool _d = false;
        for (int k = 0; k < compare.Length; k++)
        {

            if (compare[k] == spade.Count && !_s)
            {
                _s = true;
                for (int j = 0; j < spade.Count; j++)
                {
                    cardsSprite_Sorted.Add(cardsObject[spade[j]].GetComponent<Image>().sprite);
                }
                cardsSeqArray[k] = spade.Count;

            }
            else if (compare[k] == club.Count && !_c)
            {

                _c = true;
                for (int j = 0; j < club.Count; j++)
                {
                    cardsSprite_Sorted.Add(cardsObject[club[j]].GetComponent<Image>().sprite);
                }
                cardsSeqArray[k] = club.Count;


            }
            else if (compare[k] == heart.Count && !_h)
            {

                _h = true;
                for (int j = 0; j < heart.Count; j++)
                {
                    cardsSprite_Sorted.Add(cardsObject[heart[j]].GetComponent<Image>().sprite);
                }
                cardsSeqArray[k] = heart.Count;


            }
            else if (compare[k] == diamond.Count && !_d)
            {

                _d = true;
                for (int j = 0; j < diamond.Count; j++)
                {
                    cardsSprite_Sorted.Add(cardsObject[diamond[j]].GetComponent<Image>().sprite);
                }
                cardsSeqArray[k] = diamond.Count;


            }


        }




    }

    public void AddCardToOpenDeckbyP2()
    {
        managerRef.connectionRef.cardSound.Play();
        int sn = Random.Range(0, 2);
        if (managerRef.roundCount > finishAfterRound && sn == 1)
        {
            declareCardImg.gameObject.SetActive(true);

            declareCardImg.sprite = gameDeck[Random.Range(0, gameDeck.Count)];
            declarePanel.SetActive(true);
            StartCoroutine(managerRef.connectionRef.TimerOpp(45));
            managerRef._isDeclared = true;

        }
        if (gameDeck.Count == 0)
        {
            AddOpenDecktoClosedDeck();
        }
        GameObject newCard = Instantiate(cardPrefab, managerRef.player2.transform);
        newCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

        newCard.transform.DOMove(managerRef.openDeck.transform.position, 0.3f)
           .SetEase(Ease.Linear)
           .OnComplete(() =>
           {
               newCard.transform.parent = managerRef.openDeck.transform;

           });

        Sprite s = gameDeck[Random.Range(0, gameDeck.Count)];
        newCard.GetComponent<Image>().sprite = s;
        openCardsList.Add(s);

        gameDeck.Remove(s);

       
      
    }

    bool _suitSort = false;
    public void SortCardsButton()
    {

        for (int k = 0; k < mainBlock.transform.childCount; k++)
        {
            Destroy(mainBlock.transform.GetChild(k).gameObject);
        }

        if (!_suitSort)
        {
            List<List<Sprite>> s = SortCards(cardSprites);
          

            for (int i = 0; i < s.Count(); i++)
            {
                GameObject newBlock = Instantiate(blockPrefab, mainBlock.transform);
                newBlock.transform.parent = mainBlock.transform;
                for (int j = 0; j < s[i].Count(); j++)
                {
                    GameObject newCard = Instantiate(cardPrefab);
                    newCard.GetComponent<Image>().sprite = s[i][j];

                    newCard.transform.parent = newBlock.transform.GetChild(0).transform;
                    newCard.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
                    if (s[i][j].name.Substring(1) == ConnectionRummy.jokerRank)
                    {
                        newCard.transform.GetChild(0).gameObject.SetActive(true);
                    }
                }
            }

        }
        else
        {
            Dictionary<char, List<Sprite>> suitGroups = new Dictionary<char, List<Sprite>>();

            foreach (var card in cardSprites)
            {
                string cardName = card.name;

                if (cardName.Length < 2)
                {
                    Debug.LogError("Invalid card name format.");
                    continue;
                }

                // Get the suit (first character of the card's name)
                char suit = cardName[0];

                // Add the card to its respective suit group
                if (!suitGroups.ContainsKey(suit))
                {
                    suitGroups[suit] = new List<Sprite>();
                }

                suitGroups[suit].Add(card);
            }

            // Sort each suit group based on the rank
            foreach (var suitGroup in suitGroups)
            {
                // Sort the cards in ascending order by their rank
                suitGroup.Value.Sort((a, b) =>
                {
                    string rankA = a.name.Substring(1); // Get the rank part (ignore the first char which is the suit)
                    string rankB = b.name.Substring(1);

                    // Convert face cards to appropriate values
                    int valueA = ConvertCardRankToValue(rankA);
                    int valueB = ConvertCardRankToValue(rankB);

                    return valueA.CompareTo(valueB);
                });

                // Instantiate blocks for each suit group
                GameObject newBlock = Instantiate(blockPrefab, mainBlock.transform);

                // For each suit, get the sorted cards and instantiate them
                foreach (var card in suitGroup.Value)
                {
                    GameObject newCard = Instantiate(cardPrefab);
                    newCard.GetComponent<Image>().sprite = card;
                    newCard.transform.parent = newBlock.transform.GetChild(0).transform;
                    newCard.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

                    // Check for Joker card and activate its indicator if necessary
                    if (card.name.Substring(1) == ConnectionRummy.jokerRank)
                    {
                        newCard.transform.GetChild(0).gameObject.SetActive(true);
                    }
                }
            }
        }

        _suitSort = !_suitSort;

        Invoke("TotalScoreUpdater", 0.1f);


    }


    int ConvertCardRankToValue(string rank)
    {
        switch (rank)
        {
            case "a": return 14;   // Ace is treated as 14
            case "0": return 10;  // Jack is 11
            case "j": return 11;  // Jack is 11
            case "q": return 12;  // Queen is 12
            case "k": return 13;  // King is 13
            default:
                // Parse numeric ranks (like "2" to "10")
                if (int.TryParse(rank, out int value))
                {
                    return value;
                }
                else
                {
                    Debug.LogError("Invalid card rank: " + rank);
                    return 0; // Return a default value if parsing fails
                }
        }
    }

    public List<List<Sprite>> SortCards(List<Sprite> sprites)
    {
        // Define the order of ranks
        Dictionary<string, int> rankOrder = new Dictionary<string, int>
        {
            { "2", 2 }, { "3", 3 }, { "4", 4 }, { "5", 5 },
            { "6", 6 }, { "7", 7 }, { "8", 8 }, { "9", 9 },
            { "10", 10 }, { "j", 11 }, { "q", 12 }, { "k", 13 }, { "a", 14 }
        };

        // Parse the cards
        var parsedCards = sprites.Select(sprite => new { Sprite = sprite, Parsed = ParseCard(sprite.name, rankOrder) }).ToList();

        // Group by suit and sort by rank
        var groupedBySuit = parsedCards.GroupBy(card => card.Parsed.suit)
                                       .Select(group => group.OrderBy(card => card.Parsed.rank).ToList())
                                       .ToList();

        // Find sequences of three consecutive cards
        List<List<Sprite>> sequences = new List<List<Sprite>>();
        HashSet<Sprite> usedCards = new HashSet<Sprite>();

        foreach (var group in groupedBySuit)
        {
            for (int i = 0; i <= group.Count - 3; i++)
            {
                if (group[i + 2].Parsed.rank == group[i + 1].Parsed.rank + 1 && group[i + 1].Parsed.rank == group[i].Parsed.rank + 1)
                {
                    sequences.Add(new List<Sprite> { group[i].Sprite, group[i + 1].Sprite, group[i + 2].Sprite });
                    usedCards.Add(group[i].Sprite);
                    usedCards.Add(group[i + 1].Sprite);
                    usedCards.Add(group[i + 2].Sprite);
                    i += 2; // Skip to the card after the sequence
                }
            }
        }

        // Find combos of three or more cards with the same rank from remaining cards
        var remainingCards = parsedCards.Where(card => !usedCards.Contains(card.Sprite)).ToList();
        var groupedByRank = remainingCards.GroupBy(card => card.Parsed.rank)
                                          .Where(group => group.Count() >= 3)
                                          .Select(group => group.Select(card => card.Sprite).ToList())
                                          .ToList();

        // Collect all sequences and trails
        List<List<Sprite>> sortedCardGroups = new List<List<Sprite>>(sequences);
        sortedCardGroups.AddRange(groupedByRank);

        // Add remaining non-sequence and non-trail cards
        var remainingNonSequences = remainingCards.Where(card => !groupedByRank.SelectMany(group => group).Contains(card.Sprite))
                                                  .Select(card => card.Sprite)
                                                  .ToList();

        if (remainingNonSequences.Count > 0)
        {
            sortedCardGroups.Add(remainingNonSequences);
        }

        return sortedCardGroups;
    }


    (string suit, int rank) ParseCard(string cardName, Dictionary<string, int> rankOrder)
    {
        string suit = cardName.Substring(0, 1); // Get the first character as the suit
        string rankStr = cardName.Substring(1); // Get the rest as the rank
        if (rankOrder.TryGetValue(rankStr, out int rank))
        {
            return (suit, rank);
        }
        else
        {
            return ("error", 404);
            //throw new ArgumentException($"Invalid card rank: {rankStr}");
        }
    }


    public void Group()
    {
        if (mainBlock.transform.childCount >= 6)
        {
            return;
        }
        GameObject newBlock = Instantiate(blockPrefab, mainBlock.transform);
        newBlock.transform.parent = mainBlock.transform;
        foreach (GameObject g in managerRef.selectedCardsList)
        {
            g.transform.parent = newBlock.transform.GetChild(0).transform;
            g.GetComponent<DraggableItem>()._isSelected = false;
        }
        managerRef.selectedCardsList.Clear();
        alignChildrenRef.AlignChildren();
        managerRef.GroupBtnOjb.SetActive(false);

        Invoke("TotalScoreUpdater", 0.1f);
    }

    public void JokerCardUpdate()
    {
        for (int i = 0; i < mainBlock.transform.childCount; i++)
        {
            mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().CheckforCardsChange();
        }
    }
}
