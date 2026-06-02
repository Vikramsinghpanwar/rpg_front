using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangePlayerNum : MonoBehaviour
{
    // Start is called before the first frame update
    public int playerNum;
    public TextMeshProUGUI text;
    void Start()
    {
        playerNum = Random.Range(999, 4999);
        StartCoroutine(Me());
    }

   public IEnumerator Me()
    {
        do
        {
            yield return new WaitForSeconds(Random.Range(3, 5));
            playerNum = playerNum + Random.Range(-10, 50);
            text.text = playerNum.ToString();
        }
        while (true);
      

    }
}
