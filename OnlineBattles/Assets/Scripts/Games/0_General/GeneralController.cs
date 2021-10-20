using GameEnumerations;
using UnityEngine;

public abstract class GeneralController : MonoBehaviour
{
    public event DataHolder.Notification StartTheGame;
    public event DataHolder.Notification PauseTheGame;
    public event DataHolder.Notification ResumeTheGame;
    [SerializeField] private bool _turnOnStartTimer = true;

    public bool GameOn { get; protected set; } = false;   
        
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

        if (DataHolder.GameType == GameTypes.WifiClient)
            Network.UpdateScore += UpdateOnlineScore;

        StartTimer();
    }  
  
    protected void OpenEndRoundPanel()
    {
        _res.EndRound.gameObject.SetActive(true);
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
            _gameOver = true;
            

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

    private void StartGame()
    {
        _gameStarted = true;
        GameOn = true;
        StartTheGame?.Invoke();
    }

    private void UpdateOnlineScore(string message)
    {
        string[] mes = message.Split(' ');

        PlayerTypes player = DataHolder.ParseEnum<PlayerTypes>(mes[1]);
        GameResults result = DataHolder.ParseEnum<GameResults>(mes[2]);
        _res.GameScore.UpdateScore(player, result);

        bool setPause = bool.Parse(mes[3]);
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
}
