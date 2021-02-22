using UnityEngine;

public class OnlineGameTemplate : MonoBehaviour
{
    protected const int _delay = 100 * 10000; // 100ms для интерполяции
    protected bool _finishTheGame = false;
    protected bool _gameOn = true;

    protected virtual void Start()
    {
        Network.EndOfGame += FinishTheGame;
        Network.CreateUDP();
        DataHolder.MessageUDPget.Clear();
        DataHolder.ClientTCP.SendMessage("start");
        DataHolder.ClientUDP.SendMessage("start", true); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
    }

    protected void BaseOnlineFunctions()
    {
        Network.ConnectionLifeSupport();
    }

    public void FinishTheGame()
    {
        _finishTheGame = true;
    }

    protected void GiveUp()
    {
        DataHolder.ClientTCP.SendMessage("leave");
    }

    protected void CloseAll()
    {
        _gameOn = false;
        CancelInvoke(); //TODO: Временное решение для первой игры
        Network.EndOfGame -= FinishTheGame;

        // Там автоматически после GameOn = false вызовется CloseClient()
        Network.CloseUdpConnection();
    }


    protected void ExitGame()
    {
        if (DataHolder.ClientUDP != null)
            CloseAll();

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
