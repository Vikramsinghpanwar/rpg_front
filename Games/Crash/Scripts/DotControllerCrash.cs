using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DotControllerCrash : MonoBehaviour
{

    public bool _Vertical;
    public GameObject dotPrefab;
    public GameObject startPoint;
    public GameObject endPoint;
    public Vector2 ePoint;

    public Transform scaleImg_V;
    public float speedMultiplierV;
    public float offsetV;
    public float gapV = 100f;

    public Transform scaleImg_H;
    public float speedMultiplierH;
    public float offsetH;
    public float gapH = 200f;

    public void ScalePosVUpdate(double multiplierV)
    {
        scaleImg_V.localPosition = new Vector2(0, ((float)multiplierV + offsetV) * gapV * speedMultiplierV);
    }
    
    public void ScalePosHUpdate(double multiplierH)
    {
        scaleImg_H.localPosition = new Vector2(((float)multiplierH + offsetH) * gapH / speedMultiplierH, 0);
    }
    // Start is called before the first frame update
    private void Start()
    {
        ePoint = endPoint.transform.localPosition;

        if (_Vertical)
        {
            PopulateV();
        }
        else
        {
            PopulateH();
        }
    }
    public void PopulateV()
    {


        float val = 0.1f;
        Vector2 l = new Vector2(0, 0);
        do
        {
            GameObject dot = Instantiate(dotPrefab, scaleImg_V);
            dot.transform.localPosition = ePoint + l;

            dot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = val.ToString("F1");
            val += 0.1f;
            l.y += gapV;

        }
        while (val < 300);

    }
    public void PopulateH()
    {
        float val = 0f;
        Vector2 l = new Vector2(0, 0);
        do
        {
            GameObject dot = Instantiate(dotPrefab, scaleImg_H);
            dot.transform.localPosition = ePoint + l;
            dot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = val.ToString();
            val += 2f;
            l.x += gapH;
        }
        while (val < 600);

    }


    
}
