using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class LotteryBallAnimation : MonoBehaviour
{
    public GameObject blowerRef;
    public GameObject blowerRef1;
    public GameObject blowerRef_up;

    public IEnumerator RandomBallMovement()
    {
        blowerRef.SetActive(true);
        blowerRef_up.SetActive(true);
        blowerRef1.SetActive(true);
        yield return new WaitForSeconds(5f);
        blowerRef.SetActive(false);
        blowerRef_up.SetActive(false);
        blowerRef1.SetActive(false);

    }





}
