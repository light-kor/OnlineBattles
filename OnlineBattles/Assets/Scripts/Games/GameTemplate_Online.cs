using UnityEngine;

public class GameTemplate_Online : MonoBehaviour
{
    protected const int _delay = 3125 * 100; // 31.25 ms для интерполяции
    protected bool _finishTheGame = false;
    protected bool _gameOn = true;
    protected string[] frame = null, frame2 = null;

    protected virtual void Start()
    {
        Network.EndOfGame += FinishTheGame;
        LeaveGameButton.WantLeaveTheGame += GiveUp;

        Network.CreateUDP();
        DataHolder.MessageUDPget.Clear();

        if (DataHolder.GameType == "Multiplayer")
            DataHolder.ClientTCP.SendMessage("start");

        DataHolder.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
    }

    protected virtual void Update()
    {
        Network.ConnectionLifeSupport();

        if (_finishTheGame)
        {
            DataHolder.StartMenuView = "WifiClient";
            CloseAll();
            _finishTheGame = false;
        }
    }

    private void FinishTheGame()
    {
        _finishTheGame = true;
    }

    private void GiveUp()
    {
        DataHolder.ClientTCP.SendMessage("leave");
    }

    protected bool SplitFramesAndChechTrash()
    {
        frame = DataHolder.MessageUDPget[0].Split(' ');
        frame2 = DataHolder.MessageUDPget[1].Split(' ');

        if (frame[0] != "g")
        {
            DataHolder.MessageUDPget.RemoveAt(0);
            return false;
        }
        else if (frame2[0] != "g")
        {
            DataHolder.MessageUDPget.RemoveAt(1);
            return false;
        }

        return true;
    }

    protected void CloseAll()
    {
        _gameOn = false;
        CancelInvoke(); //TODO: Временное решение для первой игры       
        // Там автоматически после GameOn = false вызовется CloseClient()
        Network.CloseUdpConnection();
    }

    private void OnDestroy()
    {
        Network.EndOfGame -= FinishTheGame;
        LeaveGameButton.WantLeaveTheGame -= GiveUp;
    }
}
