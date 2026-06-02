using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AndarBahar
{
    public class TouchPos : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            AndarBaharGame.Instance.RegisterTouch(eventData.position);
        }
    }

}
