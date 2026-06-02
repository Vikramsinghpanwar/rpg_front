using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.ChickenRoad2
{
    public class LiveWin : MonoBehaviour
    {
        public GameObject winObject;
        public TextMeshProUGUI playerNameTxt;
        public TextMeshProUGUI amountTxt;
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(OfflineWin());
        }

        IEnumerator OfflineWin()
        {
            while (true)
            {
                string name = "user_" + Random.Range(10, 10000);
                PlayerWin(name, Random.Range(1000, 10000).ToString());
                yield return new WaitForSeconds(Random.Range(0.5f, 5f));
            }
        }
        public void PlayerWin(string name, string amount)
        {
            if (winObject.activeInHierarchy) winObject.SetActive(false);
            playerNameTxt.text = name;
            amountTxt.text = "+₹" + amount;
            winObject.SetActive(true);
        }
    }
}

