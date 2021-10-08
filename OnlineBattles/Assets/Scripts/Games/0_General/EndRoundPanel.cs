using UnityEngine;
using UnityEngine.EventSystems;

public class EndRoundPanel : MonoBehaviour, IPointerClickHandler
{
    public event DataHolder.Notification RestartLevel;

    public void OnPointerClick(PointerEventData eventData)
    {
        RestartLevel?.Invoke();
    }
}
