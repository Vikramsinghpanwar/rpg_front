using UnityEngine;
using TMPro;

public class AlignchildrenToCenter : MonoBehaviour
{
    public void RefreshCardStatus()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CardsChecker x = transform.GetChild(i).gameObject.GetComponent<CardsChecker>();
            if (x._isContainImpureSequence || x._isContainImpureTrail)
            {
                x.CheckforCardsChange();
            }
        }
    }

    public void RefreshCardStatusSequence()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CardsChecker x = transform.GetChild(i).gameObject.GetComponent<CardsChecker>();
            if (x._isContainImpureTrail)
            {
                x.CheckforCardsChange();
            }
        }
    }

    public void UnRefreshCardStatus()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CardsChecker x = transform.GetChild(i).gameObject.GetComponent<CardsChecker>();

            Transform ts = transform.GetChild(i).transform;
            string s = ts.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
            if (s == "Sequence" || s == "Trail")
            {
                x.CheckforCardsChange();
            }
        }
    }

    public void UnRefreshCardStatusTrail()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            CardsChecker x = transform.GetChild(i).gameObject.GetComponent<CardsChecker>();

            Transform ts = transform.GetChild(i).transform;
            string s = ts.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
            if (s == "Trail")
            {
                x.CheckforCardsChange();
            }
        }
    }

    public float gap = 10f; // Gap between children

    void Start()
    {
        AlignChildren();
    }

    private void OnTransformChildrenChanged()
    {
        AlignChildren();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AlignChildren();
        }
    }
    public void AlignChildren()
    {
        RectTransform rt = GetComponent<RectTransform>();

        if (rt == null)
        {
            Debug.LogError("No RectTransform found on the GameObject.");
            return;
        }

        float totalWidth = 0; // Start with padding on both sides
        int childCount = rt.childCount;

        // Calculate the total width of all children plus gaps
        for (int i = 0; i < childCount; i++)
        {
            RectTransform childRt = rt.GetChild(i).GetComponent<RectTransform>();

            if (childRt != null)
            {
                totalWidth += childRt.rect.width;
                if (i > 0) // Add gap for all children except the first one
                {
                    totalWidth += gap;
                }
            }
        }

        // Calculate the starting position for the first child
        float startX = -totalWidth / 2f; // Start with padding on the left

        // Align children to the center horizontally with gaps
        for (int i = 0; i < childCount; i++)
        {
            RectTransform childRt = rt.GetChild(i).GetComponent<RectTransform>();

            if (childRt != null)
            {
                // Set the anchored position of the child to align it to the center horizontally
                Vector2 anchoredPosition = childRt.anchoredPosition;
                anchoredPosition.x = startX + childRt.rect.width / 2f;
                childRt.anchoredPosition = anchoredPosition;

                // Update the starting position for the next child
                startX += childRt.rect.width + gap;
            }
        }
    }
}
