using UnityEngine;

public class GeneralUIResources : MonoBehaviour
{
    [SerializeField] private StartScreenTimer _timer;
    [SerializeField] private Score _gameScore;
    [SerializeField] private EndRoundPanel _endRound;
    [SerializeField] private PauseMenu _pausePanel;
    [SerializeField] private PauseButton _pause;

    public StartScreenTimer Timer => _timer;
    public Score GameScore => _gameScore;
    public EndRoundPanel EndRound => _endRound;
    public PauseMenu PausePanel => _pausePanel;
    public PauseButton Pause => _pause;
}
