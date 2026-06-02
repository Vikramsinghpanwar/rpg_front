using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class GetTouchPosition : MonoBehaviour, IPointerDownHandler
{
    public Vector3 touchPos;
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        touchPos = eventData.position;
        Debug.Log("Pointer Detected at : " + touchPos.x + ", " + touchPos.y);
    }
}
