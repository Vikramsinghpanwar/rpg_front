using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenDeckManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GridLayoutGroup gridLayout;
    public bool _isExpanded;
    public AdjustWidthofOpenDeck adjustWidthOpenDeckRef;
    void Start()
    {
        _isExpanded = false;
        gridLayout = transform.GetChild(0).GetComponent<GridLayoutGroup>();
        adjustWidthOpenDeckRef = transform.GetChild(0).GetComponent<AdjustWidthofOpenDeck>();
        gridLayout.spacing = new Vector2(-70, 0);
    }

    public void ExpandOrCollapse()
    {
        Debug.Log("callin....");
        adjustWidthOpenDeckRef.UpdateWidth();
        if (_isExpanded)
        {
            gridLayout.spacing = new Vector2(-80, 0);
            adjustWidthOpenDeckRef.ClosedDeckWidth();

        }
        else
        {
            gridLayout.spacing = new Vector2(-40,0);
            adjustWidthOpenDeckRef.OpenDeckWidth();
        }
        _isExpanded = !_isExpanded;

    }
    // Update is called once per frame
    void Update()
    {
        
    }


  

}
