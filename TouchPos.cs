using UnityEngine;
using UnityEngine.EventSystems;
namespace WingoLottery
{
    public class TouchPos : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            GameRuleManager.Instance.RegisterTouch(eventData.position);
        }
    }

}
