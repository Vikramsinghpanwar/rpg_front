using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JackpotValueIncreaser : MonoBehaviour
{

    public float from = 100000f;
    public float to = 500000f;
    Text text;
    public double value;
    public float step = 0.01f;
    public float delay = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<Text>();
        value = Random.Range(from, to);
        StartCoroutine(Increaser());
    }

   public IEnumerator Increaser()
    {
        do
        {
            text.text = "₹" + value.ToString("F2");
            value += 0.01f;
            yield return new WaitForSeconds(delay);
        }
        while (true);
    }
}
