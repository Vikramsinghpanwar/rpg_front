using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdjustWidthofOpenDeck : MonoBehaviour
{
    public float padding = 10f; // Padding from both ends
    public float childSize = 50f; // Size of each child
    public RectTransform rtP;
    public GameObject tmpCardHolder;
    public float parentWidth;
    public OpenDeckManager openDeckManagerRef;
    public void UpdateWidth()
    {
        rtP = transform.parent.GetComponent<RectTransform>();
        if (transform.childCount == 0 && tmpCardHolder.transform.childCount == 0)
        {
            //transform.localScale = Vector3.one;
            //Destroy(transform.parent.gameObject);
        }
        // Get the image component
        Image image = GetComponent<Image>();

        // Calculate the total width based on child count and padding
        RectTransform rt = transform as RectTransform;
        float totalWidth = (transform.childCount * childSize) + (2 * padding) + 20;

        // Set the size of the image
        parentWidth = totalWidth + 0;
        if (openDeckManagerRef._isExpanded)
        {
            rtP.sizeDelta = new Vector2(parentWidth, rtP.sizeDelta.y);

        }
    }
    public void OpenDeckWidth()
    {
        rtP.sizeDelta = new Vector2(parentWidth, rtP.sizeDelta.y);

    }

    public void ClosedDeckWidth()
    {
        rtP.sizeDelta = new Vector2(70f, rtP.sizeDelta.y);

    }

    private void OnTransformChildrenChanged()
    {
        UpdateWidth();

       
    }


    private void Start()
    {
        openDeckManagerRef = FindObjectOfType<OpenDeckManager>();
        tmpCardHolder = GameObject.FindGameObjectWithTag("TmpCardHolder");
        UpdateWidth();
    }
}
