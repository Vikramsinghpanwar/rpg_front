using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ManagerMines : MonoBehaviour
{
    public GameObject betRoda;
    public Button plusBtn, minusBtn;
    public Transform mineBoxContainer;
    public long periodId;
    public Jugad dbRef;
    public TextMeshProUGUI YouWinTxt;
    public TextMeshProUGUI NextWinTxt;
    public TextMeshProUGUI YouWinMultiplierTxt;
    public TextMeshProUGUI NextWinMultiplierTxt;
    public Multipliers multiplierRef;
    public float winningAmnt;
    public int winningBox;
    public ScrollRect scrollRect;
    public float scrollSpeed = 1000f;
    public float scrollAmount = 50f;
    public GameObject[][] coverObjects;
    public Image[][] hiddenObjects;
    public int minesCount;
    public Sprite mine, gold;
    public TextMeshProUGUI mineCountTxt;
    bool _cashOut;
    bool _canSpin;
    public GameObject CashoutBtn;
    public Button SpinBtn;
    public BetManagerMines bManagerScript;
    public List<GameObject> coverObjectList;
    public List<Image> hiddenObjectList;
    float YouMultiplier;
    // Start is called before the first frame update
    void Start()
    {
        dbRef = FindObjectOfType<Jugad>();
        multiplierRef = FindObjectOfType<Multipliers>();
        bManagerScript = FindObjectOfType<BetManagerMines>();
        CashoutBtn.SetActive(false);
        SpinBtn.interactable = false;
        minesCount = 2;
        _canSpin = true;
        coverObjects = new GameObject[5][];
        for (int i = 0; i < 5; i++)
        {
            coverObjects[i] = new GameObject[5];
            for (int j = 0; j < 5; j++)
            {
                coverObjects[i][j] = coverObjectList[i * 5 + j];
            }
        }

        // Convert hiddenObjectList to a 2D array
        hiddenObjects = new Image[5][];
        for (int i = 0; i < 5; i++)
        {
            hiddenObjects[i] = new Image[5];
            for (int j = 0; j < 5; j++)
            {
                hiddenObjects[i][j] = hiddenObjectList[i * 5 + j];
            }
        }


        YouWinMultiplierTxt.text = "0X";
        YouWinTxt.text = "0.00";
        BoxNum_TextHandler();
    }

    // Update is called once per frame
  
    public void AddMines()
    {
        if(minesCount < 24)
        {
            mineBoxContainer.GetChild(minesCount - 2).gameObject.SetActive(false);
            minesCount++;
            mineCountTxt.text = minesCount.ToString();
            SetMultipliersOnPlaceBet();
            BoxNum_TextHandler();
            //multiplierRef.UpdateMultipliers(minesCount);

        }
    }

    void BoxNum_TextHandler()
    {
        int num = 1;
        for(int i = 0; i<mineBoxContainer.childCount; i++)
        {
            GameObject t = mineBoxContainer.GetChild(i).gameObject;
            if (t.activeInHierarchy)
            {
                t.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = num.ToString();
                num++;
            }
        }
    }
  
    public void SetMultiplierInGame(int mineCnt)
    {
        float NxtMultiplier = multiplierRef.rewardMultipliers[minesCount - 1 + mineCnt];
        YouMultiplier = multiplierRef.rewardMultipliers[minesCount - 2 + mineCnt];

        NextWinMultiplierTxt.text = NxtMultiplier.ToString() + "X";
        YouWinMultiplierTxt.text = YouMultiplier.ToString() + "X";

        YouWinTxt.text = (bManagerScript.totalBetAmt * YouMultiplier).ToString();
        NextWinTxt.text = (bManagerScript.totalBetAmt * NxtMultiplier).ToString();

        winningAmnt = YouMultiplier* bManagerScript.totalBetAmt;
    }




    public void SetMultipliersOnPlaceBet()
    {
        float NxtMultiplier = multiplierRef.rewardMultipliers[minesCount -2];

        NextWinMultiplierTxt.text = NxtMultiplier.ToString() + "X";

        NextWinTxt.text = (bManagerScript.totalBetAmt * NxtMultiplier).ToString();
    }


    public void SubtractMines()
    {
        if (minesCount > 2)
        {
            minesCount--;
            mineCountTxt.text = minesCount.ToString();
            SetMultipliersOnPlaceBet();
            mineBoxContainer.GetChild(minesCount-2).gameObject.SetActive(true);
            BoxNum_TextHandler();
            //multiplierRef.UpdateMultipliers(minesCount);

        }
    }

    public void ClearBoard()
    {
        betRoda.SetActive(false);

        for (int i =0; i<hiddenObjects.Length; i++)
        {
            for(int y = 0; y< hiddenObjects.Length; y++)
            {
                hiddenObjects[i][y].sprite = gold;
            }
        }
        _canSpin = true;
        SpinBtn.interactable = false;
        plusBtn.interactable = true;
        minusBtn.interactable = true;
    }
    public void SetMines()
    {
        for(int i = 0; i<5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                hiddenObjects[i][j].sprite = gold;

            }
        }
        Debug.Log("setting Mines : " + minesCount);
        bool isDuplicate;
        int tx, ty;
        List<int[]> exVal = new List<int[]>();
        exVal.Clear();
        for(int i =0;i<minesCount - 1; i++)
        {
            do
            {
                tx = Random.Range(0, 5);
                ty = Random.Range(0, 5);

            }
            while (isDuplicate = exVal.Any(pair => pair[0] == tx && pair[1] == ty));
            hiddenObjects[tx][ty].sprite = mine;
            exVal.Add(new int[] { tx, ty });
            Debug.Log("tx : " + tx + " and ty : " + ty);
        }

        for(int i = 0; i< 5; i++)
        {
            for (int j = 0; j< 5; j++)
            {
                string s;
                if(hiddenObjects[i][j].sprite != null)
                {
                    if (hiddenObjects[i][j].sprite.name != "mine")
                    {
                        hiddenObjects[i][j].sprite = gold;
                    }
                }
                else
                {
                    hiddenObjects[i][j].sprite = gold;

                }


            }
        }
    }
    public void RoundCompleted()
    {
        betRoda.SetActive(false);

        bManagerScript.Clear();
        foreach (GameObject g in coverObjectList)
        {
            g.GetComponent<Image>().color = new Color(255, 255, 255, 1f);
            g.SetActive(true);
        }
        ClearBoard();
    }

    public void Spin()
    {
        periodId = long.Parse(System.DateTime.Now.ToString("yyyyMMddHHmmss"));
        if (!_canSpin)
        {
            return;
        }
        plusBtn.interactable = false;
        minusBtn.interactable = false;
        winningBox = 0;
        winningAmnt = 0;
        if(bManagerScript.totalBetAmt > 0)
        {
            betRoda.SetActive(true);
            bManagerScript.walletAmt -= bManagerScript.totalBetAmt;
            Wallet.DeductAmount(bManagerScript.totalBetAmt);
            bManagerScript.walletTxt.text = "₹ " + bManagerScript.walletAmt.ToString("F2");
            SpinBtn.interactable =false;
            dbRef.SaveGameHistoryDatabase(periodId, "Mines", bManagerScript.totalBetAmt);
            _cashOut = false;
            SetMines();
            CashoutBtn.SetActive(true);
            CashoutBtn.GetComponent<Button>().interactable = false;


        }
    }

    

    public void CashOut()
    {        
        _cashOut = true;
        CashoutBtn.SetActive(false);
        SpinBtn.interactable = false;
        winningAmnt = bManagerScript.totalBetAmt * (YouMultiplier);
        Wallet.DeductFromPool(winningAmnt);
        bManagerScript.UpdateWallet(winningAmnt);
        dbRef.UpdateGameHistory(periodId,winningAmnt);

        Reset();
    }

    public void Reset()
    {
        betRoda.SetActive(false);
        Debug.Log("Resetting");
        bManagerScript.Clear();
        float NxtMultiplier = multiplierRef.rewardMultipliers[minesCount - 1];
        YouMultiplier = 0;

        NextWinMultiplierTxt.text = NxtMultiplier.ToString() + "X";
        YouWinMultiplierTxt.text = YouMultiplier.ToString() + "X";

        YouWinTxt.text = (bManagerScript.totalBetAmt * YouMultiplier).ToString();
        NextWinTxt.text = (bManagerScript.totalBetAmt * NxtMultiplier).ToString();
      
        RoundCompleted();
    }

    public void ScrollLeft()
    {
        float targetScrollPosition = Mathf.Clamp(scrollRect.content.anchoredPosition.x - scrollAmount, 0f, scrollRect.content.rect.width - scrollRect.viewport.rect.width);
        StartCoroutine(ScrollToPosition(targetScrollPosition));
    }

    public void ScrollRight()
    {
        float targetScrollPosition = Mathf.Clamp(scrollRect.content.anchoredPosition.x + scrollAmount, 0f, scrollRect.content.rect.width - scrollRect.viewport.rect.width);
        StartCoroutine(ScrollToPosition(targetScrollPosition));
    }

    private IEnumerator ScrollToPosition(float targetPosition)
    {
        float currentScrollPosition = scrollRect.content.anchoredPosition.x;
        float startTime = Time.time;
        float journeyLength = Mathf.Abs(targetPosition - currentScrollPosition);

        while (currentScrollPosition != targetPosition)
        {
            float distCovered = (Time.time - startTime) * scrollSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            currentScrollPosition = Mathf.Lerp(currentScrollPosition, targetPosition, fractionOfJourney);
            scrollRect.content.anchoredPosition = new Vector2(currentScrollPosition, scrollRect.content.anchoredPosition.y);
            yield return null;
        }
    }

    public void Lobby()
    {
        SceneManager.LoadScene(1);
    }


    public void OpenBlock(Transform block)
    {

        if (!CashoutBtn.activeInHierarchy)
        {
            return;
        }

      
        if (block.GetChild(0).gameObject.GetComponent<Image>().sprite.name == "mine")
        {

            Debug.Log("bomb p click kr diya hai");
            int p = Random.Range(0, 3);
            int q = Random.Range(0, 4);
            Debug.Log("naya bomb set kiya hai : " + p + q);
            bool _isSet = false;
            for (int i = p; i < 5; i++)
            {
                for (int j = q; j < 5; j++)
                {
                    if (hiddenObjects[i][j].sprite == gold)
                    {
                        hiddenObjects[i][j].sprite = mine;
                        _isSet = true;
                    }
                    if (_isSet)
                    {
                        break;
                    }
                }
                if (_isSet)
                {
                    break;
                }
            }
            if (!_isSet)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (hiddenObjects[i][j].sprite == gold)
                        {
                            hiddenObjects[i][j].sprite = mine;
                            _isSet = true;
                        }
                        if (_isSet)
                        {
                            break;
                        }
                    }
                    if (_isSet)
                    {
                        break;
                    }
                }
            }
        }
        else
        {
            if(winningBox == 0)
            {
                CashoutBtn.GetComponent<Button>().interactable = true;
            }
            int k = Random.Range(0, 1000);
            int z = 0;
            switch (winningBox)
            {
                case 0:
                    z = 700;
                    break;
                case 1:
                    z = 600;
                    break;
                case 2:
                    z = 500;
                    break;
                case 3:
                    z = 400;
                    break;
                case 4:
                    z = 300;
                    break;
                case 5:
                    z = 200;
                    break;
                case 6:
                    z = 100;
                    break;
                default:
                    z = 0;
                    break;
            }

            if (k > z || minesCount > 15 || winningAmnt >= Wallet.GetPool())
            {
                Debug.Log("Your pool is : " + Wallet.GetPool());
                Debug.Log("bomb laga diya hai");
                block.GetChild(0).gameObject.GetComponent<Image>().sprite = mine;
            }
        }
            
      
       if(block.GetChild(0).gameObject.GetComponent<Image>().sprite.name == "mine")
       {
            //bomb
            CashoutBtn.SetActive(false);
            Wallet.AddToPool(bManagerScript.totalBetAmt);
            foreach (GameObject g in coverObjectList)
            {
                g.GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
            }
            Invoke("RoundCompleted", 2f);

        }
        else
       {
            if (block.GetChild(1).gameObject.activeInHierarchy)
            {
                block.GetChild(1).gameObject.SetActive(false);
                SetMultiplierInGame(winningBox);
                winningBox++;

            }

        }
    }
}
