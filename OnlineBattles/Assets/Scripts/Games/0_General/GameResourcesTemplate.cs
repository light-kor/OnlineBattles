using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameResourcesTemplate : MonoBehaviour
{
    [SerializeField] private bool _turnOnStartTimer = true;
    protected static int _blueScore { get; private set; } = 0;
    protected static int _redScore { get; private set; } = 0;
    public bool GameOn { get; protected set; } = false;
    protected bool _gameStarted = false, _gameOver = false;   
    private GeneralUIResources _res;

    protected virtual void Awake()
    {
        _res = GetComponent<GeneralUIResources>();
        _res.Timer.StartGame += StartTheGame;
        _res.EndRoundPanel.RestartLevel += RestartLevel;
        _res.PauseButton.PauseGame += PauseTheGame;
        _res.PauseMenu.ResumeGame += ResumeTheGame;

        DisplayScore();
        StartTimer();
    }

    protected void UpdateScore(GameObject player, Result result)
    {
        if (player != null)
        {
            if (player.name == "BluePlayer")
            {
                if (result == Result.Lose)
                    _redScore++;
                else if (result == Result.Win)
                    _blueScore++;
            }
            else if (player.name == "RedPlayer")
            {
                if (result == Result.Lose)
                    _blueScore++;
                else if (result == Result.Win)
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
            StartTheGame();
    }

    protected void OpenEndRoundPanel()
    {
        _res.EndRoundPanel.gameObject.SetActive(true);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    protected virtual void PauseTheGame(PauseType pauseType)
    {
        if (pauseType == PauseType.ManualPause)
        {
            if (_gameStarted == false)
                _res.Timer.StopTimer();

            _res.PauseMenu.gameObject.SetActive(true);
        }
        else if (pauseType == PauseType.EndRound)
            _gameOver = true;

        GameOn = false;
    }

    protected virtual void ResumeTheGame()
    {
        if (_gameStarted == false)
            StartTimer();
        else if (_gameOver == false)
            GameOn = true;
    }

    protected virtual void StartTheGame()
    {
        _gameStarted = true;
        GameOn = true;
    }

    public static void ResetScore()
    {
        _blueScore = 0;
        _redScore = 0;
    }

    public enum PlayerType
    {
        Null,
        BluePlayer,
        RedPlayer
    }

    public enum PauseType
    {
        Null,
        ManualPause,
        EndRound
    }

    public enum Result
    {       
        Win,
        Lose,
        Draw
    }

    public enum ControlType
    {
        Local,
        Remote,
        Broadcast
    }
}
