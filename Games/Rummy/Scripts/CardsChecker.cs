using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardsChecker : MonoBehaviour
{
    public bool _isFinish = false;
    // Start is called before the first frame update
    public Transform cardHolder;
    public List<int> cardValueList;
    public List<string> cardNameList;
    public TextMeshProUGUI cardOrderTxt;
    public int blockSum;
    public InitialCardDistributor initialCardDistributorRef;
    public InitialCardDistributor2Player initialCardDistributor2PlayerRef;
    public Image statusImg;
    public Color myGreen;
    public Color myRed;
    public Color myYellow;
    public bool _isContainImpureSequence;
    public bool _isContainImpureTrail;
    public bool _isPureSeq;
    public bool _isImpureSeq;
    public bool _isTwoSeq;
    private void Start()
    {
        _isPureSeq = false;
        _isTwoSeq = false;

        _isContainImpureSequence = false;
        _isContainImpureTrail = false;
        initialCardDistributorRef = FindObjectOfType<InitialCardDistributor>();
        initialCardDistributor2PlayerRef = FindObjectOfType<InitialCardDistributor2Player>();
        cardHolder = transform.GetChild(0);
        cardValueList = new List<int>();
        cardOrderTxt.text = "";
        GetCardValues();
        CheckforCardsChange();

    }



    public void GetCardValues()
    {
        cardHolder = transform.GetChild(0);

        cardValueList.Clear();
        cardNameList.Clear();
        for (int i = 0; i < cardHolder.childCount; i++)
        {
            cardValueList.Add(GetCardNumber(cardHolder.GetChild(i).GetComponent<Image>().sprite.name));
            cardNameList.Add(cardHolder.GetChild(i).GetComponent<Image>().sprite.name);
        }
    }
    static int GetCardValueForSum(string card)
    {
        string k = ConnectionRummy.jokerRank;
        string numberString = card.Substring(1);

        if (numberString == "J" || numberString == ConnectionRummy.jokerRank)
        {
            return 0;
        }
        else if (numberString == "a" || numberString == "k" || numberString == "q" || numberString == "j" || numberString == "0")
        {
            return 10;
        }

        else
        {
            return int.Parse(numberString);
        }
    }

    static int CalculateCardSum(List<string> cards)
    {
        int sum = 0;
        foreach (string cardstr in cards)
        {

            sum += GetCardValueForSum(cardstr);
        }

        return sum;
    }


    public void CheckforCardsChange()
    {

        GetCardValues();
        blockSum = CalculateCardSum(cardNameList);
        if (cardHolder.childCount < 3)
        {
            _isContainImpureSequence = false;
            _isContainImpureTrail = false;
            cardOrderTxt.text = "invalid(" + CalculateCardSum(cardNameList) + ")";
            statusImg.color = myRed;
        }

        else
        {           
            if (CheckPureSequence(cardNameList))
            {
                _isPureSeq = true;
                _isContainImpureSequence = false;
                cardOrderTxt.text = "Pure Sequence";
                blockSum = 0;
                statusImg.color = myGreen;
                AlignchildrenToCenter alignRef = gameObject.GetComponentInParent<AlignchildrenToCenter>();
                if(alignRef != null)
                {
                    alignRef.RefreshCardStatus();
                }
                else
                {
                    Debug.LogWarning("alignchildren nahi mila");
                }
                

            }
            else if (CheckSequence(cardValueList, cardNameList))
            {
                _isContainImpureSequence = true;

                if (CheckIfPureSequenceExist())
                {

                    _isImpureSeq = true;
                    blockSum = 0;
                    cardOrderTxt.text = "Sequence";
                    statusImg.color = myGreen;
                    gameObject.GetComponentInParent<AlignchildrenToCenter>().RefreshCardStatusSequence();

                }
                else
                {

                    cardOrderTxt.text = "Sequence(" + CalculateCardSum(cardNameList) + ")";
                    statusImg.color = myYellow;
                }

            }
            else if (CheckTrail(cardValueList, cardNameList))
            {
                _isContainImpureTrail = true;
                if (_isFinish == true)
                {
                    Transform topParent = transform.parent;
                    int cc = topParent.childCount;
                    int Spure_count = 0;
                    int S_count = 0;
                    for(int i = 0; i< cc; i++)
                    {
                        if(topParent.GetChild(i).GetComponent<CardsChecker>().cardOrderTxt.text == "Pure Sequence")
                        {
                            Spure_count++;
                        }
                        else if(topParent.GetChild(i).GetComponent<CardsChecker>().cardOrderTxt.text == "Sequence")
                        {
                            S_count++;
                        }
                        
                    }
                    if((Spure_count == 1 && S_count >= 1) ||(Spure_count > 1))
                    {
                        cardOrderTxt.text = "Trail";
                        blockSum = 0;
                        statusImg.color = myGreen;
                    }
                    else
                    {
                        cardOrderTxt.text = "Trail(" + CalculateCardSum(cardNameList) + ")";
                        statusImg.color = myYellow;
                    }
                
                }
                else
                {
                    if (CheckIfPureSequenceExist() && CheckIfTwoSequenceExist())
                    {
                        cardOrderTxt.text = "Trail";
                        blockSum = 0;
                        statusImg.color = myGreen;
                    }
                    else
                    {

                        cardOrderTxt.text = "Trail(" + CalculateCardSum(cardNameList) + ")";
                        statusImg.color = myYellow;
                    }
                }




            }
            else
            {
                _isContainImpureSequence = false;
                _isContainImpureTrail = false;
                cardOrderTxt.text = "invalid(" + CalculateCardSum(cardNameList) + ")";
                statusImg.color = myRed;
            }
        }

        if (_isPureSeq == true)
        {

            if (cardNameList.Count < 3 || !CheckPureSequence(cardNameList))
            {

                _isPureSeq = false;
                gameObject.GetComponentInParent<AlignchildrenToCenter>().UnRefreshCardStatus();
            }
            else
            {
            }
        }

        if (_isImpureSeq == true)
        {
            if (cardNameList.Count < 3 || !CheckSequence(cardValueList, cardNameList))
            {
                _isImpureSeq = false;
                gameObject.GetComponentInParent<AlignchildrenToCenter>().UnRefreshCardStatusTrail();

            }
        }

    }
    static bool CheckPureSequence(List<string> myCardsNameList)
    {
        //case when pure sequence contain the jokercard below closed deck
        List<int> myCardsValueList = new List<int>();
        for (int i = 0; i < myCardsNameList.Count; i++)
        {
            myCardsValueList.Add(GetCardNumberWithoutJoker(myCardsNameList[i]));
        }
        //check for joker main card only
        for (int i = 0; i < myCardsValueList.Count; i++)
        {
            if (myCardsValueList[i] == 0)
            {
                return false;
            }
        }

        for (int i = 0; i < myCardsNameList.Count - 2; i++)
        {
            //duplicate cards ate h invalid
            if (myCardsNameList[i] == myCardsNameList[i + 1])
            {
                return false;
            }
            if (myCardsNameList[i].Substring(0, 1) != myCardsNameList[i + 1].Substring(0, 1))
            {
                //chk if first card is joker
                if (GetCardNumberWithoutJoker(myCardsNameList[i]) == 0)
                {
                    if (myCardsNameList[i + 1].Substring(0, 1) != myCardsNameList[i + 2].Substring(0, 1))
                    {
                        if (GetCardNumberWithoutJoker(myCardsNameList[i + 1]) != 0)
                        {
                            return false;
                        }
                    }
                }
                //chkk if second card is joker
                else if (GetCardNumberWithoutJoker(myCardsNameList[i + 1]) == 0)
                {
                    if (myCardsNameList[i].Substring(0, 1) != myCardsNameList[i + 2].Substring(0, 1))
                    {
                        if (GetCardNumberWithoutJoker(myCardsNameList[i + 2]) != 0)
                        {
                            return false;
                        }
                    }
                }
                else return false;
            }
            else if (myCardsNameList[i].Substring(0, 1) != myCardsNameList[i + 2].Substring(0, 1))
            {
                if (GetCardNumberWithoutJoker(myCardsNameList[i + 2]) != 0)
                {
                    return false;
                }
            }
        }
        if (!CheckSequence(myCardsValueList, myCardsNameList))
        {
            return false;
        }
        return true;
    }

    public bool CheckIfTwoSequenceExist()
    {
        int seqCount = 0;
        Transform tform;

        tform = transform.parent;
        for (int i = 0; i < tform.childCount; i++)
        {
            string k = tform.GetChild(i).transform.GetChild(1).transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text;
            if (k == "Pure Sequence" || k == "Sequence")
            {
                seqCount++;
            }
        }
        if (seqCount >= 2)
        {
            return true;
        }
        else return false;

    }

    public bool CheckIfPureSequenceExist()
    {
        if (BootValueDecider.rummyPlayerCount == 2)
        {
            if (_isFinish)
            {
                Transform tform;

                tform = transform.parent;
                for (int i = 0; i < tform.childCount; i++)
                {
                    Transform k = tform.GetChild(i).transform;
                    if (k.GetChild(1).transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text == "Pure Sequence")
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                Transform tform;
                if (initialCardDistributor2PlayerRef != null)
                {
                    tform = initialCardDistributor2PlayerRef.mainBlock.transform;
                }
                else 
                { 
                    return false; 
                }

                for (int i = 0; i < tform.childCount; i++)
                {
                    Transform k = tform.GetChild(i).transform;
                    if (k.GetChild(1).transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text == "Pure Sequence")
                    {
                        return true;
                    }
                }
                return false;
            }

        }
        else
        {
            if (_isFinish)
            {
                Transform tform;

                tform = transform.parent;
                for (int i = 0; i < tform.childCount; i++)
                {
                    Transform k = tform.GetChild(i).transform;
                    if (k.GetChild(1).transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text == "Pure Sequence")
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                Transform tform = null;
                if (initialCardDistributorRef.isActiveAndEnabled)
                {
                    tform = initialCardDistributorRef.mainBlock.transform;
                }
                for (int i = 0; i < tform.childCount; i++)
                {
                    Transform k = tform.GetChild(i).transform;
                    if (k.GetChild(1).transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text == "Pure Sequence")
                    {
                        return true;
                    }
                }
                return false;
            }

        }
    }
    static bool CheckSequence(List<int> myCardsValueList, List<string> myCardsNameList)
    {

        //Duplicate cards ka tabeej
        for (int i = 0; i < myCardsNameList.Count; i++)
        {

            for(int s = i+1; s<myCardsNameList.Count; s++)
            {
                if (myCardsNameList[i] == myCardsNameList[s])
                {
                     if (myCardsNameList[i].Substring(1) != "J")
                    {
                        return false;
                    }
                }
            }            
        }
        

        //alag symbol cards ka tabeez
        string standardCard = "";

        //joker k alwa koi bhi card ate hi standard ban jaega
        for (int i = 0; i < myCardsNameList.Count; i++)
        {
            if (GetCardNumber(myCardsNameList[i]) != 0)
            {
                standardCard = myCardsNameList[i];
                break;
            }
        }


        for (int i = 0; i < myCardsNameList.Count; i++)
        {
            if (standardCard.Substring(0, 1) != myCardsNameList[i].Substring(0, 1))
            {
                if (GetCardNumber(myCardsNameList[i]) != 0)
                {
                    Debug.Log("different symbol wale card hone k karan sequence hi nahi hai");
                    return false;
                }
            }
        }

        //alag symbol ka tabeej 2 -----------------------------------------------------------------isko band krke check krna hai
        for (int i = 0; i < myCardsNameList.Count - 2; i++)
        {
            if (myCardsNameList[i].Substring(0, 1) != myCardsNameList[i + 1].Substring(0, 1) && GetCardNumber(myCardsNameList[i]) != 0 && GetCardNumber(myCardsNameList[i+1]) != 0)
            {
                //chk if first card is joker
                if (GetCardNumber(myCardsNameList[i]) == 0)
                {
                    if (myCardsNameList[i + 1].Substring(0, 1) != myCardsNameList[i + 2].Substring(0, 1))
                    {
                        if (GetCardNumber(myCardsNameList[i + 1]) != 0)
                        {
                            return false;
                        }
                    }
                }
                //chkk if second card is joker
                else if (GetCardNumber(myCardsNameList[i + 1]) == 0)
                {
                    if (myCardsNameList[i].Substring(0, 1) != myCardsNameList[i + 2].Substring(0, 1))
                    {
                        if (GetCardNumber(myCardsNameList[i + 2]) != 0)
                        {
                            return false;
                        }
                    }
                }
                else return false;


            }
            else if (myCardsNameList[i].Substring(0, 1) != myCardsNameList[i + 2].Substring(0, 1) && GetCardNumber(myCardsNameList[i]) != 0 && GetCardNumber(myCardsNameList[i + 2]) != 0)
            {
                if (GetCardNumber(myCardsNameList[i + 2]) != 0)
                {
                    return false;
                }
            }
        }

        myCardsValueList.Sort();
        int jokerCounts = 0;

        //check number of jokers
        for (int i = 0; i < myCardsValueList.Count; i++)
        {
            if (myCardsValueList[i] == 0)
            {
                jokerCounts++;
            }
        }
        for (int i = jokerCounts; i < myCardsValueList.Count - 1; i++)
        {
            int k = myCardsValueList[i + 1] - myCardsValueList[i];
            if (k > jokerCounts + 1 )
            {

                /// A ki value 1 krke chek kro ab
                /// 
                jokerCounts = 0;
                for (int jc = 0; jc < myCardsValueList.Count; jc++)
                {
                    if (myCardsValueList[jc] == 0)
                    {
                        jokerCounts++;
                    }
                }

                for (int b = 0; b<myCardsValueList.Count; b++)
                {
                    if(myCardsValueList[b] == 14)
                    {
                        myCardsValueList[b] = 1;
                    }
                }
                myCardsValueList.Sort();

                for (int s = jokerCounts; s < myCardsValueList.Count - 1; s++)
                {

                    int sk = myCardsValueList[s + 1] - myCardsValueList[s];
                    if (sk > jokerCounts + 1 && !IsSpecialSequence(myCardsValueList[s], myCardsValueList[s + 1]))
                    {
                        return false;
                    }
                    if (sk > 1)
                    {
                        jokerCounts -= (sk - 1);
                    }
                }                
            }
            if (k > 1)
            {
                jokerCounts -= (k - 1);
            }
        }

        return true;
    }

    static bool IsSpecialSequence(int previousNumber, int currentNumber)
    {
        return (previousNumber == 2 && currentNumber == 14) ||   // A, 2
         (previousNumber == 3 && currentNumber == 14) ||   // A, 2
         (previousNumber == 4 && currentNumber == 14) ||   // A, 2
         (previousNumber == 5 && currentNumber == 14) ||   // A, 2
         (previousNumber == 6 && currentNumber == 14);  // A, 3

    }

    static bool CheckTrail(List<int> myCardsValueList, List<string> myCardsNameList)
    {
        List<string> trailCardsSuitList = new List<string>();

        //check for cards having same suit 
        for (int i = 0; i < myCardsNameList.Count; i++)
        {

            if (GetCardNumber(myCardsNameList[i]) != 0)
            {
                trailCardsSuitList.Add(myCardsNameList[i].Substring(0, 1));
            }
        }

        for (int i = 0; i < trailCardsSuitList.Count - 1; i++)
        {
            for (int j = i + 1; j < trailCardsSuitList.Count; j++)
            {
                if (trailCardsSuitList[i] == trailCardsSuitList[j])
                {
                    return false;
                }
            }
        }

        myCardsValueList.Sort();

        int jokerCounts = 0;

        //check number of jokers
        for (int i = 0; i < myCardsValueList.Count; i++)
        {
            if (myCardsValueList[i] == 0)
            {
                jokerCounts++;
            }
        }


        for (int i = jokerCounts; i < myCardsValueList.Count - 1; i++)
        {
            int currentCardNumber = myCardsValueList[i];
            int nextCardNumber = myCardsValueList[i + 1];
            if (currentCardNumber != nextCardNumber)
            {
                return false;
            }

        }

        return true;
    }

    static int GetCardNumber(string cardName)
    {
        string numberString = cardName.Substring(1);
        if (numberString == "J" || numberString == ConnectionRummy.jokerRank)
        {
            return 0;
        }
        else if (numberString == "a")
        {
            return 14;
        }
        else if (numberString == "0")
        {
            return 10;
        }
        else if (numberString == "j")
        {
            return 11;
        }
        else if (numberString == "q")
        {
            return 12;
        }
        else if (numberString == "k")
        {
            return 13;
        }

        else
        {
            return int.Parse(numberString);
        }
    }

    //for pure sequence we need this
    static int GetCardNumberWithoutJoker(string cardName)
    {
        string numberString = cardName.Substring(1);
        if (numberString == "J")
        {
            return 0;
        }
        else if (numberString == "a")
        {
            return 14;
        }
        else if (numberString == "0")
        {
            return 10;
        }
        else if (numberString == "j")
        {
            return 11;
        }
        else if (numberString == "q")
        {
            return 12;
        }
        else if (numberString == "k")
        {
            return 13;
        }

        else
        {
            return int.Parse(numberString);
        }
    }


}
