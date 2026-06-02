using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AdjustImageWidth : MonoBehaviour
{
    public float padding = 10f; // Padding from both ends
    public float childSize = 50f; // Size of each child
    public RectTransform rtP;
    public GameObject tmpCardHolder;
    public bool _isResult = false;
    private void UpdateWidth()
    {
        rtP = transform.parent.GetComponent<RectTransform>();
        if (transform.childCount == 0 && tmpCardHolder.transform.childCount == 0)
        {
            //transform.localScale = Vector3.one;
            Destroy(transform.parent.gameObject);
        }
        // Get the image component
        Image image = GetComponent<Image>();

        // Calculate the total width based on child count and padding
        RectTransform rt = transform as RectTransform;
        float totalWidth = (transform.childCount * childSize) + (2 * padding) + 60;

        // Set the size of the image
        rtP.sizeDelta = new Vector2(totalWidth, rtP.sizeDelta.y);
    }

    private void OnTransformChildrenChanged()
    {
        UpdateWidth();

        if (transform.childCount > 0)
        {
            if (!_isResult)
            {
                gameObject.GetComponentInParent<CardsChecker>().CheckforCardsChange();
                CardPosUpdate();

            }

        }



    }


    public void CardPosUpdate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            DraggableItem d = transform.GetChild(i).gameObject.GetComponent<DraggableItem>();
            if (d._isSelected)
            {
                transform.GetChild(i).localPosition = new Vector3(0, d.selectedPosY, 0);
            }
        }
    }
    private void Start()
    {
        tmpCardHolder = GameObject.FindGameObjectWithTag("TmpCardHolder");
        UpdateWidth();
    }
}
