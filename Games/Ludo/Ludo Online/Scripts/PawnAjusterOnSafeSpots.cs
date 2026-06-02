using System.Collections.Generic;
using UnityEngine;

public class PawnAjusterOnSafeSpots : MonoBehaviour
{
    private GameObject _topPawn;

    void Start()
    {
        _topPawn = null;
        if (transform.childCount > 0)
            transform.GetChild(0).gameObject.SetActive(false);
    }

    public void AddPawn(GameObject pawn)
    {
        if (pawn == null) return;

        pawn.transform.SetParent(transform, worldPositionStays: false);
        pawn.transform.SetAsLastSibling();
        _topPawn = pawn;

        AdjustPawnSizes();
    }

    public void RemovePawn(GameObject pawn)
    {
        if (pawn == null) return;

        if (pawn.transform.parent == transform)
            pawn.transform.SetParent(null, worldPositionStays: true);

        // restore exited pawn scale
        pawn.transform.localScale = Vector3.one;

        if (_topPawn == pawn)
        {
            if (transform.childCount > 0)
                _topPawn = transform.GetChild(transform.childCount - 1).gameObject;
            else
                _topPawn = null;
        }

        AdjustPawnSizes();
    }

    private void AdjustPawnSizes()
    {
        int count = transform.childCount;
        if (count == 0) return;

        // Decide the scale to use for all pawns
        float scale = (count == 1) ? 1f : 0.8f; // <-- adjust 0.8f to your liking

        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            child.localScale = Vector3.one * scale;
        }

        // Ensure top pawn always gets same scale
        if (_topPawn != null && _topPawn.transform.parent == transform)
            _topPawn.transform.localScale = Vector3.one * scale;
    }
}
