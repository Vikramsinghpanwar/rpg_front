using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RandomBoli : MonoBehaviour
{
    public TextMeshProUGUI t1, t2, t3, t4, t5, t6, t7, a1t, a2t, a3t, a4t, a5t, a6t, a7t;
    public Animator a1, a2, a3, a4, a5, a6, a7;
    public int i1, i2, i3, i4, i5, i6, i7;
    public bool _isBet;



    public void BetStarter()
    {

        i1 = 0;
        i2 = 0;
        i3 = 0;
        i4 = 0;
        i5 = 0;
        i7 = 0;
    
    }

    public void BetUpdate(int i)
    {
        switch (i)
        {
            case 1:
                t1.text = i1.ToString();

                break;
            case 2:
                t2.text = i2.ToString();

                break;
            case 3:
                t3.text = i3.ToString();

                break;
            case 4:
                t4.text = i4.ToString();

                break;
            case 5:
                t5.text = i5.ToString();

                break;
            case 6:
                t6.text = i6.ToString();

                break;
            case 7:
                t7.text = i7.ToString();

                break;
          
        }
    }
    public IEnumerator RandomBet1()
    {
        do
        {
            int p = Random.Range(0, 7);
            int k;
            switch (p)
            {
                case 0:
                    k = Random.Range(1, 21);
                    a1t.text = "+" + (k * 100).ToString();
                    a1.SetTrigger("animTrigger");
                    i1 += k * 100;
                    t1.text = i1.ToString();
                    break;
                case 1:
                    k = Random.Range(1, 21);
                    a2t.text = "+" +  (k * 100).ToString();
                    a2.SetTrigger("animTrigger");

                    i2 += k * 100;
                    t2.text = i2.ToString();
                    break;
                case 2:
                    k = Random.Range(1, 21);
                    a3t.text = "+" + (k * 100).ToString();
                    a3.SetTrigger("animTrigger");
                    i3 += k * 100;
                    t3.text = i3.ToString();
                    break;
                case 3:
                    k = Random.Range(1, 21);
                    a4t.text = "+" + (k * 100).ToString();
                    a4.SetTrigger("animTrigger");
                    i4 += k * 100;
                    t4.text = i4.ToString();
                    break;
                case 4:
                    k = Random.Range(1, 21);
                    a5t.text = "+" + (k * 100).ToString();
                    a5.SetTrigger("animTrigger");
                    i5 += k * 100;
                    t5.text = i5.ToString();
                    break;
                case 5:
                    k = Random.Range(1, 21);
                    a6t.text = "+" + (k * 100).ToString();
                    a6.SetTrigger("animTrigger");
                    i6 += k * 100;
                    t6.text = i6.ToString();
                    break;
                case 6:
                    k = Random.Range(1, 21);
                    a7t.text = "+" + (k * 100).ToString();
                    a7.SetTrigger("animTrigger");
                    i7 += k * 100;
                    t7.text = i7.ToString();

                    break;
               
            }
            for(int i = 0; i<7; i++)
            {

            }
          
            yield return new WaitForSeconds(0.8f);
            

        }
        while (_isBet);
    }
    public IEnumerator RandomBet2()
    {
        do
        {
            int p = Random.Range(0, 7);
            int k;
            switch (p)
            {
                case 0:
                    k = Random.Range(1, 21);
                    a1t.text = "+" + (k * 100).ToString();
                    a1.SetTrigger("animTrigger");
                    i1 += k * 100;
                    t1.text = i1.ToString();
                    break;
                case 1:
                    k = Random.Range(1, 21);
                    a2t.text = "+" +  (k * 100).ToString();
                    a2.SetTrigger("animTrigger");

                    i2 += k * 100;
                    t2.text = i2.ToString();
                    break;
                case 2:
                    k = Random.Range(1, 21);
                    a3t.text = "+" + (k * 100).ToString();
                    a3.SetTrigger("animTrigger");
                    i3 += k * 100;
                    t3.text = i3.ToString();
                    break;
                case 3:
                    k = Random.Range(1, 21);
                    a4t.text = "+" + (k * 100).ToString();
                    a4.SetTrigger("animTrigger");
                    i4 += k * 100;
                    t4.text = i4.ToString();
                    break;
                case 4:
                    k = Random.Range(1, 21);
                    a5t.text = "+" + (k * 100).ToString();
                    a5.SetTrigger("animTrigger");
                    i5 += k * 100;
                    t5.text = i5.ToString();
                    break;
                case 5:
                    k = Random.Range(1, 21);
                    a6t.text = "+" + (k * 100).ToString();
                    a6.SetTrigger("animTrigger");
                    i6 += k * 100;
                    t6.text = i6.ToString();
                    break;
                case 6:
                    k = Random.Range(1, 21);
                    a7t.text = "+" + (k * 100).ToString();
                    a7.SetTrigger("animTrigger");
                    i7 += k * 100;
                    t7.text = i7.ToString();

                    break;
               
            }
            for(int i = 0; i<7; i++)
            {

            }
          
            yield return new WaitForSeconds(1.2f);
            

        }
        while (_isBet);
    }
    public IEnumerator RandomBet3()
    {
        do
        {
            int p = Random.Range(0, 7);
            int k;
            switch (p)
            {
                case 0:
                    k = Random.Range(1, 21);
                    a1t.text = "+" + (k * 100).ToString();
                    a1.SetTrigger("animTrigger");
                    i1 += k * 100;
                    t1.text = i1.ToString();
                    break;
                case 1:
                    k = Random.Range(1, 21);
                    a2t.text = "+" +  (k * 100).ToString();
                    a2.SetTrigger("animTrigger");

                    i2 += k * 100;
                    t2.text = i2.ToString();
                    break;
                case 2:
                    k = Random.Range(1, 21);
                    a3t.text = "+" + (k * 100).ToString();
                    a3.SetTrigger("animTrigger");
                    i3 += k * 100;
                    t3.text = i3.ToString();
                    break;
                case 3:
                    k = Random.Range(1, 21);
                    a4t.text = "+" + (k * 100).ToString();
                    a4.SetTrigger("animTrigger");
                    i4 += k * 100;
                    t4.text = i4.ToString();
                    break;
                case 4:
                    k = Random.Range(1, 21);
                    a5t.text = "+" + (k * 100).ToString();
                    a5.SetTrigger("animTrigger");
                    i5 += k * 100;
                    t5.text = i5.ToString();
                    break;
                case 5:
                    k = Random.Range(1, 21);
                    a6t.text = "+" + (k * 100).ToString();
                    a6.SetTrigger("animTrigger");
                    i6 += k * 100;
                    t6.text = i6.ToString();
                    break;
                case 6:
                    k = Random.Range(1, 21);
                    a7t.text = "+" + (k * 100).ToString();
                    a7.SetTrigger("animTrigger");
                    i7 += k * 100;
                    t7.text = i7.ToString();

                    break;
               
            }
            for(int i = 0; i<7; i++)
            {

            }
          
            yield return new WaitForSeconds(1f);
            

        }
        while (_isBet);
    }

}
