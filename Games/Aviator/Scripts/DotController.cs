using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotController : MonoBehaviour
{
    float diffY;
    float diffX;
    // Start is called before the first frame update
    private void Start()
    {
        sPoint = startPoint.transform.localPosition;
        ePoint = endPoint.transform.localPosition;
        diffY = sPoint.y - ePoint.y;
        diffX = sPoint.x - ePoint.x;
    }
    public void Populate()
    {
        
        
        if (_Vertical)
        {
            gap = (sPoint.y - ePoint.y) / dotsPer;
            Vector2 l = new Vector2(0, 0);
            do
            {
                l.y += gap;
                GameObject dot = Instantiate(dotPrefab, gameObject.transform);
                dot.transform.localPosition = ePoint + l;
                DotsList.Add(dot);

            }
            while ((l.y ) < (diffY));
        }
        else
        {
            gap = (sPoint.x - ePoint.x) / dotsPer;
            Vector2 l = new Vector2(0, 0);
            do
            {
                l.x += gap;
                GameObject dot = Instantiate(dotPrefab, gameObject.transform);
                dot.transform.localPosition = ePoint + l;
                DotsList.Add(dot);

            }
            while ((l.x ) < (diffX));
        }
       
    }


    public  void DeleteAll()
    {
        for(int k = 0; k< DotsList.Count; k++)
        {
            Destroy(DotsList[k]);
        }
        DotsList.Clear();
       

    }
    public bool _Vertical;
    float gap;
    public float dotsPer;
    public GameObject dotPrefab;
    public GameObject startPoint;
    public GameObject endPoint;
    public List<GameObject> DotsList;
    public Vector2 sPoint;
    public Vector2 ePoint;

    public void MoveDots()
    {
      
            foreach (GameObject g in DotsList)
            {
                g.GetComponent<MoveContinously>()._is = true;
            }
     
        

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
