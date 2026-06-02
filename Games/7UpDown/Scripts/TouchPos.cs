using UnityEngine;
using UnityEngine.EventSystems;
namespace SevenUpDown
{
    public class TouchPos : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            Manager7UD.Instance.RegisterTouch(eventData.position);
        }
    }

}
