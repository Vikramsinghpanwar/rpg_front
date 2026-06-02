using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Features.Lobby.Integration;

public class BetManagerMines : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        walletAmt = BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f;
        BetAmount = 50;
        totalBetAmt = 0;
        DisSelectAllCoins();
        Glow[0].SetActive(true);
        walletTxt.text = "₹" + walletAmt.ToString();
        managerRef = FindObjectOfType<ManagerMines>();
    }

    public void DisSelectAllCoins()
    {
        foreach (GameObject g in Glow)
        {
            g.SetActive(false);
        }
    }
    public ManagerMines managerRef;
    public GameObject parentObject;
    public Vector3 pos;
    public List<GameObject> coinsBettedList;
    public GameObject prefab2000, prefab50, prefab100, prefab500, prefab1000, prefab5000;
    public float walletAmt;
    public float displacementY;
    public List<GameObject> Glow;
    public int BetAmount;
    public int totalBetAmt;
    public Text walletTxt;
    public TextMeshProUGUI totalBetAmtText;

    public void UpdateWallet(float val)
    {
        walletAmt += val;
        walletTxt.text = "₹ " + walletAmt.ToString("F2");
        Wallet.AddToWinWallet(val);
    }

    public void DownAll()
    {
        for (int i = 0; i < Glow.Count; i++)
        {
            Glow[i].transform.parent.transform.localPosition = new Vector2(Glow[i].transform.parent.transform.localPosition.x, 0);
        }
    }
    public void BetCoin(int val)
    {
        DisSelectAllCoins();
        DownAll();
        switch (val)
        {
            case 50:
                BetAmount = 50;
                Glow[0].SetActive(true);
                Glow[0].transform.parent.transform.localPosition = new Vector2(Glow[0].transform.parent.transform.localPosition.x, displacementY);

                break;
            case 100:
                BetAmount = 100;
                Glow[1].SetActive(true);
                Glow[1].transform.parent.transform.localPosition = new Vector2(Glow[1].transform.parent.transform.localPosition.x, displacementY);

                break;
            case 500:
                BetAmount = 500;
                Glow[2].SetActive(true);
                Glow[2].transform.parent.transform.localPosition = new Vector2(Glow[2].transform.parent.transform.localPosition.x, displacementY);

                break;
            case 1000:
                BetAmount = 1000;
                Glow[3].SetActive(true);
                Glow[3].transform.parent.transform.localPosition = new Vector2(Glow[3].transform.parent.transform.localPosition.x, displacementY);

                break;
            case 2000:
                BetAmount = 2000;
                Glow[4].SetActive(true);
                Glow[4].transform.parent.transform.localPosition = new Vector2(Glow[4].transform.parent.transform.localPosition.x, displacementY);

                break;
            case 5000:
                BetAmount = 5000;
                Glow[5].SetActive(true);
                Glow[5].transform.parent.transform.localPosition = new Vector2(Glow[5].transform.parent.transform.localPosition.x, displacementY);

                break;

        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void Clear()
    {
        totalBetAmt = 0;
        foreach (GameObject g in coinsBettedList)
        {
            Destroy(g);
        }
        coinsBettedList.Clear();
        totalBetAmtText.text = totalBetAmt.ToString();
    }

    public void PlaceBet()
    {
        if ((walletAmt - totalBetAmt) >= BetAmount)
        {
            managerRef.SpinBtn.interactable = true;

            Vector3 mPos = Input.mousePosition;

            totalBetAmt += BetAmount;
            GameObject g = null;
            switch (BetAmount)
            {
                case 2000:
                    g = Instantiate(prefab2000);

                    break;
                case 50:
                    g = Instantiate(prefab50);

                    break;
                case 100:
                    g = Instantiate(prefab100);

                    break;
                case 500:
                    g = Instantiate(prefab500);

                    break;
                case 1000:
                    g = Instantiate(prefab1000);

                    break;
                case 5000:
                    g = Instantiate(prefab5000);

                    break;

            }
            g.transform.parent = parentObject.transform;
            g.transform.localScale = Vector3.one;
            g.transform.position = mPos;// new Vector3(pos.x + Random.Range(-50, 50), pos.y + Random.Range(-10, 10));
            coinsBettedList.Add(g);
            walletTxt.text = "₹ " + walletAmt.ToString("F2");
            totalBetAmtText.text = totalBetAmt.ToString();
            managerRef.SetMultipliersOnPlaceBet();

        }
    }


}
