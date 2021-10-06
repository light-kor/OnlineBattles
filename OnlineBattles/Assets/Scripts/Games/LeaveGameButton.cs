using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaveGameButton : MonoBehaviour
{
    public static event DataHolder.Notification WantLeaveTheGame;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(LeaveTheGame);
    }
    public void LeaveTheGame()
    {
        if (DataHolder.GameType == DataHolder.GameTypes.Single || DataHolder.GameType == DataHolder.GameTypes.Null)
            SceneManager.LoadScene(0); //TODO: ����� ������� ���������� � ���� ������ ������
        else
             WantLeaveTheGame?.Invoke();
    }
}
