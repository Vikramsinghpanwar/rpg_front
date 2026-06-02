using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Bootstrap;

public class DailyBonus : MonoBehaviour
{

    public Sprite claimedSpr;
    public Sprite lockedSpr;
    Image[] status = new Image[7];
    public GameObject[] obstacleImg;
    public Sprite GreenClaim;
    public Sprite YelloClaim;
    public Sprite DarkClaim;
    private int activeBtn;
    public GameObject claimedPopUp;
    void Start()
    {

        for (int i = 0; i < obstacleImg.Length; i++)
        {
            status[i] = obstacleImg[i].transform.GetChild(0).GetComponent<Image>();
        }
        claimedPopUp.SetActive(false);
        activeBtn = 100;
        int Day = UserDetail.Day;
        int Daily = UserDetail.Daily;
        Day--;
        for (int i = 0; i < 7; i++)
        {
            if (i < Day)
            {
                status[i].sprite = claimedSpr;
                obstacleImg[i].SetActive(true);
            }
            else if (i == Day)
            {
                if (Daily == 0)
                {
                    activeBtn = i;
                    obstacleImg[i].SetActive(false);
                }
                else if (Daily == 1)
                {
                    status[i].sprite = claimedSpr;
                    obstacleImg[i].SetActive(true);
                }
            }
            else if (i > Day)
            {
                obstacleImg[i].SetActive(true);
                status[i].sprite = lockedSpr;
            }
        }
    }



    //    public void BonusBtn(int ClickBtn){
    //         if(activeBtn == ClickBtn){
    //             Login appl = FindObjectOfType<Login>();
    //             appl.ApplyDaily();
    //         }else{
    //             Debug.Log(" not get bonus");
    //         }
    //    }

    public void GetDailly(string data)
    {
        Debug.Log(data.Length + "le");
        DailyGet[] dataArray = JsonHelper.FromJson<DailyGet>(data);
        if (dataArray[0].status == 1)
        {
            claimedPopUp.SetActive(true);
            claimedPopUp.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Claimed : " + dataArray[0].bonus;
            Invoke("DeactivatePopUP", 2f);
            UserDetail.Bonus = dataArray[0].bonus;
            UserData ff = FindObjectOfType<UserData>();
            ff.WallCha(BootstrapService.Instance.Wallet.deposit_balance / 100f, dataArray[0].bonus, BootstrapService.Instance.Wallet.win_balance / 100f, Wallet.GetPool());
            status[activeBtn].sprite = claimedSpr;
            obstacleImg[activeBtn].SetActive(true);
            activeBtn = 12;
            Debug.Log("ok, All Bonus Updted ");
        }
        else
        {
            Debug.Log("failed, array length is " + dataArray.Length);
        }
    }

    public void DeactivatePopUP()
    {
        claimedPopUp.SetActive(false);
    }



    [System.Serializable]
    public class DailyGet
    {
        public int status;
        public int day;
        public int daily;
        public float bonus;
    }


}
