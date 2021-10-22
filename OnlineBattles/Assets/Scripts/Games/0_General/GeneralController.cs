using GameEnumerations;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public abstract class GeneralController : MonoBehaviour
{
    public static event DataHolder.StringEvent EndOfGame;
    public static event DataHolder.Notification OpponentLeftTheGame;

    public event DataHolder.Notification StartTheGame;
    public event DataHolder.Notification PauseTheGame;
    public event DataHolder.Notification ResumeTheGame;
    
    [SerializeField] private bool _turnOnStartTimer = true;
  
    public bool GameOn { get; private set; } = false;
    public int GameMessagesCount => _gameMessages.Count;
    public NumberFormatInfo NumFormat = new CultureInfo("en-US").NumberFormat;

    private List<string[]> _gameControlMessages = new List<string[]>();
    private List<string[]> _gameMessages = new List<string[]>();
    private bool _gameStarted = false;
    private bool _gameOver = false;
    protected GeneralUIResources _res { get; private set; }

    protected virtual void Awake()
    {
        _res = GetComponent<GeneralUIResources>();
        _res.Timer.StartGame += StartGame;
        _res.EndRound.RestartLevel += ResetLevel;
        _res.Pause.PauseGame += PauseGame;
        _res.PausePanel.ResumeGame += ResumeGame;

        if (DataHolder.GameType == GameTypes.WifiHost)
        {
            WifiServer_Host.NewGameControlMessage += NewGameControlMessage;
            GameTemplate_WifiHost.BackgroundPause += PauseGame;
        }          
        else if (DataHolder.GameType == GameTypes.WifiClient || DataHolder.GameType == GameTypes.Multiplayer)
        {
            Network.NewGameControlMessage += NewGameControlMessage;
            GameTemplate_Online.BackgroundPause += PauseGame;
        }           

        StartTimer();
    }   

    protected virtual void Update()
    {
        if (_gameControlMessages.Count > 0)
        {
            string[] mes = DataHolder.UseAndDeleteFirstListMessage(_gameControlMessages);

            switch (mes[0])
            {
                case "EndGame":
                    EndOfGame?.Invoke(mes[1]);
                    break;

                case "LeftTheGame":
                    OpponentLeftTheGame?.Invoke();
                    break;

                case "NextRound":
                    StartNextRound();
                    break;

                case "UpdateScore":
                    UpdateOnlineScore(mes);
                    break;

                case "Resume":

                    break;

                default:
                    _gameMessages.Add(mes);
                    break;
            }
        }
    }   

    protected virtual void ResetLevel()
    {
        _gameOver = false;
        _gameStarted = false;

        StartTimer();
    }   

    protected void PauseGame(PauseTypes pauseType)
    {
        if (pauseType == PauseTypes.ManualPause)
        {
            if (DataHolder.GameType == GameTypes.Single || DataHolder.GameType == GameTypes.Null)
            {
                if (_gameStarted == false)
                    _res.Timer.StopTimer();

                _res.PausePanel.gameObject.SetActive(true);
            }
            else if (DataHolder.GameType == GameTypes.WifiClient || DataHolder.GameType == GameTypes.WifiHost)
            {
                _res.PausePanel.gameObject.SetActive(true);
                return;
            }
        }
        else if (pauseType == PauseTypes.EndRound)
        {
            _gameOver = true;
            OpenEndRoundPanel();
        }
        else if (pauseType == PauseTypes.BackgroundPause)
        {
            if (_gameStarted == false)
                _res.Timer.StopTimer();
        }

        PauseTheGame?.Invoke();
        GameOn = false;
    }

    protected void UpdateAndTrySendScore(PlayerTypes player, GameResults result)
    {
        _res.GameScore.UpdateScore(player, result);

        if (DataHolder.GameType == GameTypes.WifiHost)
            GameTemplate_WifiHost.SendScore(player, result, true);
    }

    private void ResumeGame()
    {
        if (_gameStarted == false)
            StartTimer();
        else if (_gameOver == false)
        {
            GameOn = true;
            ResumeTheGame?.Invoke();
        }              
    }

    private void StartNextRound()
    {

    }

    private void StartGame()
    {
        _gameStarted = true;
        GameOn = true;
        StartTheGame?.Invoke();
    }

    private void UpdateOnlineScore(string[] message)
    {
        PlayerTypes player = DataHolder.ParseEnum<PlayerTypes>(message[1]);
        GameResults result = DataHolder.ParseEnum<GameResults>(message[2]);
        _res.GameScore.UpdateScore(player, result);

        bool setPause = bool.Parse(message[3]);
        if (setPause)
            PauseGame(PauseTypes.EndRound);
    }

    private void StartTimer()
    {
        if (_turnOnStartTimer)
            _res.Timer.StartTimer();
        else
            StartGame();
    }

    private void OpenEndRoundPanel()
    {
        _res.EndRound.gameObject.SetActive(true);
    }

    private void NewGameControlMessage(string[] message)
    {
        _gameControlMessages.Add(message);
    }

    public string[] UseAndDeleteGameMessage()
    {
        return DataHolder.UseAndDeleteFirstListMessage(_gameMessages);
    }

    private void OnDestroy()
    {
        _res = GetComponent<GeneralUIResources>();
        _res.Timer.StartGame -= StartGame;
        _res.EndRound.RestartLevel -= ResetLevel;
        _res.Pause.PauseGame -= PauseGame;
        _res.PausePanel.ResumeGame -= ResumeGame;

        if (DataHolder.GameType == GameTypes.WifiHost)
        {
            WifiServer_Host.NewGameControlMessage -= NewGameControlMessage;
            GameTemplate_WifiHost.BackgroundPause -= PauseGame;
        }
        else if (DataHolder.GameType == GameTypes.WifiClient || DataHolder.GameType == GameTypes.Multiplayer)
        {
            Network.NewGameControlMessage -= NewGameControlMessage;
            GameTemplate_Online.BackgroundPause -= PauseGame;
        }
    }
}
