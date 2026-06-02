using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LuckyPlayer : MonoBehaviour
{

    public List<Target> targets_list;
    // Start is called before the first frame update
    Coroutine animCoroutine;
    int k;
    public void AnimStart()
    {
        animCoroutine = StartCoroutine(Anim());
        for(int i = 0; i< targets_list.Count; i++)
        {
            targets_list[i].animImage.SetActive(false);
            targets_list[i].starImg.SetActive(false);
            targets_list[i].starFillImg.fillAmount = 0f;
        }
       
    }


    public void AnimStop()
    {

        if(animCoroutine == null)
        {
            return;
        }
        StopCoroutine(animCoroutine);
    }

    public IEnumerator Anim()
    {
        yield return new WaitForSeconds(Random.Range(2, 5));

        do
        {
            yield return new WaitForSeconds(Random.Range(.5f, 3f));
            k = Random.Range(0, targets_list.Count);

            if (k == 1)
            {
                k = Random.Range(0, targets_list.Count);
            }

            targets_list[k].animImage.SetActive(true);
            targets_list[k].starImg.SetActive(true);
            targets_list[k].starFillImg.fillAmount += 0.2f;

            yield return new WaitForSeconds(1f);
            targets_list[k].animImage.SetActive(false);
        }
        while(true);
      
    }


}

[System.Serializable]
public class Target
{
    public GameObject animImage;
    public GameObject starImg;
    public Image starFillImg;
}
