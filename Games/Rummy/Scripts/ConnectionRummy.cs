using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;
using Features.Lobby.Integration;

public class ConnectionRummy : MonoBehaviour
{
    public AudioSource cardSound;
    public AudioSource cardSound2;
    public AudioSource cardSound3;
    public AudioSource cardshuffle_audio;
    public Transform tmpTransform;

    public GameObject invalidDeclaration_panel;
    public GameObject draggedCard;
    public bool _isFinishedByPlayer;
    public GameObject finishBtn;
    public GameObject discardBtn;
    public GameObject declarePanel;
    public Text opp_dPanel_timerTxt2;
    public Text opp_dPanel_timerTxt6;
    public Text dPanel_timerTxt;

    public long periodId;
    public Jugad databaseRef;
    public GameObject DropBtn;
    public float DropValue;
    Text dropvalueTxt;
    public GameObject AddBlockObj;
    public GameObject informationPanel;
    public ManagerRummy csBankRef;
    public ManagerRummy2Player managerRummy2PlayerRef;
    public float walletAmount;
    public float bootAmnt;
    public GameObject initialCardDistributor;
    public GameObject initialCardDistributor2Player;

    public GameObject manager;
    public GameObject manager2Player;

    public GameObject player6;
    public GameObject player2;
    public InitialCardDistributor initialCardDistributorRef;
    public InitialCardDistributor2Player initialCardDistributor2PlayerRef;

    public ManagerRummy managerRummyRef;
    public ManagerRummy2Player managerRummy2PlayerReff;
    public static string jokerRank;
    public static bool _isDroped;


    private void Awake()
    {
        DropBtn.SetActive(false);
        jokerRank = "";
        manager2Player.SetActive(false);
        initialCardDistributor2Player.SetActive(false);
        manager.SetActive(false);
        initialCardDistributor.SetActive(false);
        player6.SetActive(false);
        player2.SetActive(false);

        if (BootValueDecider.rummyPlayerCount == 2)
        {
            manager2Player.SetActive(true);
            initialCardDistributor2Player.SetActive(true);
            player2.SetActive(true);

        }
        else
        {
            manager.SetActive(true);
            initialCardDistributor.SetActive(true);
            player6.SetActive(true);

        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        databaseRef = FindObjectOfType<Jugad>();
        periodId = long.Parse(System.DateTime.Now.ToString("yyyyMMddHHmmss"));
        invalidDeclaration_panel.SetActive(false);

        _isDroped = false;
        dropvalueTxt = DropBtn.transform.GetChild(1).gameObject.GetComponent<Text>();
        DropValue = BootValueDecider.rummyBootValue * 10;
        dropvalueTxt.text = "-  ₹" + DropValue;


        AddBlockObj.SetActive(false);
        bootAmnt = 0;
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerRummy2PlayerRef = FindAnyObjectByType<ManagerRummy2Player>();
            initialCardDistributor2PlayerRef = FindObjectOfType<InitialCardDistributor2Player>();
            managerRummy2PlayerReff = FindObjectOfType<ManagerRummy2Player>();
        }
        else
        {
            csBankRef = FindAnyObjectByType<ManagerRummy>();
            initialCardDistributorRef = FindObjectOfType<InitialCardDistributor>();
            managerRummyRef = FindObjectOfType<ManagerRummy>();
        }
        databaseRef.SaveGameHistoryDatabase(periodId, "Rummy", DropValue);
        SetWalletAmouont(BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f);
        //Debug.LogError("Hum");
        //SetBootAmount(50f);
    }

    public void SetBootAmount(float value)
    {

        Debug.Log("Set Boot Ammount called");
        Debug.Log("Value reccievved for Boot is : " + value);
        /* csBankRef.bootAmt = value;
         csBankRef.chaalAmt = 2 * value;
         uiManager.BootUpdate(value);*/

        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerRummy2PlayerRef.myWallet -= value;
            managerRummy2PlayerRef.myWalletTxt.text = managerRummy2PlayerRef.myWallet.ToString();
        }
        else
        {
            csBankRef.myWallet -= value;
            csBankRef.myWalletTxt.text = csBankRef.myWallet.ToString();
        }
    }


    public void FinishBtn(bool _isDragged = true)
    {
        cardSound.Play();
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            if (!managerRummy2PlayerReff._isMyChance)
            {
                return;
            }
            if (!managerRummy2PlayerReff._isTakenCard)
            {
                return;
            }
            if (_isDragged)
            {
                Transform tempParent = draggedCard.transform.parent;
                draggedCard.transform.SetParent(tmpTransform);
                int total = 0;
                for (int i = 0; i < initialCardDistributor2PlayerRef.mainBlock.transform.childCount; i++)
                {
                    if (initialCardDistributor2PlayerRef.mainBlock.transform.GetChild(i).GetChild(0).childCount != 0)
                    {
                        total += initialCardDistributor2PlayerRef.mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
                    }
                }
                if (total != 0)
                {
                    invalidDeclaration_panel.SetActive(true);
                    Debug.Log("idhar : " + tempParent.name);
                    if (tempParent.childCount == 0)
                    {
                        Debug.Log("mamlat is null");
                        GameObject g = Instantiate(initialCardDistributor2PlayerRef.blockPrefab, initialCardDistributor2PlayerRef.mainBlock.transform);
                        g.transform.parent = initialCardDistributor2PlayerRef.mainBlock.transform;
                        draggedCard.transform.SetParent(g.transform.GetChild(0).transform);
                    }
                    else
                    {
                        draggedCard.transform.SetParent(tempParent);
                    }
                    return;
                }
            }
            else
            {

                //finish button se 2 player 
                int total = 0;
                Transform card = managerRummy2PlayerReff.selectedCardsList[0].transform;
                Transform tempParent = card.parent;
                Transform toSkip = null;

                if (tempParent.childCount > 1)
                {
                    card.SetParent(tmpTransform);
                    //invalidDeclaration_panel.SetActive(true);
                    //card.SetParent(tempParent);
                    //return;
                }
                else
                {
                    toSkip = tempParent.parent;
                }
                for (int i = 0; i < initialCardDistributor2PlayerRef.mainBlock.transform.childCount; i++)
                {
                    if (toSkip != null)
                    {
                        if (initialCardDistributor2PlayerRef.mainBlock.transform.GetChild(i) == toSkip)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (initialCardDistributor2PlayerRef.mainBlock.transform == card)
                        {
                            continue;
                        }
                    }
                    if (initialCardDistributor2PlayerRef.mainBlock.transform.GetChild(i).GetChild(0).childCount != 0)
                    {
                        total += initialCardDistributor2PlayerRef.mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
                    }
                }
                card.SetParent(tempParent);

                Debug.Log("total :  " + total);
                if (total != 0)
                {
                    invalidDeclaration_panel.SetActive(true);
                    if (tempParent.childCount == 0)
                    {
                        Debug.Log("mamlat is null");
                        GameObject g = Instantiate(initialCardDistributorRef.blockPrefab, initialCardDistributorRef.mainBlock.transform);
                        g.transform.parent = initialCardDistributorRef.mainBlock.transform;
                        draggedCard.transform.SetParent(g.transform.GetChild(0).transform);
                    }
                    else
                    {
                        draggedCard.transform.SetParent(tempParent);
                    }
                    return;
                }
            }

        }
        else
        {
            if (!managerRummyRef._isMyChance)
            {
                return;
            }
            if (!managerRummyRef._isTakenCard)
            {
                return;
            }
            if (_isDragged)
            {
                Transform tempParent = draggedCard.transform.parent;
                draggedCard.transform.SetParent(tmpTransform);
                int total = 0;
                for (int i = 0; i < initialCardDistributorRef.mainBlock.transform.childCount; i++)
                {

                    if (initialCardDistributorRef.mainBlock.transform.GetChild(i).GetChild(0).childCount != 0)
                    {
                        total += initialCardDistributorRef.mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
                    }
                }
                if (total != 0)
                {
                    invalidDeclaration_panel.SetActive(true);
                    draggedCard.transform.SetParent(tempParent);
                    return;
                }

            }

            else
            {
                //finish btn se 6 player k liye
                int total = 0;
                Transform card = managerRummyRef.selectedCardsList[0].transform;
                Transform tempParent = card.parent;
                Transform toSkip = null;

                if (tempParent.childCount > 1)
                {
                    card.SetParent(tmpTransform);
                    //invalidDeclaration_panel.SetActive(true);
                    //card.SetParent(tempParent);
                    //return;
                }
                else
                {
                    toSkip = tempParent.parent;
                    Debug.Log("to skip : " + toSkip.name);
                }

                for (int i = 0; i < initialCardDistributorRef.mainBlock.transform.childCount; i++)
                {
                    if (toSkip != null)
                    {
                        Debug.Log("to skip not null");
                        if (initialCardDistributorRef.mainBlock.transform.GetChild(i) == toSkip)
                        {
                            Debug.Log("skipped");
                            continue;
                        }
                    }
                    else
                    {
                        if (initialCardDistributorRef.mainBlock.transform == card)
                        {
                            Debug.Log("card hai ");
                            continue;
                        }
                    }
                    if (initialCardDistributorRef.mainBlock.transform.GetChild(i).GetChild(0).childCount != 0)
                    {
                        total += initialCardDistributorRef.mainBlock.transform.GetChild(i).GetComponent<CardsChecker>().blockSum;
                    }
                }

                card.SetParent(tempParent);



                if (total != 0)
                {
                    invalidDeclaration_panel.SetActive(true);
                    if (tempParent.childCount == 0)
                    {
                        Debug.Log("mamlat is null");
                        GameObject g = Instantiate(initialCardDistributorRef.blockPrefab, initialCardDistributorRef.mainBlock.transform);
                        g.transform.parent = initialCardDistributorRef.mainBlock.transform;
                        //draggedCard.transform.SetParent(g.transform.GetChild(0).transform);
                    }
                    else
                    {
                        Debug.Log("hudi baba");
                        //draggedCard.transform.SetParent(tempParent);
                    }
                    return;
                }
            }
        }
        //agar cards 14  nahi hai to return chk

        _isFinishedByPlayer = true;

        finishBtn.GetComponent<Image>().enabled = false;
        finishBtn.GetComponent<Button>().interactable = false;
        discardBtn.GetComponent<Image>().enabled = false;
        discardBtn.GetComponent<Button>().interactable = false;
        declarePanel.SetActive(true);
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            StopCoroutine(managerRummy2PlayerReff.myTimerCoroutine);
            managerRummy2PlayerReff.selectedCardsList[managerRummy2PlayerReff.selectedCardsList.Count - 1].transform.DOMove(managerRummy2PlayerReff.finishSlotPos.position, 0.5f)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
        });
            managerRummy2PlayerReff.selectedCardsList[managerRummy2PlayerReff.selectedCardsList.Count - 1].transform.SetParent(managerRummy2PlayerReff.finishSlotPos);
            //managerRummy2PlayerReff.selectedCardsList.Clear();
        }
        else
        {
            StopCoroutine(managerRummyRef.myTimerCoroutine);
            managerRummyRef.selectedCardsList[managerRummyRef.selectedCardsList.Count - 1].transform.DOMove(managerRummyRef.finishSlotPos.position, 0.5f)
       .SetEase(Ease.Linear)
       .OnComplete(() =>
       {

       });
            managerRummyRef.selectedCardsList[managerRummyRef.selectedCardsList.Count - 1].transform.SetParent(managerRummyRef.finishSlotPos);
            //managerRummyRef.selectedCardsList.Clear();
        }
        StartCoroutine(Timer(45));
    }


    public IEnumerator TimerOpp(int val)
    {
        float startTime = Time.realtimeSinceStartup;
        int remainingTime = val;

        // Loop until the timer reaches zero
        while (remainingTime > 0)
        {
            // Calculate remaining time based on real time passed
            remainingTime = val - Mathf.FloorToInt(Time.realtimeSinceStartup - startTime);

            // Update the timer texts
            opp_dPanel_timerTxt2.text = remainingTime.ToString();
            opp_dPanel_timerTxt6.text = remainingTime.ToString();

            yield return null; // Check the timer every frame
        }

        // Timer has finished, proceed with the rest of your logic
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerRummy2PlayerReff.Finish();
        }
        else
        {
            managerRummyRef.Finish();
        }

        declarePanel.SetActive(false);
    }


    public IEnumerator Timer(int val)
    {
        Debug.Log("Timer running");
        float startTime = Time.realtimeSinceStartup;
        int remainingTime = val;

        while (remainingTime > 0)
        {
            // Calculate the remaining time based on real time passed
            remainingTime = val - Mathf.FloorToInt(Time.realtimeSinceStartup - startTime);
            dPanel_timerTxt.text = remainingTime.ToString();
            yield return null; // Check the timer every frame
        }

        // Timer has finished, proceed with your logic
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            Debug.Log("idhar");
            if (!managerRummy2PlayerReff.finishPanel.activeInHierarchy)
            {
                Debug.Log("udhar");
                Finish();
            }
        }
        else
        {
            if (!managerRummyRef.finishPanel.activeInHierarchy)
            {
                Finish();
            }
        }
        declarePanel.SetActive(false);
    }


    public void SetWalletAmouont(float value)
    {

        walletAmount = value;
        Debug.LogWarning("hum " + BootValueDecider.rummyPlayerCount);
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerRummy2PlayerRef.myWallet = value;
            managerRummy2PlayerRef.myWalletTxt.text = value.ToString();
        }
        else
        {
            csBankRef.myWallet = value;
            csBankRef.myWalletTxt.text = value.ToString();
        }


    }

    public void SetBoolTwoPlayer(int val)
    {

    }

    public void Discard(GameObject card = null)
    {
        Debug.Log(DropValue + " & " + BootValueDecider.rummyBootValue * 10f);
        float x = BootValueDecider.rummyBootValue * 10f;
        if (DropValue == x)
        {
            Debug.Log("h");
            DropValue *= 2f;
            dropvalueTxt.text = "-  ₹" + DropValue.ToString("F2");
        }

        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerRummy2PlayerReff.Discard(card);
        }
        else
        {
            managerRummyRef.Discard(card);
        }
    }
    public void DiscardBtn()
    {
        cardSound.Play();
        float x = BootValueDecider.rummyBootValue * 10f;
        if (DropValue == x)
        {
            Debug.Log("h");
            DropValue *= 2f;
            dropvalueTxt.text = "-  ₹" + DropValue.ToString("F2");
        }

        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerRummy2PlayerReff.Discard(null);
        }
        else
        {
            managerRummyRef.Discard(null);

        }
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToLobby()
    {

        SceneManager.LoadScene(1);
    }

    public void DropWithLoss()
    {
        _isDroped = true;
        Debug.Log("status : " + _isDroped);
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerRummy2PlayerReff.DropWithLoss();
            Invoke("ReturnToLobby", 5f);

        }
        else
        {
            managerRummyRef.DropWithLoss();
        }

        if (DropValue > BootValueDecider.rummyBootValue * 10)
        {
            databaseRef.UpdateTeenpattiWinAmountGameHistory(periodId, (DropValue - BootValueDecider.rummyBootValue * 10));
        }
        //databaseRef.UpdateTeenpattiWinAmountGameHistory(periodId, DropValue);
    }


    public void Finish()
    {
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            managerRummy2PlayerReff.Finish(1);
        }
        else
        {
            managerRummyRef.Finish(1);

            /* if (initialCardDistributorRef.totalScore != 0)
             {
                 invalidDeclaration_panel.SetActive(true);
                 float v = BootValueDecider.rummyBootValue * 13;
                 Wallet.DeductAmount(v);
                 DeductAmount(v);
             }
             else
             {
                 managerRummyRef.Finish(1);
             }*/

        }
    }

    public void WinAmountUpdate(float val)
    {
        databaseRef.UpdateGameHistory(periodId, val + BootValueDecider.rummyBootValue * 10);
    }


    public void DeductAmount(float val)
    {
        if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < val)
        {
            val = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
        }
        databaseRef.UpdateTeenpattiWinAmountGameHistory(periodId, val - BootValueDecider.rummyBootValue * 10);
    }
    public void OpenCard()
    {
        cardSound.Play();
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            initialCardDistributor2PlayerRef.OpenCardRequest();
        }
        else
        {
            initialCardDistributorRef.OpenCardRequest();
        }
    }

    public void SortCards()
    {
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            initialCardDistributor2PlayerRef.SortCardsButton();
        }
        else
        {
            initialCardDistributorRef.SortCardsButton();
        }
    }

    public void Information()
    {
        informationPanel.SetActive(true);
    }

    public void CloseInformation()
    {
        informationPanel.SetActive(false);
    }

    public void Group()
    {
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            initialCardDistributor2PlayerRef.Group();

        }
        else
        {
            initialCardDistributorRef.Group();

        }
    }

}
