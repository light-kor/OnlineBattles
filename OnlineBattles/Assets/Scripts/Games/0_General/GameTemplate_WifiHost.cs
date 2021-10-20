using GameEnumerations;
using UnityEngine;

public abstract class GameTemplate_WifiHost : MonoBehaviour
{
    protected bool _gameOn = true;
    protected string[] _messages;
    private string _earlyOpponentStatus = null;
    private ConnectTypes _gameType;

    protected void BaseStart(ConnectTypes type)
    {
        WifiServer_Host.OpponentGaveUp += OpponentGiveUp;
        PauseMenu.WantLeaveTheGame += IGiveUp;

        _gameType = type;

        if (_gameType == ConnectTypes.UDP)
        {
            Network.UDPMessagesBig.Clear();
            Network.CreateUDP();
            Network.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес   
            InvokeRepeating("SendFramesUDP", 0f, WifiServer_Host.UpdateRate);
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

    public static void SendScore(PlayerTypes player, GameResults result, bool setPause)
    {
        if (WifiServer_Host.Opponent != null)
            WifiServer_Host.SendTcpMessage($"UpdateScore {player} {result} {setPause}");
    }

    private void SendResumeRequest()
    {
        WifiServer_Host.SendTcpMessage("Resume");
    }

    protected void CloseAll()
    {
        _gameOn = false;

        if (_gameType == ConnectTypes.UDP)
        {
            CancelInvoke();
            Network.CloseUdpConnection();
        }
    }

    protected virtual void SendFramesUDP()
    {

    }

    private void OnDestroy()
    {
        WifiServer_Host.OpponentGaveUp -= OpponentGiveUp;
        PauseMenu.WantLeaveTheGame -= IGiveUp;
        //PauseButton.SendPauseGame -= SendPauseRequest;
        //ExitMenu.SendResumeGame -= SendResumeRequest;
    }
}
