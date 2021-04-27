using UnityEngine;

public class GameTemplate_WifiHost : MonoBehaviour
{
    private bool _finishTheGame = false;
    private bool _fastLeave = false;
    protected bool _gameOn = false;
    protected string[] messege;

    protected void BaseStart(string type)
    {
        WifiServer_Host.OpponentLeaveTheGame += FinishGame;
        LeaveGameButton.WantLeaveTheGame += GiveUp;

        if (type == "udp")
        {
            StartUdpConnection();
            InvokeRepeating("SendAllChanges", 0f, WifiServer_Host.UpdateRate);
        }
    }

    protected virtual void Update()
    {        
        if (_finishTheGame)
        {
            _finishTheGame = false;
            CloseAll();
            EndOfGame("lose");           
        }

        if (_fastLeave)
        {
            _fastLeave = false;
            CloseAll();
            EndOfGame("win");
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

    protected static void EndOfGame(string opponentStatus)
    {
        WifiServer_Host.SendTcpMessage(opponentStatus);
        if (opponentStatus == "drawn")
            NotificationPanels.NP.AddNotificationToQueue("Ничья", 4);
        else if (opponentStatus == "lose")
            NotificationPanels.NP.AddNotificationToQueue("Вы победили", 4);
        else if (opponentStatus == "win")
            NotificationPanels.NP.AddNotificationToQueue("Вы проиграли", 4);
    }

    private void GiveUp()
    {
        _fastLeave = true;
    }

    private void FinishGame()
    {
        _finishTheGame = true;
    }

    protected void CloseAll()
    {
        _gameOn = false;
        CancelInvoke();
        Network.CloseUdpConnection();
    }

    public virtual void SendAllChanges()
    {

    }

    private void OnDestroy()
    {
        WifiServer_Host.OpponentLeaveTheGame -= FinishGame;
        LeaveGameButton.WantLeaveTheGame -= GiveUp;
    }
}
