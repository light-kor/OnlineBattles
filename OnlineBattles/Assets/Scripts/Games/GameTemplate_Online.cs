using UnityEngine;

public class GameTemplate_Online : MonoBehaviour
{
    protected const int _delay = 3125 * 100; // 31.25 ms для интерполяции
    protected bool _finishTheGame = false;
    protected bool _gameOn = true;
    protected string[] frame = null, frame2 = null;
    private string _gameType = null;
    private string _endStatus = null;

    protected void BaseStart(string type)
    {
        Network.EndOfGame += FinishTheGame;
        LeaveGameButton.WantLeaveTheGame += GiveUp;
        DataHolder.StartMenuView = "WifiClient";
        _gameType = type;

        if (_gameType == "udp")
        {
            Network.CreateUDP();
            DataHolder.MessageUDPget.Clear();
            DataHolder.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
        }

        if (DataHolder.GameType == "Multiplayer")
            DataHolder.ClientTCP.SendMessage("start");    
    }

    protected virtual void Update()
    {
        Network.ConnectionLifeSupport();

        if (_finishTheGame)
        {
            CloseAll();
            _finishTheGame = false;
            EndOfGame();
        }

        SendAllChanges();
    }

    private void FinishTheGame(string Status)
    {
        _endStatus = Status;
        _finishTheGame = true;
    }

    /// <summary>
    /// Завершение игры. Вывод уведомления.
    /// </summary>
    protected void EndOfGame()
    {
        if (_endStatus == "drawn")
            NotificationPanels.NP.AddNotificationToQueue("Ничья", 4);
        else if (_endStatus == "win")
            NotificationPanels.NP.AddNotificationToQueue("Вы победили", 4);
        else if (_endStatus == "lose")
            NotificationPanels.NP.AddNotificationToQueue("Вы проиграли", 4);
    }

    private void GiveUp()
    {
        DataHolder.ClientTCP.SendMessage("GiveUp");
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

        if (_gameType == "udp")
        {
            Network.CloseUdpConnection();
        }
    }

    /// <summary>
    /// Регулярная отправка сообщений. Переопределяется в наследуемых классах.
    /// </summary>
    public virtual void SendAllChanges()
    {

    }

    private void OnDestroy()
    {
        Network.EndOfGame -= FinishTheGame;
        LeaveGameButton.WantLeaveTheGame -= GiveUp;
    }
}
