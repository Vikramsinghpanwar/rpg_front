using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Baccarat
{
    public class TouchPos : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            ManagerBac.Instance.RegisterTouch(eventData.position);
        }
    }

}
