using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace JhandiMunda
{
    public class TouchPos : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            JhandiMundaGame.Instance.RegisterTouch(eventData.position);
        }
    }

}
