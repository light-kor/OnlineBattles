using UnityEngine;

public class GeneralUIResources : MonoBehaviour
{
    [SerializeField] private StartScreenTimer _timer;
    [SerializeField] private Score _gameScore;
    [SerializeField] private OpponentPause _opponentPause;
    [SerializeField] private PauseMenu _pausePanel;
    [SerializeField] private PauseButton _pause;

    public StartScreenTimer Timer => _timer;
    public Score GameScore => _gameScore;
    public OpponentPause OpponentPaused => _opponentPause;
    public PauseMenu PausePanel => _pausePanel;
    public PauseButton Pause => _pause;
}
