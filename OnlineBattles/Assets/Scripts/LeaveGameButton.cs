using UnityEngine;
using UnityEngine.EventSystems;

public class LeaveGameButton : MonoBehaviour, IPointerClickHandler
{
    public static event DataHolder.Notification WantLeaveTheGame;

    public void OnPointerClick(PointerEventData eventData)
    {
        WantLeaveTheGame?.Invoke();
    }
}
