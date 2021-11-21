using GameEnumerations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{   
    [SerializeField] private NameField _nameField;
    [SerializeField] private LevelSelection _levelSelection;

    public const float AnimTime = 0.5f;
    private const int FrameRate = 60;

    private List<string[]> _tcpControlMessages = new List<string[]>();
    private MenuView _menuView;

    private void Start()
    {
        if (DataHolder.AppStarted == false)
            ApplicationStart();
        
        Network.TcpConnectionIsDone += TcpConnectionIsReady;
        Network.NewGameControlMessage += NewControlMessage;   
        _menuView = GetComponent<MenuView>();
    }
   
    private void Update()
    {
        Network.ConnectionLifeSupport();

        if (_tcpControlMessages.Count > 0)
        {
            string[] mes = DataHolder.UseAndDeleteFirstListMessage(_tcpControlMessages);
            if (mes[0] == "S")
            {
                DataHolder.IDInThisGame = Convert.ToInt32(mes[1]);
                DataHolder.LobbyID = Convert.ToInt32(mes[2]);
                _levelSelection.LoadServerLevel();
                NotificationManager.NM.CloseNotification();
            }
            // Значит до этого игрок вылетел, и сейчас может восстановиться в игре
            else if (mes[0] == "goto")
            {
                DataHolder.SelectedServerGame = Convert.ToInt32(mes[1]);
                DataHolder.IDInThisGame = Convert.ToInt32(mes[2]);
                DataHolder.LobbyID = Convert.ToInt32(mes[3]);
                _levelSelection.LoadLevelFromString("lvl" + DataHolder.SelectedServerGame);
            }
            else if (mes[0] == "wifi_go")
            {
                _levelSelection.LoadLevelFromString(mes[1]);
            }
            //else if (mes[0] == "disconnect") //TODO: Почему это вообще здесь? Надо бы перенести в Network
            //{ 
            //    Network.CloseTcpConnection();
            //    _menuView.ChangePanelWithAnimation(_menuView.ActivateMainMenu);
            //    new Notification("Сервер отключён", Notification.ButtonTypes.SimpleClose);
            //}
        }
    }   

    public void SelectSingleGame()
    {
        DataHolder.GameType = GameTypes.Local;
        _menuView.ChangePanelWithAnimation(_menuView.ActivateSingleplayerMenu);
    }

    public void SelectWifiGame()
    {
        if (_nameField.CheckNickNameAvailability() == false) return;

        _menuView.ChangePanelWithAnimation(_menuView.ActivateWifiMenu);
    }

    public void SelectMultiplayerGame()
    {
        new Notification("Сервер недоступен", Notification.ButtonTypes.SimpleClose); //TODO: Временная заглушка

        //if (!CheckNickNameAvailability()) return;

        //DataHolder.GameType = GameTypes.Multiplayer;
        //if (Network.ClientTCP == null)
        //    Network.CreateTCP();
        //else 
        //    TcpConnectionIsReady();
    }

    public void SelectExit()
    {
        Application.Quit();
        //TODO: В мануале пишут, что на ios это может вызвать баги
    }

    private void TcpConnectionIsReady()
    {
        if (DataHolder.GameType == GameTypes.Multiplayer)
            _menuView.ChangePanelWithAnimation(_menuView.ActivateMultiplayerMenu);
    }
   
    private void NewControlMessage(string[] message)
    {
        _tcpControlMessages.Add(message);
    }

    private void ApplicationStart()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = FrameRate;
        DataHolder.GameType = GameTypes.Null; // Костыль для отладки отдельных уровней. Если зайти в игру минуя меню, то будет установлен Local
        DataHolder.AppStarted = true;
    }

    private void OnDestroy()
    {
        Network.TcpConnectionIsDone -= TcpConnectionIsReady;
        Network.NewGameControlMessage -= NewControlMessage;
    }   
}