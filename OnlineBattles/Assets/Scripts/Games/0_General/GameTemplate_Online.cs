﻿using GameEnumerations;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameTemplate_Online : MonoBehaviour
{
    protected const float _delay = 0.035f; // 35 ms для интерполяции, если кадры идут каждые 33 ms
    protected string[] _frame = null, _frame2 = null; //TODO: Это надо будет удалить нафиг
    private ConnectTypes _connectType;

    protected void BaseStart(ConnectTypes type)
    {
        GeneralController.OpponentLeftTheGame += OpponentLeftTheGame;
        GeneralController.EndOfGame += FinishTheGame;
        GeneralController.RemotePause += SendPauseRequest;
        GeneralController.RemoteResume += SendResumeRequest;
        PauseMenu.LeaveTheGame += LeaveTheGame;
        _connectType = type;

        if (_connectType == ConnectTypes.UDP)
        {
            Network.CreateUDP();
            Network.UDPMessagesBig.Clear();
            Network.ClientUDP.SendMessage("sss"); // Именно UDP сообщение, чтоб сервер получил удалённый адрес
        }

        if (DataHolder.GameType == GameTypes.Multiplayer)
            Network.ClientTCP.SendMessage("start");    
    }

    //protected virtual void Update()
    //{
    //    Network.ConnectionLifeSupport();  
    //}

    private void FinishTheGame(string status)
    {
        CloseAll();
        EndOfGame(status);
    }

    /// <summary>
    /// Завершение игры. Вывод уведомления.
    /// </summary>
    private void EndOfGame(string status)
    {
        string notifText = null;
        if (status == "Draw")
            notifText = "Ничья";
        else if (status == "Win")
            notifText = "Вы победили";
        else if (status == "Lose")
            notifText = "Вы проиграли";

        new Notification(notifText, Notification.ButtonTypes.MenuButton);
    }    

    private void LeaveTheGame()
    {
        CloseAll();
        Network.ClientTCP.SendMessage("LeftTheGame");
        SceneManager.LoadScene("mainMenu");
    }

    private void SendPauseRequest()
    {
        Network.ClientTCP.SendMessage("Pause");
    }

    private void SendResumeRequest()
    {
        Network.ClientTCP.SendMessage("Resume");
    }

    private void OpponentLeftTheGame()
    {
        CloseAll();
        new Notification("Противник сдался", Notification.ButtonTypes.MenuButton);
    }

    private void CloseAll()
    {
        if (_connectType == ConnectTypes.UDP)
        {
            Network.CloseUdpConnection();
        }
    }
   
    private void OnDestroy()
    {
        GeneralController.OpponentLeftTheGame -= OpponentLeftTheGame;
        GeneralController.EndOfGame -= FinishTheGame;
        GeneralController.RemotePause -= SendPauseRequest;
        GeneralController.RemoteResume -= SendResumeRequest;
        PauseMenu.LeaveTheGame -= LeaveTheGame;
    }
}
