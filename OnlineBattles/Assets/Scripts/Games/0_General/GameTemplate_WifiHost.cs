using UnityEngine;

public class GameTemplate_WifiHost : MonoBehaviour
{
    protected bool _gameOn = true;
    protected string[] messege;
    private string _earlyOpponentStatus = null;
    private GameType _gameType;

    protected void BaseStart(GameType type)
    {
        WifiServer_Host.OpponentGaveUp += OpponentGiveUp;
        LeaveGameButton.WantLeaveTheGame += IGiveUp;
        _gameType = type;

        if (_gameType == GameType.UDP)
        {
            StartUdpConnection();
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

    private void StartUdpConnection()
    {
        DataHolder.MessageUDPget.Clear();
        Network.CreateUDP();
        DataHolder.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес       
    }

    protected bool SplitFramesAndChechTrash()
    {
        messege = DataHolder.MessageUDPget[0].Split(' ');
        if (messege[0] != "g")
        {
            DataHolder.MessageUDPget.RemoveAt(0);
            return false;
        }
        return true;
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

        if (_gameType == GameType.UDP)
        {
            CancelInvoke();
            Network.CloseUdpConnection();
        }
    }

    /// <summary>
    /// Регулярная отправка сообщений. Переопределяется в наследуемых классах.
    /// </summary>
    public virtual void SendAllChanges()
    {

    }

    public enum GameType
    {
        TCP,
        UDP        
    }

    private void OnDestroy()
    {
        WifiServer_Host.OpponentGaveUp -= OpponentGiveUp;
        LeaveGameButton.WantLeaveTheGame -= IGiveUp;
    }
}
