using UnityEngine;

public class GameTemplate_WifiHost : MonoBehaviour
{
    public static event DataHolder.GameNotification ShowGameNotification;
    private bool _finishTheGame = false;
    private bool _fastLeave = false;
    protected bool _gameOn = true;
    protected string[] messege;

    protected virtual void Start()
    {
        WifiServer_Host.OpponentLeaveTheGame += FinishGame;
        LeaveGameButton.WantLeaveTheGame += GiveUp;                
    }

    protected void StartUdpConnection()
    {
        DataHolder.MessageUDPget.Clear();
        Network.CreateUDP();
        DataHolder.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес       
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

    public static void EndOfGame(string opponentStatus)
    {
        WifiServer_Host.SendTcpMessage(opponentStatus);
        if (opponentStatus == "drawn")
        {
            ShowGameNotification?.Invoke("Игра завершена\r\ndrawn", 4);
        }
        else if (opponentStatus == "lose")
        {
            ShowGameNotification?.Invoke("Игра завершена\r\nwin", 4);
        }
        else if (opponentStatus == "win")
        {
            ShowGameNotification?.Invoke("Игра завершена\r\nlose", 4);
        }
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

    private void OnDestroy()
    {
        WifiServer_Host.OpponentLeaveTheGame -= FinishGame;
        LeaveGameButton.WantLeaveTheGame -= GiveUp;
    }
}
