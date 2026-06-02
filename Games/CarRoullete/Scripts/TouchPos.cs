using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace CarRoulette
{
    public class TouchPos : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            CarRoulleteManager.Instance.RegisterTouch(eventData.position);
        }
    }

}
