using UnityEngine;
using UnityEngine.UI;
public class Spot : MonoBehaviour
{
    public int spotNo;

    void Start()
    {
        if (GetComponent<GridLayoutGroup>() == null)
        {
            GridLayoutGroup g = gameObject.AddComponent<GridLayoutGroup>();
            g.childAlignment = TextAnchor.MiddleCenter;
            g.cellSize = new Vector2(20, 20);
        }
    }
}