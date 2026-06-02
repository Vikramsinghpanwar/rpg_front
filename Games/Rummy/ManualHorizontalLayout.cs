using UnityEngine;

public class ManualHorizontalLayout : MonoBehaviour
{
    public float childWidth = 100f;  // Width of each child
    public float childHeight = 100f; // Height of each child
    public float paddingX = 10f;     // Padding between children in the X-axis
    float selectedPosY;
    public int childCount;
    void Start()
    {
        ArrangeChildren();
        childCount = transform.childCount;
    }
    private void OnTransformChildrenChanged()
    {
        if (transform.childCount > childCount)
        {
            Invoke("ArrangeChildren", 0.05f);
        }
        ArrangeChildren();
        childCount = transform.childCount;

    }
    public void ArrangeChildren()
    {
        int childCount = transform.childCount;

        // Calculate total width required for the children including padding
        float totalWidth = (childWidth + paddingX) * childCount - paddingX;

        // Calculate the starting position to center the children
        float startX = -totalWidth / 2 + childWidth / 2;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            RectTransform rectTransform = child.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                // Set the size of the child
                rectTransform.sizeDelta = new Vector2(childWidth, childHeight);

                // Calculate the new position
                float posX = startX + i * (childWidth + paddingX);
                float posY;
                DraggableItem d = child.gameObject.GetComponent<DraggableItem>();
                if (d._isSelected == true)
                {
                    posY = d.displacementY;
                }
                else
                {
                    posY = 0f;
                }
                rectTransform.anchoredPosition = new Vector2(posX, posY);
            }
        }
    }
}
