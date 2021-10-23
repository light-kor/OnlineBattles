using GameEnumerations;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public abstract class GeneralController : MonoBehaviour
{
    public static event DataHolder.StringEvent EndOfGame;
    public static event DataHolder.Notification OpponentLeftTheGame;
    public static event DataHolder.Notification RemotePause;
    public static event DataHolder.Notification RemoteResume;

    public event DataHolder.Notification StartTheGame;
    public event DataHolder.Notification PauseTheGame;
    public event DataHolder.Notification ResumeTheGame;
    
    [SerializeField] private bool _turnOnStartTimer = true;
  
    public bool GameOn { get; private set; } = false;
    public int GameMessagesCount => _gameMessages.Count;
    public NumberFormatInfo NumFormat = new CultureInfo("en-US").NumberFormat;

    private List<string[]> _gameControlMessages = new List<string[]>();
    private List<string[]> _gameMessages = new List<string[]>();
    private bool _timerIsDone = false;
    protected GeneralUIResources _res { get; private set; }

    protected virtual void Awake()
    {
        _res = GetComponent<GeneralUIResources>();
        _res.Timer.StartGame += StartGame;
        _res.Timer.RestartLevel += ResetLevel;
        _res.Pause.PauseGame += PauseGame;
        _res.PausePanel.ResumeGame += ManualResumeGame;

        if (DataHolder.GameType == GameTypes.WifiHost)
            WifiServer_Host.NewGameControlMessage += NewGameControlMessage;       
        else if (DataHolder.GameType == GameTypes.WifiClient || DataHolder.GameType == GameTypes.Multiplayer)
            Network.NewGameControlMessage += NewGameControlMessage;

        StartTimer();
    }   

    protected virtual void Update()
    {
        if (_gameControlMessages.Count > 0)
        {
            string[] mes = DataHolder.UseAndDeleteFirstListMessage(_gameControlMessages);

            switch (mes[0])
            {
                case "UpdateScore":
                    UpdateOnlineScore(mes);
                    break;

                case "EndGame":
                    PauseGame(PauseTypes.BackgroundPause);
                    EndOfGame?.Invoke(mes[1]);
                    break;
                    
                case "EndRound":
                    EndRound();
                    break;

                case "LeftTheGame":
                    OpponentLeftTheGame?.Invoke();
                    break;              
               
                case "Pause":
                    PauseGame(PauseTypes.RemotePause);
                    break;

                case "Resume":
                    OpponentPausePanelSwitch(false);
                    ResumeGame();
                    break;

                default:
                    _gameMessages.Add(mes);
                    break;
            }
        }
    }   

    protected virtual void ResetLevel()
    {
        
    }   

    protected void PauseGame(PauseTypes pauseType)
    {
        GameOn = false;
        TryStopTimer();
        PauseTheGame?.Invoke();

        if (pauseType == PauseTypes.ManualPause)
        {           
            _res.PausePanel.gameObject.SetActive(true);
            RemotePause?.Invoke();
        }
        else if (pauseType == PauseTypes.RemotePause)
            OpponentPausePanelSwitch(true);
    }


    protected void EndRound()
    {
        _timerIsDone = false;
        GameOn = false;       
        PauseTheGame?.Invoke();

        StartTimer();
    }

    protected void UpdateScoreAndCheckGameState(PlayerTypes player, GameResults result, int winScore, bool setRoundPause)
    {
        UpdateAndTrySendScore(player, result);
        CheckAndRoundOrEndGame(winScore, setRoundPause);
    }

    private void UpdateAndTrySendScore(PlayerTypes player, GameResults result)
    {
        _res.GameScore.UpdateScore(player, result);

        if (DataHolder.GameType == GameTypes.WifiHost)
            GameTemplate_WifiHost.SendScore(player, result);
    }

    private void CheckAndRoundOrEndGame(int winScore, bool setRoundPause)
    {
        bool result = _res.GameScore.CheckEndGame(winScore, DataHolder.GameType);
        if (setRoundPause)
        {
            if (result == false)
            {
                EndRound();

                if (DataHolder.GameType == GameTypes.WifiHost)
                    WifiServer_Host.Opponent.SendTcpMessage("EndRound");
            }
            else
                PauseGame(PauseTypes.BackgroundPause);
        }       
    }

    private void ManualResumeGame()
    {
        RemoteResume?.Invoke();
        ResumeGame();
    }

    private void ResumeGame()
    {
        if (_timerIsDone == false)
            StartTimer();
        else
        {
            GameOn = true;
            ResumeTheGame?.Invoke();
        }
    }

    private void StartGame()
    {
        _timerIsDone = true;
        GameOn = true;
        StartTheGame?.Invoke();
    }

    private void UpdateOnlineScore(string[] message)
    {
        PlayerTypes player = DataHolder.ParseEnum<PlayerTypes>(message[1]);
        GameResults result = DataHolder.ParseEnum<GameResults>(message[2]);
        _res.GameScore.UpdateScore(player, result);
    }

    private void StartTimer()
    {        
        if (_turnOnStartTimer)
            _res.Timer.StartTimer();
        else
            StartGame();
    }

    private void TryStopTimer()
    {
        _res.Timer.StopTimer();
    }

    private void NewGameControlMessage(string[] message)
    {
        _gameControlMessages.Add(message);
    }

    public string[] UseAndDeleteGameMessage()
    {
        return DataHolder.UseAndDeleteFirstListMessage(_gameMessages);
    }

    private void OpponentPausePanelSwitch(bool setActive)
    {
        _res.OpponentPaused.gameObject.SetActive(setActive);
    }

    private void OnDestroy()
    {
        _res = GetComponent<GeneralUIResources>();
        _res.Timer.StartGame -= StartGame;
        _res.Timer.RestartLevel -= ResetLevel;
        _res.Pause.PauseGame -= PauseGame;
        _res.PausePanel.ResumeGame -= ManualResumeGame;

        if (DataHolder.GameType == GameTypes.WifiHost)
            WifiServer_Host.NewGameControlMessage -= NewGameControlMessage;
        else if (DataHolder.GameType == GameTypes.WifiClient || DataHolder.GameType == GameTypes.Multiplayer)
            Network.NewGameControlMessage -= NewGameControlMessage;
    }
}
