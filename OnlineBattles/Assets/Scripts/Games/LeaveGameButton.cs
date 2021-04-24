using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LeaveGameButton : MonoBehaviour, IPointerClickHandler
{
    public static event DataHolder.Notification WantLeaveTheGame;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (DataHolder.GameType == "OnPhone")
            SceneManager.LoadScene("MainMenu"); //TODO: ����� ������� ���������� � ���� ������ ������
        else
             WantLeaveTheGame?.Invoke();
    }
}
