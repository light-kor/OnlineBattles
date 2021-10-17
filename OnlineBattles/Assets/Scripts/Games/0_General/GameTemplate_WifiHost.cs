using UnityEngine;

public abstract class GameTemplate_WifiHost : MonoBehaviour
{
    protected bool _gameOn = true;
    protected string[] _messages;
    private string _earlyOpponentStatus = null;
    private DataHolder.ConnectType _gameType;

    protected void BaseStart(DataHolder.ConnectType type)
    {
        WifiServer_Host.OpponentGaveUp += OpponentGiveUp;
        PauseMenu.WantLeaveTheGame += IGiveUp;
        _gameType = type;

        if (_gameType == DataHolder.ConnectType.UDP)
        {
            Network.UDPMessagesBig.Clear();
            Network.CreateUDP();
            Network.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес   
            InvokeRepeating("SendAllChanges", 0f, WifiServer_Host.UpdateRate);
        }
    }

    protected virtual void Update()
    {        
        if (_earlyOpponentStatus != null)
        {
            CloseAll();
            EndOfGame(_earlyOpponentStatus);
            _earlyOpponentStatus = null;
        }
    }

    /// <summary>
    /// Завершение игры. Вывод уведомления и отправка инфы противнику.
    /// </summary>
    /// <param name="opponentStatus">Статус противника.</param>
    protected static void EndOfGame(string opponentStatus)
    {
        WifiServer_Host.SendTcpMessage(opponentStatus);
        string notifText = null;
        if (opponentStatus == "drawn")
            notifText = "Ничья";
        else if (opponentStatus == "lose")
            notifText = "Вы победили";
        else if (opponentStatus == "win")
            notifText = "Вы проиграли";

        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    }

    private void IGiveUp()
    {
        _earlyOpponentStatus = "win";
    }

    private void OpponentGiveUp()
    {
        _earlyOpponentStatus = "lose";
    }

    protected void CloseAll()
    {
        _gameOn = false;

        if (_gameType == DataHolder.ConnectType.UDP)
        {
            CancelInvoke();
            Network.CloseUdpConnection();
        }
    }

    private void OnDestroy()
    {
        WifiServer_Host.OpponentGaveUp -= OpponentGiveUp;
        PauseMenu.WantLeaveTheGame -= IGiveUp;
    }
}
