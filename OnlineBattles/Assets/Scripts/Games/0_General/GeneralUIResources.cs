using UnityEngine;

public class GeneralUIResources : MonoBehaviour
{
    [SerializeField] private StartScreenTimer _timer;
    [SerializeField] private Score _gameScore;
    [SerializeField] private OpponentPause _opponentPause;
    [SerializeField] private PauseMenu _pausePanel;
    [SerializeField] private PauseButton _pause;
    [SerializeField] private EndRoundFlash _flashPanel;
    [SerializeField] private EndGameChecker _endGameChecker;

    public StartScreenTimer Timer => _timer;
    public Score GameScore => _gameScore;
    public PauseMenu PausePanel => _pausePanel;
    public PauseButton Pause => _pause;
    public EndGameChecker EndGame => _endGameChecker;

    public void EndRoundFlashAnimation()
    {
        _flashPanel.FlashAnimation();
    }

    public void ActivateOpponentPausePanel()
    {
        _opponentPause.gameObject.SetActive(true);
    }

    public void DeactivateOpponentPausePanel()
    {
        _opponentPause.gameObject.SetActive(false);
    }
}
