using UnityEngine;
using System.Collections;

public class Popup : MonoBehaviour
{
    public float duration = 2f;

    void OnEnable()
    {
        StartCoroutine(AutoHide());
    }

    IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }
}
