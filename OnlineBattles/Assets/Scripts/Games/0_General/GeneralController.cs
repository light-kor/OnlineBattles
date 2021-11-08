using GameEnumerations;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

public abstract class GeneralController : MonoBehaviour
{
    public static event UnityAction<string> EndOfGame;
    public static event UnityAction OpponentLeftTheGame;
    public static event UnityAction RemotePause;
    public static event UnityAction RemoteResume;

    public event UnityAction StartTheGame;
    public event UnityAction PauseTheGame;
    public event UnityAction ResumeTheGame;
    public event UnityAction ResetTheGame;
    public event UnityAction NewMessageReceived;  
  
    public bool GameOn { get; private set; } = false;
    public int GameMessagesCount => _gameMessages.Count;
    public NumberFormatInfo NumFormat = new CultureInfo("en-US").NumberFormat;

    private List<string[]> _gameControlMessages = new List<string[]>();
    private List<string[]> _gameMessages = new List<string[]>();
    private bool _timerIsDone = false;
    private bool _checkingResults = false;
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

        _res.Timer.TryStartTimer();
    }   

    protected virtual void Update()
    {
        MessageHandler();
    }

    private void MessageHandler()
    {
        if (_gameControlMessages.Count > 0)
        {
            string[] mes = DataHolder.UseAndDeleteFirstListMessage(_gameControlMessages);

            switch (mes[0])
            {
                case "UpdateScore":
                    _res.GameScore.UpdateOnlineScore(mes);
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
                    _res.DeactivateOpponentPausePanel();
                    ResumeGame();
                    break;

                default:
                    _gameMessages.Add(mes);
                    NewMessageReceived?.Invoke();
                    break;
            }
        }
    }

    private void ResetLevel()
    {
        _checkingResults = false;
        ResetTheGame?.Invoke();
    }

    private void PauseGame(PauseTypes pauseType)
    {
        GameOn = false;
        _res.Timer.StopTimer();
        PauseTheGame?.Invoke();

        if (pauseType == PauseTypes.ManualPause)
        {
            _res.PausePanel.gameObject.SetActive(true);
            RemotePause?.Invoke();
        }
        else if (pauseType == PauseTypes.RemotePause)
            _res.ActivateOpponentPausePanel();
    }

    private void EndRound()
    {
        _timerIsDone = false;
        GameOn = false;       
        PauseTheGame?.Invoke();

        _res.Timer.TryStartTimer();
    }

    public bool StartCheckingResults()
    {
        if (DataHolder.GameType != GameTypes.WifiClient && DataHolder.GameType != GameTypes.Multiplayer)
        {
            if (_checkingResults == false)
            {
                _checkingResults = true;
                return true;
            }
            else return false;
        }
        else return false;
    }

    protected void UpdateScoreAndCheckGameState(PlayerTypes player, GameResults result, int winScore, bool setRoundPause)
    {
        _res.GameScore.UpdateAndTrySendScore(player, result);
        CheckAndRoundOrEndGame(winScore, setRoundPause);
    }
   
    private void CheckAndRoundOrEndGame(int winScore, bool setRoundPause)
    {
        _res.GameScore.GetScore(out int blueScore, out int redScore);
        bool result = _res.EndGame.CheckEndGame(winScore, blueScore, redScore);

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
            _res.Timer.TryStartTimer();
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
        _res.Timer.RestartLevel -= ResetLevel;
        _res.Pause.PauseGame -= PauseGame;
        _res.PausePanel.ResumeGame -= ManualResumeGame;

        if (DataHolder.GameType == GameTypes.WifiHost)
            WifiServer_Host.NewGameControlMessage -= NewGameControlMessage;
        else if (DataHolder.GameType == GameTypes.WifiClient || DataHolder.GameType == GameTypes.Multiplayer)
            Network.NewGameControlMessage -= NewGameControlMessage;
    }
}
