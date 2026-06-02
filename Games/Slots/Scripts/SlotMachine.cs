using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Features.Lobby.Integration;


public class SlotMachine : MonoBehaviour
{
    public GameObject bigWinPanel;
    public GameObject megaWinPanel;
    public GameObject epicWinPanel;
    public TextMeshProUGUI bigWinAmountTxt;
    public TextMeshProUGUI megaWinAmountTxt;
    public TextMeshProUGUI epicWinAmountTxt;
    bool _isPos1;
    bool _isPos2;
    bool _isPos3;
    public bool _autoSpin;
    string gameName;
    long periodId;
    public Jugad dbRef;
    public GameObject freeSpinOject;
    public GameObject freeSpinOjectS;
    public GameObject insufficientFundsObj;
    public int freeSpin;
    public TextMeshProUGUI winAmntTxt;
    public float winAmnt;
    public float win3 = 1.2f;
    public float win4 = 2f;
    public float win5 = 3f;
    public AudioSource spinStart, spinEnd, spinSound, winAudio;
    public Reels reel1;
    public Reels reel2;
    public Reels reel3;
    public Reels reel4;
    public Reels reel5;
    public bool _isSpinning;
    string[][] arrays;
    public ManagerSlot managerScript;
    public BetManagerSlot bManagerScript;
    public float glowDuration = 1.5f;
    Coroutine winChkV;
    Coroutine winChkH;
    int k = 0;
    List<Animator> allSymbolAnimatorsList;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            reel1.myReel.spot4Img.sprite = reel1.myReel.slotImages[7].sprite;
            reel1.myReel.spot3Img.sprite = reel1.myReel.slotImages[7].sprite;

            reel2.myReel.spot2Img.sprite = reel1.myReel.slotImages[7].sprite;
            reel2.myReel.spot4Img.sprite = reel1.myReel.slotImages[7].sprite;

            reel3.myReel.spot3Img.sprite = reel1.myReel.slotImages[7].sprite;
            reel4.myReel.spot3Img.sprite = reel1.myReel.slotImages[7].sprite;
            reel5.myReel.spot3Img.sprite = reel1.myReel.slotImages[7].sprite;

        }
    }


    private void Awake()
    {
        freeSpin = 0;
        Application.targetFrameRate = 90;
        managerScript = FindObjectOfType<ManagerSlot>();
        bManagerScript = FindObjectOfType<BetManagerSlot>();
    }


    private void Start()
    {
        bigWinPanel.SetActive(true);
        megaWinPanel.SetActive(true);
        epicWinPanel.SetActive(true);
        bigWinAmountTxt = bigWinPanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        megaWinAmountTxt = megaWinPanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        epicWinAmountTxt = epicWinPanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        bigWinPanel.SetActive(false);
        megaWinPanel.SetActive(false);
        epicWinPanel.SetActive(false);
        _autoSpin = false;
        gameName = SceneManager.GetActiveScene().name;
        dbRef = FindObjectOfType<Jugad>();
        insufficientFundsObj.SetActive(false);
        freeSpinOject.SetActive(false);
        freeSpinOjectS.SetActive(false);
        _isSpinning = false;
        managerScript = FindObjectOfType<ManagerSlot>();
        bManagerScript = FindObjectOfType<BetManagerSlot>();

        allSymbolAnimatorsList = new List<Animator>();
        for (int i = 1; i < 4; i++)
        {
            allSymbolAnimatorsList.Add(reel1.transform.GetChild(i).GetComponent<Animator>());
            allSymbolAnimatorsList.Add(reel2.transform.GetChild(i).GetComponent<Animator>());
            allSymbolAnimatorsList.Add(reel3.transform.GetChild(i).GetComponent<Animator>());
            allSymbolAnimatorsList.Add(reel4.transform.GetChild(i).GetComponent<Animator>());
            allSymbolAnimatorsList.Add(reel5.transform.GetChild(i).GetComponent<Animator>());
        }

        StopAllAnimation();
        StopAllCoroutines();
        periodId = 0;

    }



    public void WinChk()
    {
        CalculateAmount();
        winAmntTxt.text = winAmnt.ToString("F2");
        managerScript.WalletUpdate(winAmnt);
        Debug.Log("win amount : " + winAmnt);
        dbRef.UpdateGameHistory(periodId, winAmnt);
        StartCoroutine(ChkForScatter());
    }

    void CalculateAmount()
    {
        string[] reel1Imgs = new string[3];
        string[] reel2Imgs = new string[3];
        string[] reel3Imgs = new string[3];
        string[] reel4Imgs = new string[3];
        string[] reel5Imgs = new string[3];

        reel1Imgs[0] = reel1.myReel.spot2Img.sprite.name;
        reel1Imgs[1] = reel1.myReel.spot3Img.sprite.name;
        reel1Imgs[2] = reel1.myReel.spot4Img.sprite.name;

        reel2Imgs[0] = reel2.myReel.spot2Img.sprite.name;
        reel2Imgs[1] = reel2.myReel.spot3Img.sprite.name;
        reel2Imgs[2] = reel2.myReel.spot4Img.sprite.name;

        reel3Imgs[0] = reel3.myReel.spot2Img.sprite.name;
        reel3Imgs[1] = reel3.myReel.spot3Img.sprite.name;
        reel3Imgs[2] = reel3.myReel.spot4Img.sprite.name;

        reel4Imgs[0] = reel4.myReel.spot2Img.sprite.name;
        reel4Imgs[1] = reel4.myReel.spot3Img.sprite.name;
        reel4Imgs[2] = reel4.myReel.spot4Img.sprite.name;

        reel5Imgs[0] = reel5.myReel.spot2Img.sprite.name;
        reel5Imgs[1] = reel5.myReel.spot3Img.sprite.name;
        reel5Imgs[2] = reel5.myReel.spot4Img.sprite.name;

        arrays = new string[][]
        {
            reel1Imgs,
            reel2Imgs,
            reel3Imgs,
            reel4Imgs,
            reel5Imgs
        };
        WinAmntCalculator();
    }
    public IEnumerator ChkForScatter()
    {
        List<Position> positionToGlow = new List<Position>();
        int scatterCount = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (arrays[i][j] == "scatter")
                {
                    positionToGlow.Add(new Position(i, j));
                    scatterCount++;
                }
            }
        }
        if (scatterCount == 1)
        {
            freeSpin += 2;
            freeSpinOject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "+2";
        }
        else if (scatterCount == 2)
        {
            freeSpin += 5;
            freeSpinOject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "+5";

        }
        else if (scatterCount >= 3)
        {
            freeSpin += 10;
            freeSpinOject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "+10";

        }
        if (scatterCount > 0)
        {
            bManagerScript._canPress = false;
            freeSpinOject.SetActive(true);
            freeSpinOjectS.SetActive(true);
            for (int i = 0; i < positionToGlow.Count; i++)
            {
                GlowBox(positionToGlow[i].Row, positionToGlow[i].Column);
                yield return new WaitForSeconds(glowDuration);
                StopAllAnimation();
            }
            yield return new WaitForSeconds(1f);
            freeSpinOject.SetActive(false);
        }

        else if (freeSpin == 0)
        {
            bManagerScript._canPress = true;
            freeSpinOjectS.SetActive(false);
        }

        winChkH = StartCoroutine(LoopHChecker());

    }
    void WinAmntCalculator()
    {
        List<Position> positions1 = new List<Position>();
        List<Position> positions2 = new List<Position>();
        List<Position> positions3 = new List<Position>();

        positions1.Add(new Position(0, 0));
        positions2.Add(new Position(0, 1));
        positions3.Add(new Position(0, 2));
        List<string> excludedValues = new List<string>();

        string ks = "";
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                ks += arrays[i][j] + " ";
            }
            ks += "\n";

        }


        int multiplier1 = 1;
        int multiplier2 = 1;
        int multiplier3 = 1;
        for (int i = 1; i < 5; i++)
        {
            bool _is = false;
            for (int j = 0; j < 3; j++)
            {
                if (arrays[i][j].Contains(arrays[0][0]) || arrays[i][j].Contains("wild") && positions1.Count >= i)
                {
                    int duplicates = 1;
                    for (int s = j + 1; s < 3; s++)
                    {
                        if (arrays[i][s].Contains(arrays[0][0]) || arrays[i][s].Contains("wild"))
                        {
                            duplicates++;
                        }
                    }
                    multiplier1 *= duplicates;
                    positions1.Add(new Position(i, j));
                    _is = true;
                    break;
                }
            }
            if (!_is)
            {

                break;
            }
        }

        for (int i = 1; i < 5; i++)
        {
            bool _is = false;
            for (int j = 0; j < 3; j++)
            {
                if (arrays[i][j].Contains(arrays[0][1]) || arrays[i][j].Contains("wild") && positions2.Count >= i)
                {
                    int duplicates = 1;
                    for (int s = j + 1; s < 3; s++)
                    {
                        if (arrays[i][s].Contains(arrays[0][1]) || arrays[i][s].Contains("wild"))
                        {
                            duplicates++;
                        }
                    }
                    multiplier2 *= duplicates;
                    positions2.Add(new Position(i, j));
                    _is = true;

                    break;
                }
            }
            if (!_is)
            {
                break;
            }
        }

        for (int i = 1; i < 5; i++)
        {
            bool _is = false;
            for (int j = 0; j < 3; j++)
            {
                if (arrays[i][j].Contains(arrays[0][2]) || arrays[i][j].Contains("wild") && positions3.Count >= i)
                {
                    int duplicates = 1;
                    for (int s = j + 1; s < 3; s++)
                    {
                        if (arrays[i][s].Contains(arrays[0][2]) || arrays[i][s].Contains("wild"))
                        {
                            duplicates++;
                        }
                    }
                    multiplier3 *= duplicates;
                    positions3.Add(new Position(i, j));
                    _is = true;

                    break;
                }
            }
            if (!_is)
            {
                break;
            }
        }



        float k = 0f;
        float l = 0f;
        float m = 0f;


        if (positions1.Count > 2)
        {
            _isPos1 = true;
            if (arrays[0][0] == "wild")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 1f;
                        break;
                    case 4:
                        k = 6f;
                        break;
                    case 5:
                        k = 12f;
                        break;
                }
            }
            else if (arrays[0][0] == "scatter")
            {

            }
            else if (arrays[0][0] == "1")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 1f;
                        break;
                    case 4:
                        k = 6f;
                        break;
                    case 5:
                        k = 12f;
                        break;
                }
            }
            else if (arrays[0][0] == "2")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 0.6f;
                        break;
                    case 4:
                        k = 4f;
                        break;
                    case 5:
                        k = 10f;
                        break;
                }
            }
            else if (arrays[0][0] == "3")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 0.6f;
                        break;
                    case 4:
                        k = 4f;
                        break;
                    case 5:
                        k = 10f;
                        break;
                }
            }
            else if (arrays[0][0] == "4")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 0.6f;
                        break;
                    case 4:
                        k = 2f;
                        break;
                    case 5:
                        k = 8f;
                        break;
                }
            }
            else if (arrays[0][0] == "5")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 0.2f;
                        break;
                    case 4:
                        k = 1.2f;
                        break;
                    case 5:
                        k = 4f;
                        break;
                }
            }
            else if (arrays[0][0] == "6")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 0.2f;
                        break;
                    case 4:
                        k = 1.2f;
                        break;
                    case 5:
                        k = 4f;
                        break;
                }
            }
            else if (arrays[0][0] == "7")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 0.2f;
                        break;
                    case 4:
                        k = 0.8f;
                        break;
                    case 5:
                        k = 4f;
                        break;
                }
            }
            else if (arrays[0][0] == "8")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 0.2f;
                        break;
                    case 4:
                        k = 0.6f;
                        break;
                    case 5:
                        k = 3f;
                        break;
                }
            }
            else if (arrays[0][0] == "9")
            {
                switch (positions1.Count)
                {
                    case 3:
                        k = 0.2f;
                        break;
                    case 4:
                        k = 0.6f;
                        break;
                    case 5:
                        k = 3f;
                        break;
                }
            }

            winAmnt = bManagerScript.betValue * k;
        }
        else
        {
            _isPos1 = false;
        }
        if (positions2.Count > 2)
        {
            _isPos2 = true;

            if (arrays[0][1] == "wild")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 1f;
                        break;
                    case 4:
                        l = 6f;
                        break;
                    case 5:
                        l = 12f;
                        break;
                }
            }
            else if (arrays[0][1] == "scatter")
            {

            }
            else if (arrays[0][1] == "1")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 1f;
                        break;
                    case 4:
                        l = 6f;
                        break;
                    case 5:
                        l = 12f;
                        break;
                }
            }
            else if (arrays[0][1] == "2")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 0.6f;
                        break;
                    case 4:
                        l = 4f;
                        break;
                    case 5:
                        l = 10f;
                        break;
                }
            }
            else if (arrays[0][1] == "3")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 0.6f;
                        break;
                    case 4:
                        l = 4f;
                        break;
                    case 5:
                        l = 10f;
                        break;
                }
            }
            else if (arrays[0][1] == "4")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 0.6f;
                        break;
                    case 4:
                        l = 2f;
                        break;
                    case 5:
                        l = 8f;
                        break;
                }
            }
            else if (arrays[0][1] == "5")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 0.2f;
                        break;
                    case 4:
                        l = 1.2f;
                        break;
                    case 5:
                        l = 4f;
                        break;
                }
            }
            else if (arrays[0][1] == "6")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 0.2f;
                        break;
                    case 4:
                        l = 1.2f;
                        break;
                    case 5:
                        l = 4f;
                        break;
                }
            }
            else if (arrays[0][1] == "7")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 0.2f;
                        break;
                    case 4:
                        l = 0.8f;
                        break;
                    case 5:
                        l = 4f;
                        break;
                }
            }
            else if (arrays[0][1] == "8")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 0.2f;
                        break;
                    case 4:
                        l = 0.6f;
                        break;
                    case 5:
                        l = 3f;
                        break;
                }
            }
            else if (arrays[0][1] == "9")
            {
                switch (positions2.Count)
                {
                    case 3:
                        l = 0.2f;
                        break;
                    case 4:
                        l = 0.6f;
                        break;
                    case 5:
                        l = 3f;
                        break;
                }
            }

            winAmnt = bManagerScript.betValue * k;
        }
        else
        {
            _isPos2 = false;
        }
        if (positions3.Count > 2)
        {
            _isPos3 = true;

            if (arrays[0][2] == "wild")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 1f;
                        break;
                    case 4:
                        m = 6f;
                        break;
                    case 5:
                        m = 12f;
                        break;
                }
            }
            else if (arrays[0][2] == "scatter")
            {

            }
            else if (arrays[0][2] == "1")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 1f;
                        break;
                    case 4:
                        m = 6f;
                        break;
                    case 5:
                        m = 12f;
                        break;
                }
            }
            else if (arrays[0][2] == "2")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 0.6f;
                        break;
                    case 4:
                        m = 4f;
                        break;
                    case 5:
                        m = 10f;
                        break;
                }
            }
            else if (arrays[0][2] == "3")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 0.6f;
                        break;
                    case 4:
                        m = 4f;
                        break;
                    case 5:
                        m = 10f;
                        break;
                }
            }
            else if (arrays[0][2] == "4")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 0.6f;
                        break;
                    case 4:
                        m = 2f;
                        break;
                    case 5:
                        m = 8f;
                        break;
                }
            }
            else if (arrays[0][2] == "5")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 0.2f;
                        break;
                    case 4:
                        m = 1.2f;
                        break;
                    case 5:
                        m = 4f;
                        break;
                }
            }
            else if (arrays[0][2] == "6")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 0.2f;
                        break;
                    case 4:
                        m = 1.2f;
                        break;
                    case 5:
                        m = 4f;
                        break;
                }
            }
            else if (arrays[0][2] == "7")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 0.2f;
                        break;
                    case 4:
                        m = 0.8f;
                        break;
                    case 5:
                        m = 4f;
                        break;
                }
            }
            else if (arrays[0][2] == "8")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 0.2f;
                        break;
                    case 4:
                        m = 0.6f;
                        break;
                    case 5:
                        m = 3f;
                        break;
                }
            }
            else if (arrays[0][2] == "9")
            {
                switch (positions3.Count)
                {
                    case 3:
                        m = 0.2f;
                        break;
                    case 4:
                        m = 0.6f;
                        break;
                    case 5:
                        m = 3f;
                        break;
                }
            }

        }
        else
        {
            _isPos3 = false;
        }
        float netWin = (k * multiplier1) + (l * multiplier2) + (m * multiplier3);
        winAmnt = bManagerScript.betValue * netWin;

        if (netWin >= 7)
        {
            epicWinPanel.SetActive(true);
            epicWinAmountTxt.text = winAmnt.ToString("F2");
            Invoke("BigWinDeactivate", 5);
        }
        else if (netWin >= 5)
        {
            megaWinPanel.SetActive(true);
            megaWinAmountTxt.text = winAmnt.ToString("F2");
            Invoke("BigWinDeactivate", 5);
        }
        else if (netWin > 3)
        {
            bigWinPanel.SetActive(true);
            bigWinAmountTxt.text = winAmnt.ToString("F2");
            Invoke("BigWinDeactivate", 5);
        }

        //Debug.Log("Win amount calculated : \n1. count of first : " + k + "\n2. count of second : " + l + "\n3. Count of m : " + m + "\n\n1. multiplier 1 : " + multiplier1 + "\n2. multiplier2 : " + multiplier2 + "\n3. multiplier 3 : " + multiplier3);
    }

    public void BigWinDeactivate()
    {
        bigWinPanel.SetActive(false);
        megaWinPanel.SetActive(false);
        epicWinPanel.SetActive(false);
    }


    public IEnumerator LoopHChecker()
    {
        if (_isPos1)
        {
            List<Position> positions1 = new List<Position>();
            positions1.Add(new Position(0, 0));
            for (int i = 1; i < 5; i++)
            {
                bool _is = false;

                for (int j = 0; j < 3; j++)
                {
                    if (arrays[i][j].Contains(arrays[0][0]) || arrays[i][j].Contains("wild") && positions1.Count >= i)
                    {
                        positions1.Add(new Position(i, j));
                        _is = true;
                        //break;
                    }
                }
                if (!_is)
                {
                    break;
                }
            }



            if (positions1.Count > 2)
            {
                for (int i = 0; i < positions1.Count; i++)
                {
                    GlowBox(positions1[i].Row, positions1[i].Column);
                }
                /* yield return new WaitForSeconds(glowDuration);
                 StopAllAnimation();*/
            }



        }

        if (_isPos2)
        {
            List<Position> positions2 = new List<Position>();
            positions2.Add(new Position(0, 1));
            for (int i = 1; i < 5; i++)
            {
                bool _is = false;
                for (int j = 0; j < 3; j++)
                {
                    if (arrays[i][j].Contains(arrays[0][1]) || arrays[i][j].Contains("wild") && positions2.Count >= i)
                    {
                        positions2.Add(new Position(i, j));
                        _is = true;
                        //break;
                    }
                }
                if (!_is)
                {
                    break;
                }
            }



            if (positions2.Count > 2)
            {
                for (int i = 0; i < positions2.Count; i++)
                {
                    GlowBox(positions2[i].Row, positions2[i].Column);
                }
                /* yield return new WaitForSeconds(glowDuration);
                 StopAllAnimation();*/
            }





        }

        if (_isPos3)
        {
            List<Position> positions3 = new List<Position>();

            positions3.Add(new Position(0, 2));

            for (int i = 1; i < 5; i++)
            {
                bool _is = false;
                for (int j = 0; j < 3; j++)
                {
                    if (arrays[i][j].Contains(arrays[0][2]) || arrays[i][j].Contains("wild") && positions3.Count >= i)
                    {
                        positions3.Add(new Position(i, j));
                        _is = true;
                        //break;
                    }
                }
                if (!_is)
                {
                    break;
                }
            }


            if (positions3.Count > 2)
            {
                for (int i = 0; i < positions3.Count; i++)
                {
                    GlowBox(positions3[i].Row, positions3[i].Column);
                }
                /* yield return new WaitForSeconds(glowDuration);
                 StopAllAnimation();*/
            }
        }



        yield return new WaitForSeconds(glowDuration);
        StopAllAnimation();

        if (bigWinPanel.activeInHierarchy)
        {
            yield return new WaitForSeconds(4);
        }










        yield return new WaitForSeconds(1);
        if (_autoSpin)
        {
            Spin();
        }

    }
    public void StopAllAnimation()
    {
        foreach (Animator a in allSymbolAnimatorsList)
        {
            a.SetBool("_isMatch", false);
        }

    }





    void GlowBox(int x, int y)
    {
        winAudio.Play();
        switch (x)
        {
            case 0:
                switch (y)
                {
                    case 0:
                        reel1.transform.GetChild(1).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 1:
                        reel1.transform.GetChild(2).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 2:
                        reel1.transform.GetChild(3).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                }
                break;
            case 1:
                switch (y)
                {
                    case 0:
                        reel2.transform.GetChild(1).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 1:
                        reel2.transform.GetChild(2).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 2:
                        reel2.transform.GetChild(3).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                }
                break;
            case 2:
                switch (y)
                {
                    case 0:
                        reel3.transform.GetChild(1).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 1:
                        reel3.transform.GetChild(2).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 2:
                        reel3.transform.GetChild(3).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                }
                break;

            case 3:
                switch (y)
                {
                    case 0:
                        reel4.transform.GetChild(1).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 1:
                        reel4.transform.GetChild(2).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 2:
                        reel4.transform.GetChild(3).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                }
                break;

            case 4:
                switch (y)
                {
                    case 0:
                        reel5.transform.GetChild(1).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 1:
                        reel5.transform.GetChild(2).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                    case 2:
                        reel5.transform.GetChild(3).GetComponent<Animator>().SetBool("_isMatch", true);
                        break;
                }
                break;

        }
    }



    public void ToogleAutoSpin()
    {
        _autoSpin = !_autoSpin;
    }


    void DummySpin()
    {
        StopAllAnimation();
        StopAllCoroutines();
        if (winChkH != null)
        {
            StopCoroutine(winChkH);
        }
        _isSpinning = true;
        bManagerScript._canPress = false;
        winAmnt = 0f;
        winAmntTxt.text = "0.00";

        winAudio.Stop();
        StartCoroutine(SpinReelsDummy());



    }


    public void Spin()
    {


        if (freeSpin > 0)
        {
            if (!_isSpinning)
            {

                if (managerScript.wallet >= bManagerScript.betValue)
                {
                    StopAllAnimation();
                    StopAllCoroutines();
                    if (winChkH != null)
                    {
                        StopCoroutine(winChkH);
                    }
                    periodId = long.Parse(System.DateTime.Now.ToString("yyyyMMddHHmmss"));
                    _isSpinning = true;
                    bManagerScript._canPress = false;
                    winAmnt = 0f;
                    winAmntTxt.text = "0.00";

                    winAudio.Stop();
                    StartCoroutine(SpinReels());
                    spinStart.Play();
                    dbRef.SaveGameHistoryDatabase(periodId, gameName, 0);

                }
                else
                {
                    insufficientFundsObj.SetActive(true);
                }
            }
            freeSpin--;
        }
        else if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < 50)
        {
            insufficientFundsObj.SetActive(true);
            return;
        }

        if (!_isSpinning)
        {

            if (managerScript.wallet >= bManagerScript.betValue)
            {
                periodId = long.Parse(System.DateTime.Now.ToString("yyyyMMddHHmmss"));

                _isSpinning = true;
                BetManagerSlot b = FindObjectOfType<BetManagerSlot>();
                b._canPress = false;
                winAmnt = 0f;
                winAmntTxt.text = "0.00";
                if (winChkH != null)
                {
                    Debug.Log("loopH coroutine is enabled and set to false");

                    StopCoroutine(winChkH);
                }

                winAudio.Stop();
                StopAllAnimation();
                managerScript.DeductFromWallet(bManagerScript.betValue);
                dbRef.SaveGameHistoryDatabase(periodId, gameName, bManagerScript.betValue);

                StartCoroutine(SpinReels());
                spinStart.Play();

            }

        }

    }

    IEnumerator SpinReelsDummy()
    {

        int[][] numArray = GenerateRandomJaggedArray(5, 4, 2, 10);
        numArray = GenerateNonRepeatingRows(numArray, 3);

        reel1.SpinDummy(0f, numArray[0]);
        reel2.SpinDummy(0f, numArray[1]);
        reel3.SpinDummy(0f, numArray[2]);
        reel4.SpinDummy(0f, numArray[3]);
        reel5.SpinDummy(0f, numArray[4]);
        yield return null;
    }
    public IEnumerator SpinReels()
    {
        Debug.Log("poo slot : " + Wallet.slots_Pool);
        yield return new WaitForSeconds(0.2f);
        if (Wallet.slots_Pool < 100)
        {
            Debug.Log("Pool nahi hai ");
            int[][] numArray = GenerateRandomJaggedArray(5, 4, 2, 10);
            numArray = GenerateNonRepeatingRows(numArray, 3);

            reel1.Spin(0f, numArray[0]);
            reel2.Spin(0.1f, numArray[1]);
            reel3.Spin(0.2f, numArray[2]);
            reel4.Spin(0.3f, numArray[3]);
            reel5.Spin(0.4f, numArray[4]);

        }
        else if (Wallet.slots_Pool < 1000 && bManagerScript.betValue > 50)
        {
            Debug.Log("Pool kam hai ");
            int[][] numArray = GenerateRandomJaggedArray(5, 4, 2, 10);
            numArray = GenerateNonRepeatingRows(numArray, 4);

            reel1.Spin(0f, numArray[0]);
            reel2.Spin(0.1f, numArray[1]);
            reel3.Spin(0.2f, numArray[2]);
            reel4.Spin(0.3f, numArray[3]);
            reel5.Spin(0.4f, numArray[4]);

        }
        else
        {
            reel1.Spin(0f);
            reel2.Spin(0.1f);
            reel3.Spin(0.2f);
            reel4.Spin(0.3f);
            reel5.Spin(0.4f);
        }
        yield return null;
        spinSound.Play();
    }
    public void ActivateSpin()
    {

        if (!reel5._spinning)
        {
            _isSpinning = false;
            BetManagerSlot b = FindObjectOfType<BetManagerSlot>();
            if (freeSpin == 0)
            {
                b._canPress = true;
            }

            /* CalculateAmount();
             Debug.Log("Real Win Amount : " + winAmnt);
             Debug.Log("My Pool : " + Wallet.GetPool());
             if(winAmnt > Wallet.GetPool())
             {

                 Debug.Log("pool se jyada le ja rha hai");
                 reel2.RegenerateAll();
                 reel4.RegenerateAll();
             }
             CalculateAmount();
             if (winAmnt > Wallet.GetPool())
             {
                 Debug.Log("pool se jyada le ja rha hai");
                 reel2.RegenerateAll();
                 reel4.RegenerateAll();
             }*/


            spinSound.Stop();
            spinEnd.Play();
            WinChk();
        }


    }






















    // Generate a 2D array with random integers
    int[][] GenerateRandomJaggedArray(int rows, int cols, int min, int max)
    {
        int[][] array = new int[rows][];
        System.Random random = new System.Random();

        for (int i = 0; i < rows; i++)
        {
            array[i] = new int[cols];
            for (int j = 0; j < cols; j++)
            {
                array[i][j] = random.Next(min, max + 1);
            }
        }

        return array;
    }

    // Generate a new jagged array ensuring no common values in the first `checkRows` rows
    int[][] GenerateNonRepeatingRows(int[][] originalArray, int checkRows)
    {
        int rows = originalArray.Length;
        int cols = originalArray[0].Length;
        int[][] newArray = new int[rows][];

        for (int i = 0; i < rows; i++)
        {
            newArray[i] = (int[])originalArray[i].Clone();
        }

        System.Random random = new System.Random();

        for (int row = 0; row < checkRows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int k = 0;
                // Ensure current value is not present in previous rows
                while (HasConflict(newArray, row, col, checkRows))
                {
                    k++;
                    newArray[row][col] = random.Next(2, 10); // Replace with a new random number
                    if (k > 99)
                    {
                        break;
                    }
                }
            }
        }

        return newArray;
    }
    // Check if a value in a specific row and column is present in the previous rows
    bool HasConflict(int[][] array, int row, int col, int checkRows)
    {
        int value = array[row][col];
        for (int i = 0; i < row; i++)
        {
            if (RowContains(array[i], value))
            {
                return true;
            }
        }

        return false;
    }

    // Check if a specific row contains a value
    bool RowContains(int[] row, int value)
    {
        foreach (int num in row)
        {
            if (num == value)
            {
                return true;
            }
        }

        return false;
    }









}

public class Position
{
    public int Row { get; set; }
    public int Column { get; set; }

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public override string ToString()
    {
        return $"({Row}, {Column})";
    }
}
