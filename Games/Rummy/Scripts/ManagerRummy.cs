using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;
using System.Linq;

public class ManagerRummy : MonoBehaviour
{
    public GameObject obstaclePanel;

    public ConnectionRummy connectionRef;
    public bool _isDeclared;
    public bool _isMyChance;

    public int roundCount;
    public Transform finishSlotPos;
    FinishPanelRummy finishPanelScriptRef;
    public Image resultJokerImg;
    public GameObject GroupBtnOjb;
    public List<GameObject> selectedCardsList;
    public float myWallet;
    public TextMeshProUGUI myWalletTxt;
    public GameObject player2;
    public GameObject player3;
    public GameObject player4;
    public GameObject player5;
    public GameObject player6;
    public int totalScore = 0;
    public GameObject openDeck;
    public GameObject closedDeck;
    public List<Sprite> deckStandard;
    public InitialCardDistributor initialCardDistributorRef;
    public GameObject getCardBarrier;
    public bool _isTakenCard;
    public float time;
    public float currentValue;
    public GameObject myTimer;
    public GameObject p2Timer;
    public GameObject p3Timer;
    public GameObject p4Timer;
    public GameObject p5Timer;
    public GameObject p6Timer;
    public Coroutine myTimerCoroutine;

    public GameObject finishPanel;
    public TextMeshProUGUI fPanel_myScore;
    public TextMeshProUGUI fPanel_p2Score;
    public TextMeshProUGUI fPanel_myBonus;
    public TextMeshProUGUI fPanel_p2Bonus;
    public TextMeshProUGUI fPanel_p2Name;
    public Coroutine gameCoroutine;
    public int playerChance; 
    Coroutine p2Coroutine;
    Coroutine p3Coroutine;
    Coroutine p4Coroutine;
    Coroutine p5Coroutine;
    Coroutine p6Coroutine;

    // Start is called before the first frame update

    System.DateTime offTime;
    bool _isInitialFocus;

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Time.timeScale = 1;
            if (!_isInitialFocus)
            {
                System.TimeSpan duration = System.DateTime.Now - offTime;
                float elapsedTime = (float)duration.TotalSeconds;
                Debug.Log("E. time : " + elapsedTime);

                if (_isMyChance)
                {
                    Debug.Log("Player chance thi");

                    StopCoroutine(myTimerCoroutine);
                    Debug.Log("current value is : " + currentValue * 30);
                    if (elapsedTime < currentValue * 30)
                    {
                        Debug.Log("elapse time kam tha");
                        float val = (currentValue * 30) - elapsedTime;
                        myTimerCoroutine = StartCoroutine(DecreaseOverTime(myTimer, 30 - val));
                    }
                    else
                    {
                        myTimer.GetComponent<Image>().fillAmount = 0;
                        obstaclePanel.SetActive(true);


                        float rem = elapsedTime % 30;
                        Debug.Log("rem " + rem);
                        if (rem > 24)
                        {
                            StopCoroutine(gameCoroutine);
                            StartCoroutine(AutoPlay(6));
                        }
                        else if (rem > 18)
                        {
                            StopCoroutine(gameCoroutine);
                            StartCoroutine(AutoPlay(5));
                        }
                        else if (rem > 12)
                        {
                            StopCoroutine(gameCoroutine);
                            StartCoroutine(AutoPlay(4));
                        }
                        else if (rem > 6)
                        {
                            StopCoroutine(gameCoroutine);
                            StartCoroutine(AutoPlay(3));
                        }
                        else
                        {
                            StopCoroutine(gameCoroutine);
                            StartCoroutine(AutoPlay(2));
                        }
                    }
                        
                    Debug.Log("Your Chance");
                    //timer yaha se start krna hai
                   
                }
                else
                {
                    Debug.Log(1);

                    if (p2Coroutine != null)
                    {
                        StopCoroutine(p2Coroutine);
                    }
                    if (p3Coroutine != null)
                    {
                        StopCoroutine(p3Coroutine);
                    }
                    if (p4Coroutine != null)
                    {
                        StopCoroutine(p4Coroutine);
                    }
                    if (p5Coroutine != null)
                    {
                        StopCoroutine(p5Coroutine);
                    }
                    if (p6Coroutine != null)
                    {
                        StopCoroutine(p6Coroutine);
                    }
                    Debug.Log(2);

                    p2Timer.GetComponent<Image>().fillAmount = 0;
                    p3Timer.GetComponent<Image>().fillAmount = 0;
                    p4Timer.GetComponent<Image>().fillAmount = 0;
                    p5Timer.GetComponent<Image>().fillAmount = 0;
                    p6Timer.GetComponent<Image>().fillAmount = 0;

                    StopCoroutine(gameCoroutine);

                    Debug.Log(3 + "pc : " + playerChance);

                    switch (playerChance)
                    {
                        case 2:
                            if(elapsedTime > 54)
                            {
                                // player pack 
                                myTimer.GetComponent<Image>().fillAmount = 0;
                                obstaclePanel.SetActive(true);
                                StartCoroutine(AutoPlay(Random.Range(2,6)));
                                return;
                            }
                            else if(elapsedTime > 24)
                            {
                                gameCoroutine = StartCoroutine(GameEnum(7));
                                return;
                            }
                            else
                            {
                                if(elapsedTime > 5)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(3));
                                }
                                else if(elapsedTime > 11)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(4));
                                }
                                else if(elapsedTime > 16)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(5));
                                }
                                else if (elapsedTime > 20)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(6));
                                }
                                else
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(2));
                                    float val = (currentValue * 30) - elapsedTime;
                                    if (p2Coroutine != null)
                                    {
                                        StopCoroutine(p2Coroutine);
                                    }
                                    p2Coroutine = StartCoroutine(DecreaseOverTime(p2Timer, 30 - val));
                                }
                            }
                            break;
                        case 3:
                            if (elapsedTime > 48)
                            {
                                myTimer.GetComponent<Image>().fillAmount = 0;
                                obstaclePanel.SetActive(true);
                                StartCoroutine(AutoPlay(Random.Range(2, 6)));
                                return;
                            }
                            else if (elapsedTime > 18)
                            {
                                gameCoroutine = StartCoroutine(GameEnum(7));
                                return;
                            }
                            else
                            {
                                if (elapsedTime > 5)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(4));
                                }
                                else if (elapsedTime > 11)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(5));
                                }
                                else if (elapsedTime > 16)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(6));
                                }
                                else
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(3));
                                    float val = (currentValue * 30) - elapsedTime;
                                    if (p3Coroutine != null)
                                    {
                                        StopCoroutine(p3Coroutine);
                                    }
                                    p3Coroutine = StartCoroutine(DecreaseOverTime(p3Timer, 30 - val));
                                }

                            }
                            break;
                        case 4:
                            if (elapsedTime > 42)
                            {
                                myTimer.GetComponent<Image>().fillAmount = 0;
                                obstaclePanel.SetActive(true);
                                StartCoroutine(AutoPlay(4));
                                return;
                            }
                            else if (elapsedTime > 12)
                            {
                                gameCoroutine = StartCoroutine(GameEnum(7));
                                return;
                            }
                            else
                            {
                                if (elapsedTime > 5)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(5));
                                }
                                else if (elapsedTime > 10)
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(6));
                                }
                                else
                                {
                                    gameCoroutine = StartCoroutine(GameEnum(4));
                                    float val = (currentValue * 30) - elapsedTime;
                                    if (p4Coroutine != null)
                                    {
                                        StopCoroutine(p4Coroutine);
                                    }
                                    p4Coroutine = StartCoroutine(DecreaseOverTime(p4Timer, 30 - val));
                                }
                            }
                            break;
                        case 5:
                            if (elapsedTime > 36)
                            {
                                myTimer.GetComponent<Image>().fillAmount = 0;
                                obstaclePanel.SetActive(true);
                                StartCoroutine(AutoPlay(5));
                                return;
                            }
                            else if (elapsedTime > 6)
                            {
                                gameCoroutine = StartCoroutine(GameEnum(7));
                                return;
                            }
                            if (elapsedTime > 4)
                            {
                                gameCoroutine = StartCoroutine(GameEnum(6));
                            }
                            else
                            {
                                gameCoroutine = StartCoroutine(GameEnum(5));
                                float val = (currentValue * 30) - elapsedTime;
                                if (p5Coroutine != null)
                                {
                                    StopCoroutine(p5Coroutine);
                                }
                                p5Coroutine = StartCoroutine(DecreaseOverTime(p5Timer, 30 - val));
                            }
                            break;
                        case 6:
                            if (elapsedTime > 30)
                            {
                                myTimer.GetComponent<Image>().fillAmount = 0;
                                obstaclePanel.SetActive(true);
                                StartCoroutine(AutoPlay(6));
                                return;
                            }
                            else if (elapsedTime > 5)
                            {
                                gameCoroutine = StartCoroutine(GameEnum(7));
                                return;
                            }
                            else
                            {
                                gameCoroutine = StartCoroutine(GameEnum(6));
                                float val = (currentValue * 30) - elapsedTime;
                                if (p6Coroutine != null)
                                {
                                    StopCoroutine(p6Coroutine);
                                }
                                p6Coroutine = StartCoroutine(DecreaseOverTime(p6Timer, 30 - val));
                            }

                            break;
                    }


                   
                }

            }
        }
        else
        {

            _isInitialFocus = false;
            offTime = System.DateTime.Now;
            Time.timeScale = 0;
        }
    }




    void Start()

    {
        obstaclePanel.SetActive(false);
        roundCount = 0;
        GroupBtnOjb.SetActive(false);
        connectionRef = FindObjectOfType<ConnectionRummy>();
        finishPanelScriptRef = FindObjectOfType<FinishPanelRummy>();
        selectedCardsList = new List<GameObject>();
        finishPanel.SetActive(false);
        _isTakenCard = false;
        getCardBarrier.SetActive(true);

        initialCardDistributorRef = FindObjectOfType<InitialCardDistributor>();

        p2Timer.GetComponent<Image>().fillAmount = 0f;
        p3Timer.GetComponent<Image>().fillAmount = 0f;
        p4Timer.GetComponent<Image>().fillAmount = 0f;
        p5Timer.GetComponent<Image>().fillAmount = 0f;
        p6Timer.GetComponent<Image>().fillAmount = 0f;
    }


    public IEnumerator GameEnum(int startPos = 0)
    {
        _isTakenCard = false;
        initialCardDistributorRef.ScoreUpdate();
        connectionRef.DropBtn.SetActive(true);


        if (startPos <= 2)
        {
            if (_isDeclared)
            {

                yield break;
            }
            //player 2 chance
            p2Coroutine = StartCoroutine(DecreaseOverTime(p2Timer));
            playerChance = 2;
            yield return new WaitForSeconds(Random.Range(1, 3));
            connectionRef.cardSound.Play();

            GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
            deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            deleteMeCard.transform.DOMove(player2.transform.position, 0.3f)
               .SetEase(Ease.Linear);
            deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                .SetEase(Ease.Linear)
               .OnComplete(() =>
               {
                   Destroy(deleteMeCard);
               });

            yield return new WaitForSeconds(Random.Range(2, 5));
            initialCardDistributorRef.AddCardToOpenDeckbyP2();
            StopCoroutine(p2Coroutine);
            p2Timer.GetComponent<Image>().fillAmount = 0;
        }

        if (startPos <= 3)
        {
            if (_isDeclared)
            {

                yield break;
            }

            //player3 chance

            p3Coroutine = StartCoroutine(DecreaseOverTime(p3Timer));
            playerChance = 3;

            yield return new WaitForSeconds(Random.Range(1, 3));
            connectionRef.cardSound.Play();

            GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
            deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            deleteMeCard.transform.DOMove(player3.transform.position, 0.3f)
               .SetEase(Ease.Linear);
            deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                .SetEase(Ease.Linear)
               .OnComplete(() =>
               {
                   Destroy(deleteMeCard);
               });

            yield return new WaitForSeconds(Random.Range(2, 5));
            initialCardDistributorRef.AddCardToOpenDeckbyP3();
            StopCoroutine(p3Coroutine);
            p3Timer.GetComponent<Image>().fillAmount = 0;

        }

        if (startPos <= 4)
        {
            if (_isDeclared)
            {

                yield break;
            }

            //player4 chance
            p4Coroutine = StartCoroutine(DecreaseOverTime(p4Timer));
            playerChance = 4;

            yield return new WaitForSeconds(Random.Range(1, 3));
            connectionRef.cardSound.Play();
            GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
            deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            deleteMeCard.transform.DOMove(player4.transform.position, 0.3f)
               .SetEase(Ease.Linear);
            deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                .SetEase(Ease.Linear)
               .OnComplete(() =>
               {
                   Destroy(deleteMeCard);
               });

            yield return new WaitForSeconds(Random.Range(2, 5));
            initialCardDistributorRef.AddCardToOpenDeckbyP4();
            StopCoroutine(p4Coroutine);
            p4Timer.GetComponent<Image>().fillAmount = 0;

        }

        if (startPos <= 5)
        {
            if (_isDeclared)
            {

                yield break;
            }
            //player5 chance

            p5Coroutine = StartCoroutine(DecreaseOverTime(p5Timer));
            playerChance = 5;

            yield return new WaitForSeconds(Random.Range(1, 3));
            connectionRef.cardSound.Play();

            GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
            deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            deleteMeCard.transform.DOMove(player5.transform.position, 0.3f)
               .SetEase(Ease.Linear);
            deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                .SetEase(Ease.Linear)
               .OnComplete(() =>
               {
                   Destroy(deleteMeCard);
               });

            yield return new WaitForSeconds(Random.Range(2, 5));
            initialCardDistributorRef.AddCardToOpenDeckbyP5();
            StopCoroutine(p5Coroutine);
            p5Timer.GetComponent<Image>().fillAmount = 0;

        }

        if (startPos <= 6)
        {

            if (_isDeclared)
            {

                yield break;
            }

            //player 6 chance
            p6Coroutine = StartCoroutine(DecreaseOverTime(p6Timer));
            playerChance = 6;

            yield return new WaitForSeconds(Random.Range(1, 3));
            connectionRef.cardSound.Play();

            GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
            deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            deleteMeCard.transform.DOMove(player6.transform.position, 0.3f)
               .SetEase(Ease.Linear);
            deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                .SetEase(Ease.Linear)
               .OnComplete(() =>
               {
                   Destroy(deleteMeCard);
               });

            yield return new WaitForSeconds(Random.Range(2, 5));
            initialCardDistributorRef.AddCardToOpenDeckbyP6();
            StopCoroutine(p6Coroutine);
            p6Timer.GetComponent<Image>().fillAmount = 0;
        }
      
       

       
       

      

        //player chance
        #if UNITY_ANDROID || PLATFORM_ANDROID
                    Handheld.Vibrate();
                #endif

        myTimerCoroutine = StartCoroutine(DecreaseOverTime(myTimer));
        getCardBarrier.SetActive(false);
    }

    int autoRounds = 0;
    public IEnumerator AutoPlay(int startPos = 0)
    {
        
        while (autoRounds < 8)
        {

            autoRounds++;

            if(startPos <= 2)
            {
                //player 2 chance
                Coroutine p2Coroutine = StartCoroutine(DecreaseOverTime(p2Timer));

                yield return new WaitForSeconds(Random.Range(1, 3));
                connectionRef.cardSound.Play();

                GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
                deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                deleteMeCard.transform.DOMove(player2.transform.position, 0.3f)
                   .SetEase(Ease.Linear);
                deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                    .SetEase(Ease.Linear)
                   .OnComplete(() =>
                   {
                       Destroy(deleteMeCard);
                   });

                yield return new WaitForSeconds(Random.Range(2, 5));
                initialCardDistributorRef.AddCardToOpenDeckbyP2();
                StopCoroutine(p2Coroutine);
                p2Timer.GetComponent<Image>().fillAmount = 0;

                if (_isDeclared)
                {
                    yield break;
                }

            }
            else if (startPos <= 3)
            {
                //player3 chance

                Coroutine p3Coroutine = StartCoroutine(DecreaseOverTime(p3Timer));

                yield return new WaitForSeconds(Random.Range(1, 3));
                connectionRef.cardSound.Play();

                GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
                deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                deleteMeCard.transform.DOMove(player3.transform.position, 0.3f)
                   .SetEase(Ease.Linear);
                deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                    .SetEase(Ease.Linear)
                   .OnComplete(() =>
                   {
                       Destroy(deleteMeCard);
                   });

                yield return new WaitForSeconds(Random.Range(2, 5));
                initialCardDistributorRef.AddCardToOpenDeckbyP3();
                StopCoroutine(p3Coroutine);
                p3Timer.GetComponent<Image>().fillAmount = 0;

                if (_isDeclared)
                {

                    yield break;
                }

            }
            else if (startPos <= 4)
            {
                //player4 chance
                Coroutine p4Coroutine = StartCoroutine(DecreaseOverTime(p4Timer));

                yield return new WaitForSeconds(Random.Range(1, 3));
                connectionRef.cardSound.Play();
                GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
                deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                deleteMeCard.transform.DOMove(player4.transform.position, 0.3f)
                   .SetEase(Ease.Linear);
                deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                    .SetEase(Ease.Linear)
                   .OnComplete(() =>
                   {
                       Destroy(deleteMeCard);
                   });

                yield return new WaitForSeconds(Random.Range(2, 5));
                initialCardDistributorRef.AddCardToOpenDeckbyP4();
                StopCoroutine(p4Coroutine);
                p4Timer.GetComponent<Image>().fillAmount = 0;


                if (_isDeclared)
                {

                    yield break;
                }
            }
            else if (startPos <= 5)
            {
                //player5 chance

                Coroutine p5Coroutine = StartCoroutine(DecreaseOverTime(p5Timer));

                yield return new WaitForSeconds(Random.Range(1, 3));
                connectionRef.cardSound.Play();

                GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
                deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                deleteMeCard.transform.DOMove(player5.transform.position, 0.3f)
                   .SetEase(Ease.Linear);
                deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                    .SetEase(Ease.Linear)
                   .OnComplete(() =>
                   {
                       Destroy(deleteMeCard);
                   });

                yield return new WaitForSeconds(Random.Range(2, 5));
                initialCardDistributorRef.AddCardToOpenDeckbyP5();
                StopCoroutine(p5Coroutine);
                p5Timer.GetComponent<Image>().fillAmount = 0;


                if (_isDeclared)
                {

                    yield break;
                }
            }
            else if (startPos <= 6)
            {
                //player 6 chance
                Coroutine p6Coroutine = StartCoroutine(DecreaseOverTime(p6Timer));

                yield return new WaitForSeconds(Random.Range(1, 3));
                connectionRef.cardSound.Play();

                GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
                deleteMeCard.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                deleteMeCard.transform.DOMove(player6.transform.position, 0.3f)
                   .SetEase(Ease.Linear);
                deleteMeCard.transform.DOScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f)
                    .SetEase(Ease.Linear)
                   .OnComplete(() =>
                   {
                       Destroy(deleteMeCard);
                   });

                yield return new WaitForSeconds(Random.Range(2, 5));
                initialCardDistributorRef.AddCardToOpenDeckbyP6();
                StopCoroutine(p6Coroutine);
                p6Timer.GetComponent<Image>().fillAmount = 0;
            }
          
           
           
           
           

           

        }
        {
            int totalCardSum = 0;
            for (int i = 0; i < initialCardDistributorRef.mainBlock.transform.childCount; i++)
            {
                totalCardSum += initialCardDistributorRef.mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
            }
            finishPanelScriptRef.ShowCards(totalCardSum);
            finishPanel.SetActive(true);
            yield return null;

        }
    }



    public void Dropped()
    {
        gameCoroutine = gameCoroutine = StartCoroutine(GameEnum());
        StopCoroutine(myTimerCoroutine);
        myTimer.GetComponent<Image>().fillAmount = 0;
    }
    public void Discard(GameObject card)
    {

        GameObject cardToDiscard;
        if (card != null)
        {
            cardToDiscard = card;
        }
        else
        {
            cardToDiscard = selectedCardsList[selectedCardsList.Count - 1];
        }

        for (int i = 0; i < openDeck.transform.childCount; i++)
        {
            openDeck.transform.GetChild(i).GetComponent<Button>().enabled = false;
            openDeck.transform.GetChild(i).GetComponent<DraggableItem>().enabled = false;
        }

        cardToDiscard.transform.DOMove(openDeck.transform.position, 0.3f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                initialCardDistributorRef.cardSprites.Remove(cardToDiscard.GetComponent<Image>().sprite);
                cardToDiscard.transform.parent = openDeck.transform;
                Debug.Log("parent cange successfully");

                cardToDiscard.GetComponent<Button>().enabled = false;
                initialCardDistributorRef.dropPanel.SetActive(false);
                _isTakenCard = false;
                Dropped();
                selectedCardsList.Clear();

            });

    }


    // Update is called once per frame
    IEnumerator DecreaseOverTime(GameObject gobj, float startTime = 0)
    {
        if (gobj == myTimer)
        {
            _isMyChance = true;
        }
        else
        {
            _isMyChance = false;
        }
        float timer = startTime;

        while (timer <= time)
        {
            // Calculate the normalized time (0 to 1)
            float normalizedTime = timer / time;
            // Use Mathf.Lerp to smoothly interpolate between 1 and 0
            currentValue = Mathf.Lerp(1.0f, 0.0f, normalizedTime);
            gobj.GetComponent<Image>().fillAmount = currentValue;
            // Increment the timer
            timer += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        if(gobj == myTimer)
        {
            obstaclePanel.SetActive(true);

            StartCoroutine(AutoPlay());
        }

        //connectionRef.DropWithLoss();
    }


    public void Finish(int k = 0)
    {
        initialCardDistributorRef.declarePanel.SetActive(false);

        int totalCardSum = 0;

        if (finishPanel.activeInHierarchy)
        {
            return;
        }
        if (k == 1)
        {
            selectedCardsList[selectedCardsList.Count - 1].transform.DOMove(finishSlotPos.position, 0.5f)
      .SetEase(Ease.Linear)
      .OnComplete(() =>
      {
          for (int i = 0; i < initialCardDistributorRef.mainBlock.transform.childCount; i++)
          {
              totalCardSum += initialCardDistributorRef.mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
          }
          finishPanelScriptRef.ShowCards(totalCardSum);
          finishPanel.SetActive(true);
      });
            selectedCardsList[0].transform.SetParent(finishSlotPos);
            selectedCardsList.Clear();
          

        }
        else
        {
        
            for (int i = 0; i < initialCardDistributorRef.mainBlock.transform.childCount; i++)
            {
                totalCardSum += initialCardDistributorRef.mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
            }
            finishPanelScriptRef.ShowCards(totalCardSum);
            finishPanel.SetActive(true);
        }

    }

    public void Leave()
    {
        SceneManager.LoadScene(1);
    }

    public void StartAgain()
    {
        SceneManager.LoadScene("Rummy");
    }

    public void DropWithLoss()
    {
        myWallet -= connectionRef.DropValue;
        Wallet.DeductAmount(connectionRef.DropValue);
        Leave();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            finishPanelScriptRef.ShowCards(0);
            finishPanel.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Time.timeScale *= 2;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Time.timeScale /= 2;
        }
        
    }
}
