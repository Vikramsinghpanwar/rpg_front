using UnityEngine;
using UnityEngine.EventSystems;
namespace DragonTiger
{
    public class TouchPos : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            ManagerDT.Instance.RegisterTouch(eventData.position);
        }
    }

}
