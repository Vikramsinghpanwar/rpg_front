using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace KingQueen
{
    public class TouchPos : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            ManagerKQ.Instance.RegisterTouch(eventData.position);
        }
    }

}
