using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;

public class ManagerRummy2Player : MonoBehaviour
{
    public bool _isMyChance;

    public ConnectionRummy connectionRef;
    public bool _isDeclared;
    public int roundCount;
    public Transform finishSlotPos;
    public FinishPanelRummy2Player finishPanelScriptRef;
    public GameObject GroupBtnOjb;
    public List<GameObject> selectedCardsList;
    public Image resultJokerImg;
    public float myWallet;
    public TextMeshProUGUI myWalletTxt;
    public GameObject player2;
    public int totalScore = 0;
    public GameObject openDeck;
    public GameObject closedDeck;
    public List<Sprite> deckStandard;
    public InitialCardDistributor2Player initialCardDistributorRef;
    public GameObject getCardBarrier;
    public bool _isTakenCard;
    public float time;
    public float currentValue;
    public GameObject myTimer;
    public GameObject p2Timer;
    public Coroutine myTimerCoroutine;
    Coroutine p2Coroutine;
    public Coroutine gameCorroutine;
    public GameObject finishPanel;
    public TextMeshProUGUI fPanel_myScore;
    public TextMeshProUGUI fPanel_p2Score;
    public TextMeshProUGUI fPanel_myBonus;
    public TextMeshProUGUI fPanel_p2Bonus;
    public TextMeshProUGUI fPanel_p2Name;
    // Start is called before the first frame update
    System.DateTime offTime;
    float myRemainingTime;
    bool _isInitialFocus;
    public Coroutine gameCoroutine;


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
                        finishPanelScriptRef.TimeOut();
                        finishPanel.SetActive(true);
                    }

                    Debug.Log("Your Chance");
                    //timer yaha se start krna hai

                }
                else
                {
                    StopCoroutine(p2Coroutine);
                    p2Timer.GetComponent<Image>().fillAmount = 0;
                    if (elapsedTime > currentValue * 30)
                    {
                        finishPanelScriptRef.TimeOut();
                        finishPanel.SetActive(true);
                    }
                    else if(elapsedTime > 5 || p2Timer.GetComponent<Image>().fillAmount < 0.7f)
                    {
                        StopCoroutine(gameCoroutine);
                        float val = (currentValue * 30) - elapsedTime + 5;
                        gameCoroutine = StartCoroutine(GameEnum(2));
                        myTimerCoroutine = StartCoroutine(DecreaseOverTime(myTimer, 30 - val));
                    }
                    else
                    {

                        float val = (currentValue * 30) - elapsedTime;
                        p2Coroutine = StartCoroutine(DecreaseOverTime(p2Timer, 30 - val));
                    }
                    
                }

            }
        }
        else
        {

            _isInitialFocus = false;
            offTime = System.DateTime.Now;
            myRemainingTime = myTimer.GetComponent<Image>().fillAmount * 30;
            Time.timeScale = 0;
        }
    }




    private void Awake()
    {
        connectionRef = FindObjectOfType<ConnectionRummy>();

    }
    void Start()

    {
        GroupBtnOjb.SetActive(false);
        finishPanel.SetActive(false);
        _isTakenCard = false;
        getCardBarrier.SetActive(true);
        initialCardDistributorRef = FindObjectOfType<InitialCardDistributor2Player>();
        finishPanelScriptRef = FindObjectOfType<FinishPanelRummy2Player>();
    }


    public IEnumerator GameEnum(int val = 1)
    {
        _isTakenCard = false;
        initialCardDistributorRef.ScoreUpdate();
        connectionRef.DropBtn.SetActive(true);

        //player 2 chance
        if(val < 2)
        {
            p2Coroutine = StartCoroutine(DecreaseOverTime(p2Timer));
            yield return new WaitForSeconds(Random.Range(1, 3));
            GameObject deleteMeCard = Instantiate(initialCardDistributorRef.cardPrefab, closedDeck.transform);
            connectionRef.cardSound.Play();
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
        
        //player chance

        if (!_isDeclared)
        {
            #if UNITY_ANDROID || PLATFORM_ANDROID
                    Handheld.Vibrate();
                #endif
            myTimerCoroutine = StartCoroutine(DecreaseOverTime(myTimer));
            getCardBarrier.SetActive(false);
        }

    }

    public void Dropped()
    {
        gameCoroutine = StartCoroutine(GameEnum());
        StopCoroutine(myTimerCoroutine);
        myTimer.GetComponent<Image>().fillAmount = 0;
    }


    public void Discard(GameObject card = null)
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
                Sprite s = cardToDiscard.GetComponent<Image>().sprite;
                initialCardDistributorRef.cardSprites.Remove(s);
                cardToDiscard.transform.parent = openDeck.transform;
                cardToDiscard.GetComponent<Button>().enabled = false;
                initialCardDistributorRef.dropPanel.SetActive(false);
                initialCardDistributorRef.openCardsList.Add(s);

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
        finishPanelScriptRef.TimeOut();
        finishPanel.SetActive(true);
    }


    public void OpenCard()
    {
        initialCardDistributorRef.OpenCardRequest();
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
            selectedCardsList[0].transform.DOMove(finishSlotPos.position, 0.5f)
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
        Finish(0);
    }
}
