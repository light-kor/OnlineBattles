using GameEnumerations;
using UnityEngine;

public abstract class GameTemplate_Online : MonoBehaviour
{
    protected const int _delay = 3125 * 100; // 31.25 ms для интерполяции
    protected bool _finishTheGame = false;
    protected bool _gameOn = true;
    protected string[] _frame = null, _frame2 = null;
    private ConnectTypes _gameType;
    private string _endStatus = null;

    protected void BaseStart(ConnectTypes type)
    {
        Network.EndOfGame += FinishTheGame;
        PauseMenu.WantLeaveTheGame += GiveUp;
        //PauseButton.SendPauseGame += SendPauseRequest;
        //ExitMenu.SendResumeGame += SendResumeRequest;
        _gameType = type;

        if (_gameType == ConnectTypes.UDP)
        {
            Network.CreateUDP();
            Network.UDPMessagesBig.Clear();
            Network.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
        }

        if (DataHolder.GameType == GameTypes.Multiplayer)
            Network.ClientTCP.SendMessage("start");    
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
        string notifText = null;
        if (_endStatus == "drawn")
            notifText = "Ничья";
        else if (_endStatus == "win")
            notifText = "Вы победили";
        else if (_endStatus == "lose")
            notifText = "Вы проиграли";

        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    }

    private void GiveUp()
    {
        Network.ClientTCP.SendMessage("GiveUp");
    }

    private void SendPauseRequest()
    {
        Network.ClientTCP.SendMessage("Pause");
    }

    private void SendResumeRequest()
    {
        Network.ClientTCP.SendMessage("Resume");
    }

    protected void CloseAll()
    {
        _gameOn = false;

        if (_gameType == ConnectTypes.UDP)
        {
            Network.CloseUdpConnection();
        }
    }
   
    private void OnDestroy()
    {
        Network.EndOfGame -= FinishTheGame;
        PauseMenu.WantLeaveTheGame -= GiveUp;
        //PauseButton.SendPauseGame -= SendPauseRequest;
        //ExitMenu.SendResumeGame -= SendResumeRequest;
    }
}
