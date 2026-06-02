using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RnadomBetsRoulleteB : MonoBehaviour
{
    public TextMeshProUGUI totalBetsTxt;
    int totalBets;
    ManagerBRoullete managerRef;
    // Start is called before the first frame update
    public List<TextMeshProUGUI> TextList;
    public List<int> betValList;
    public List<int> mybetValList;
    public bool _Stop;
    private void Start()
    {
        managerRef = FindObjectOfType<ManagerBRoullete>();

       
    }
    public void IncreaseBet()
    {
        totalBets = 0;

        totalBetsTxt.text = "Total : " + "0";

        betValList.Clear();
        mybetValList.Clear();

        mybetValList.Add(managerRef.bet1BetVal);
        mybetValList.Add(managerRef.bet2BetVal);
        mybetValList.Add(managerRef.bet3BetVal);
        mybetValList.Add(managerRef.bet4BetVal);
        mybetValList.Add(managerRef.bet5BetVal);
        mybetValList.Add(managerRef.bet6BetVal);
        mybetValList.Add(managerRef.bet7BetVal);
        mybetValList.Add(managerRef.bet8BetVal);
        mybetValList.Add(managerRef.bet9BetVal);
        mybetValList.Add(managerRef.bet10BetVal);
        mybetValList.Add(managerRef.bet11BetVal);


        for (int i = 0; i < TextList.Count; i++)
        {
            int k = Random.Range(100, 999);
            UpdateTotalBets(k);
            betValList.Add(k);
            TextList[i].text = "<color=#FFDB4F>" + mybetValList[i] + "</color>/<color=#B4EEFF>" + betValList[i].ToString() + "0" + "</color>";
        }
        BetUpdate();
    }

    public void BetUpdate()
    {
        if (!_Stop)
        {
            for (int i = 0; i < TextList.Count; i++)
            {
                int k = Random.Range(100, 500);
                UpdateTotalBets(k);
                betValList[i] += k;
                TextList[i].text = "<color=#FFDB4F>" + mybetValList[i] + "</color>/<color=#B4EEFF>" + betValList[i].ToString() + "0" + "</color>";
            }
            Invoke("BetUpdate", Random.Range(0.5f, 1f));
        }
    }

    public void MyBetUpdate(int val)
    {
        UpdateTotalBets(val);
        Debug.Log("updating for : " + val);
        switch (val)
        {
            case 1:
                mybetValList[0] = managerRef.bet1BetVal;
                break;
            case 2:
                mybetValList[1] = managerRef.bet2BetVal;
                break;
            case 3:
                mybetValList[2] = managerRef.bet3BetVal;
                break;
            case 4:
                mybetValList[3] = managerRef.bet4BetVal;
                break;
            case 5:
                mybetValList[4] = managerRef.bet5BetVal;
                break;
            case 6:
                mybetValList[5] = managerRef.bet6BetVal;
                break;
            case 7:
                mybetValList[6] = managerRef.bet7BetVal;
                break;
            case 8:
                mybetValList[7] = managerRef.bet8BetVal;
                break;
            case 9:
                mybetValList[8] = managerRef.bet9BetVal;
                break;
            case 10:
                mybetValList[9] = managerRef.bet10BetVal;
                break;
            case 11:
                mybetValList[10] = managerRef.bet11BetVal;
                break;
        }

        //BetUpdate();
    }
  
    public void UpdateTotalBets(int val)
    {
        totalBets += val;
        totalBetsTxt.text = "Total : " + totalBets + "0";
    }
}
