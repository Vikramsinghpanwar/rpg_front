using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropTarget : MonoBehaviour, IDropHandler
{
    public List<RectTransform> dragableItemPrefab;  // Reference to the prefab of the draggable item
    public RectTransform parentObject;
    public int dropdownNumber = 0;
    public List<Vector2> coinVectorPos;
    public ManualHorizontalLayout manualLayoutGroupRef;

    private void Start()
    {
        //manualLayoutGroupRef = gameObject.GetComponent<ManualHorizontalLayout>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        Debug.LogWarning("item dropped on Me");
        Debug.Log("position of item : " + eventData.position);
        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggableItem != null)
        {
            RectTransform dropTargetTransform = GetComponent<RectTransform>();
            draggableItem.transform.SetParent(dropTargetTransform);
            draggableItem.transform.position = dropTargetTransform.position;
            manualLayoutGroupRef.ArrangeChildren();
        }

    }



}