using GameEnumerations;
using UnityEngine;

public abstract class GameResourcesTemplate : MonoBehaviour
{
    public event DataHolder.Notification StartTheGame;
    public event DataHolder.Notification PauseTheGame;
    public event DataHolder.Notification ResumeTheGame;

    public bool GameOn { get; protected set; } = false;

    [SerializeField] private bool _turnOnStartTimer = true;
    protected int _blueScore { get; private set; } = 0;
    protected int _redScore { get; private set; } = 0;   
    protected bool _gameStarted = false, _gameOver = false;   
    private GeneralUIResources _res;

    protected virtual void Awake()
    {
        _res = GetComponent<GeneralUIResources>();
        _res.Timer.StartGame += StartGame;
        _res.EndRoundPanel.RestartLevel += ResetLevel;
        _res.PauseButton.PauseGame += PauseGame;
        _res.PauseMenu.ResumeGame += ResumeGame;

        DisplayScore();
        StartTimer();
    }

    protected void UpdateScore(GameObject player, GameResult result)
    {
        if (player != null)
        {
            if (player.name == "BluePlayer")
            {
                if (result == GameResult.Lose)
                    _redScore++;
                else if (result == GameResult.Win)
                    _blueScore++;
            }
            else if (player.name == "RedPlayer")
            {
                if (result == GameResult.Lose)
                    _blueScore++;
                else if (result == GameResult.Win)
                    _redScore++;
            }
        }
        DisplayScore();
    }

    protected void DisplayScore()
    {
        _res.FirstScore.text = _blueScore.ToString();
        _res.SecondScore.text = _redScore.ToString();
    }

    private void StartTimer()
    {
        if (_turnOnStartTimer)
            _res.Timer.StartTimer();
        else 
            StartGame();
    }

    protected void OpenEndRoundPanel()
    {
        _res.EndRoundPanel.gameObject.SetActive(true);
    }

    protected virtual void ResetLevel()
    {
        _gameOver = false;
        _gameStarted = false;

        DisplayScore();
        StartTimer();
    }

    protected virtual void PauseGame(PauseType pauseType)
    {
        if (pauseType == PauseType.ManualPause)
        {
            if (_gameStarted == false)
                _res.Timer.StopTimer();

            _res.PauseMenu.gameObject.SetActive(true);
        }
        else if (pauseType == PauseType.EndRound)
            _gameOver = true;

        PauseTheGame?.Invoke();
        GameOn = false;
    }

    protected virtual void ResumeGame()
    {
        if (_gameStarted == false)
            StartTimer();
        else if (_gameOver == false)
        {
            GameOn = true;
            ResumeTheGame?.Invoke();
        }              
    }

    protected virtual void StartGame()
    {
        _gameStarted = true;
        GameOn = true;
        StartTheGame?.Invoke();
    }

    public void ResetScore()
    {
        _blueScore = 0;
        _redScore = 0;
    }  
}
