using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralResources : MonoBehaviour
{
    [SerializeField] private TMP_Text _firstScore, _secondScore;
    [SerializeField] private StartScreenTimer _timer;
    [SerializeField] private EndRoundPanel _endRoundPanel;

    private void Start()
    {
        _endRoundPanel.RestartLevel += RestartLvl;
    }

    public void UpdateScore(GameObject loserPlayer)
    {
        if (loserPlayer != null)
        {
            if (loserPlayer.name == "BluePlayer")
            {
                DataHolder.EnemyScore++;
            }
            else if (loserPlayer.name == "RedPlayer")
            {
                DataHolder.MyScore++;
            }
        }

        _firstScore.text = DataHolder.MyScore.ToString();
        _secondScore.text = DataHolder.EnemyScore.ToString();
    }

    public void OpenEndRoundPanel()
    {
        _endRoundPanel.gameObject.SetActive(true);
    }

    public void StartTimer(float time, Action setOnComplete)
    {
        _timer.StartTimer(3f, setOnComplete);
    }

    public void RestartLvl()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public enum PlayerType
    {
        Null,
        BluePlayer,
        RedPlayer
    }
}
